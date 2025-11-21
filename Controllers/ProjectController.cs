//using System.ComponentModel.DataAnnotations;
using ERP_Proflipper_ProjectService;
using ERP_Proflipper_ProjectService.Models;
using ERP_Proflipper_ProjectService.Repositories.Interface;
using ERP_Proflipper_ProjectService.Repositories.Ports;
using ERP_Proflipper_ProjectService.Services;
using ERP_Proflipper_ProjectService.Models;
using FluentResults;
using FluentValidation;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Data;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;



namespace ERP_Proflipper_WorkspaceService.Controllers
{
    public class ProjectController : Controller
    {
        private readonly HttpClient _httpClient = new();
        private readonly IProjectRepository _projectRepository;
        private readonly ProjectService _projectService;
        private readonly IConfiguration _config;
        private ILogger<ProjectController> _logger;

        public ProjectController(ILogger<ProjectController> logger, IConfiguration config, IProjectRepository projectRepository, ProjectService projectService)
        {
            _logger = logger;
            _config = config;
            _projectRepository = projectRepository;
            _projectService = projectService;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        //[Authorize("OnlyForPM")] //TODO general director also can make it
        [Route("/project/create")]
        public async Task<IActionResult> CreateProject()
        {
            Project project = new Project();

            var id = await _projectService.CreateProjectInDB(project); //get project id when it is already in db
            

            _logger.LogInformation($"New project. ID: {id}");

            return Ok(Json(id)); //return id
        }

        [HttpPut]
        //[Authorize("OnlyForPM")]
        [Route("/project/edit")]
        public async Task<StatusCodeResult> EditProjectAsync()
        {
            var project = await Request.ReadFromJsonAsync<Project>();
            _logger.LogInformation($"{project.CreatedAt}");

            _logger.LogInformation($"Project by ID: {project.Id} has been edited");

            await _projectService.EditProjectAsync(project, null);

            return Ok();
        }

        [HttpPost]
        //[Authorize("OnlyForPM")]
        [Route("/project/to-pm-approve")]
        public async Task<IActionResult> SendToApproveWithOpenAccess()
        {
            var project = await Request.ReadFromJsonAsync<Project>();
            if (project is null) return BadRequest();
            //if (project.Rules.Any(x => x.RoleName == "ProjectManager" && (!x.CanWrite || !x.CanRead))) return StatusCode(401); //check for rules


            await _projectService.SendToApproveWithOpenAccess(project);

            _logger.LogInformation($"Project: {project.Id} sending to approve");

            var comment = JsonSerializer.Deserialize<JsonElement>(project.PMCardJson).GetProperty("Comment").ToString(); //required field
            var content = CreateContentWithURI(comment, $"ProjectsAndDeals/projectCard?id={project.Id}");
            var result = await _projectService.NotificateAsync("OlegAss", content);
            
            return result.IsSuccess ? Ok() : BadRequest(result.Errors);
        }


        [HttpPut]
        //[Authorize("OnlyForPM")]
        [Route("/projects/disapprove-project/{projectId}/{role}/{userLogin}")]
        public async Task<IActionResult> DisapproveProject(string projectId, string role, string userLogin)
        {
            if (projectId is null) return BadRequest();
            var project = await _projectRepository.GetProjectByIdAsync(projectId);


            _logger.LogInformation($"Project: {project.Id} sending to archive");

            project.IsArchived = true;
            await _projectService.EditPropertiesAsync(role, "Archived", userLogin, project);

            await _projectService.EditProjectAsync(project, null); //null must be a role when we will deploy or test with many roles

            return Ok();
        }


        [HttpPost]
        //[Authorize("OnlyForPM")]
        [Route("/projects/finalize-project/{projectId}/{role}/{userLogin}")] 
        public async Task<IActionResult> ToFinalizeProject(string projectId, string role, string userLogin)
        {
            if (projectId is null || role is null || userLogin is null) return BadRequest();
            var project = await _projectRepository.GetProjectByIdAsync(projectId);//get project

            string? message = (await new StreamReader(Request.Body).ReadToEndAsync()); //read message from json
            if (message is null) return BadRequest();


            _logger.LogInformation($"Project {projectId} has been sending to finalize by {role}");


            await _projectService.EditPropertiesAsync(role, "Finalize", userLogin, project); 

            var content = CreateContentWithURI(message, $"ProjectsAndDeals/projectCard?id={project.Id}");
            var result = await _projectService.NotificateAsync("OlegAss", content);

            return result.IsSuccess ? Ok() : BadRequest(result.Errors);
        }


        [HttpPost]
        [Route("/projects/to-all-approve/{projectId}/{role}/{userLogin}")] 
        public async Task<IActionResult> ToApproveProject(string projectId, string role, string userLogin)
        {
            if (projectId is null || role is null || userLogin is null) return BadRequest();

            var project = await _projectRepository.GetProjectByIdAsync(projectId);
            _logger.LogInformation($"Project:{project.Id}");

            await _projectService.EditPropertiesAsync(role, "Approved", userLogin, project);

            _logger.LogInformation("Properties updated");

            await ChangeStatusAndNotificateIfApproved(project);
            await _projectService.EditProjectAsync(project, null); //the same thing with role
            return Ok();    
        }

        [HttpPost("/projects/GeneralDirector-Approve")]
        public async Task<IActionResult> ToApproveGenDirProject() //Timur R approved project
        {
            var project = await Request.ReadFromJsonAsync<Project>();
            if (project.FullApproveComment is null) return BadRequest();

            await NotificateAllResponsibleAsync(project);

            project.NowStatus = "In Progress";
            await _projectService.EditProjectAsync(project, null);
            
            return Ok();
        }

        [HttpPost("/projects/GeneralDirector-Disapprove")]//Timur R disapproved project
        public async Task<IActionResult> ToDisapproveGenDirProject() 
        {
            var project = await Request.ReadFromJsonAsync<Project>();
            if (project.FullApproveComment is null) return BadRequest();

            await NotificateAllResponsibleAsync(project);

            //project.NowStatus = "";
            project.IsArchived = true;
            project.NowStatus = "Archived";
            await _projectService.EditProjectAsync(project, null);

            return Ok();
        }

        [HttpGet]
        //[Authorize("OnlyForPM")]
        [Route("/projects/role={role}")]
        public async Task<IActionResult> GetProjectsByRole(string role)
        {
            return Ok(await _projectRepository.GetAllProjectsByRoleAsync(role));
        }

        [HttpGet]
        //[Authorize("OnlyForPM")]
        [Route("/projects/status={status}")]
        public async Task<IActionResult> GetProjectsByStatus(string status)
        {
            try
            {
                var jsonList = await _projectRepository.GetAllProjectsByStatus(status);
                return Json(jsonList);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return NoContent();
            }
        }

        [HttpGet]
        //[Authorize("OnlyForPM")]
        [Route("/projects/get-by-id/{id}")]
        public async Task<IActionResult> GetProjectById(string id)
        {
            try
            {
                var project = await _projectRepository.GetProjectByIdAsync(id);

                return Json(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }

        private HttpContent CreateContentWithURI(string message, string redirectURI)
        {
            var notificationJSON = JsonSerializer.Serialize(new
            {
                NotificationMessage = message,
                RedirectUri = redirectURI
            }); //Here you need to insert a link to receive the project

            var content = new StringContent(notificationJSON, Encoding.UTF8, "application/json");
            var serviceKey = _config["NotificationService"];
            content.Headers.Add("X-KEY", serviceKey);

            return content;
        }

        

        private HttpContent CreateContentWithoutURI(string message)
        {
            var notificationJSON = JsonSerializer.Serialize(new
            {
                NotificationMessage = message,
            }); //Here you need to insert a link to receive the project

            var content = new StringContent(notificationJSON, Encoding.UTF8, "application/json");
            var serviceKey = _config["NotificationService"];
            content.Headers.Add("X-KEY", serviceKey);

            return content;
        }

        private async Task ChangeStatusAndNotificateIfApproved(Project project)
        {
            if (project.LawyerStatus == "Approved" && project.BuilderStatus == "Approved" && project.FinancierStatus == "Approved")
            {
                project.NowStatus = "Approved";
                _logger.LogInformation($"Project: {project.Id} sending to Timur Rashidovich");
                var content = CreateContentWithURI($"Проект согласован!", $"ProjectsAndDeals/projectCard?id={project.Id}");
                var result = _projectService.NotificateAsync("OlegAss", content);
            }
        }   

        private async Task NotificateAllResponsibleAsync(Project project)
        {
            //await _projectService.NotificateAsync(project.RolesLogins.FinancierLogin, CreateContentWithURI(project.FullApproveComment, $"ProjectsAndDeals/projectCard?id={project.Id}"));
            //await _projectService.NotificateAsync(project.RolesLogins.LawyerLogin, CreateContentWithURI(project.FullApproveComment, $"ProjectsAndDeals/projectCard?id={project.Id}"));
            //await _projectService.NotificateAsync(project.RolesLogins.BuilderLogin, CreateContentWithURI(project.FullApproveComment, $"ProjectsAndDeals/projectCard?id={project.Id}"));
            await _projectService.NotificateAsync(project.RolesLogins.ProjectManagerLogin, CreateContentWithURI(project.FullApproveComment, $"ProjectsAndDeals/projectCard?id={project.Id}")); //delete when roles wiil be
        }
    }
}
