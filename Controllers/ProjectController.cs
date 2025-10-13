//using System.ComponentModel.DataAnnotations;
using ERP_Proflipper_ProjectService;
using ERP_Proflipper_ProjectService.Models;
using ERP_Proflipper_WorkspaceService.Models;
using FluentResults;
using FluentValidation;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;


namespace ERP_Proflipper_WorkspaceService.Controllers
{
    public class ProjectController : Controller
    {
        private readonly HttpClient _httpClient = new();
        private readonly IConfiguration _config;
        ProjectValidator projectValidator = new ProjectValidator(); //custom validator
        private ILogger<ProjectController> _logger;
        private const int Mask = 0b111;
        //private ProjectDAO _projectDAO = new();

        public ProjectController(ILogger<ProjectController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        [Route("/project/create")]
        public async Task<IActionResult> CreateProject()
        {
            Project project = new Project();

            var id = await ProjectDAO.AddProjectInDB(project); //get project id when it is already in db

            return Ok(Json(id)); //return id
        }

        [HttpPut]
        [Route("/project/edit")]
        public async Task<StatusCodeResult> EditProject()
        {
            var project = await Request.ReadFromJsonAsync<Project>();


            _logger.LogInformation(project.Id);

            await ProjectDAO.EditProjectAsync(project, null);

            return Ok();
        }

        [HttpPost]
        [Route("/project/to-pm-approve")]
        public async Task<IActionResult> SendToApproveWithOpenAccess()
        {
            var project = await Request.ReadFromJsonAsync<Project>();
            project.NowStatus = "Approving";

            foreach (var rule in project.Rules)
            {
                rule.CanRead = false;
                rule.CanWrite = false;
            }

            var notificationJSON = JsonSerializer.Serialize($"Проектный менеджер отправил вам проект на согласование!"); //Here you need to insert a link to receive the project
            var content = new StringContent(notificationJSON, Encoding.UTF8, "application/json");
            var serviceKey = _config["NotificationService"];
            content.Headers.Add("X-KEY", serviceKey);


            var response = await _httpClient.PostAsync($"https://localhost:7118/user/ОлежикНавсегдаНаЕРП", content); //in parentheses must be login or name of Timur Rashidovich
            response.EnsureSuccessStatusCode(); //add check-in

            await ProjectDAO.EditProjectAsync(project, null); //when it is time to deploy or test with different roles null will be role

            return Ok(Json("investors/investorList/investorCard/projectCard"));
        }


        [HttpPut]
        [Route("/projects/disapprove-project/{id}")]
        public async Task<IActionResult> DisapproveProject(string id)
        {
            var project = await ProjectDAO.GetProjectByIdAsync(id);
            project.IsArchived = true;
            await ProjectDAO.EditProjectAsync(project, null); //null must be a role when we will deploy or test with many roles

            return Ok();
        }


        [HttpPost]
        [Route("/projects/finalize-project/{projectId}")]
        public async Task<IActionResult> ToFinalizeProject(string projectId, string message)
        {
            HttpClient httpClient = new HttpClient();

            var notificationJSON = JsonSerializer.Serialize(message);
            var content = new StringContent(notificationJSON, Encoding.UTF8, "application/json");
            var serviceKey = _config["NotificationService"];
            content.Headers.Add("X-KEY", serviceKey);

            var project = await ProjectDAO.GetProjectByIdAsync(projectId);

            var response = await httpClient.PostAsync($"https://localhost:7118/user/{project.Responsible}", content);
            response.EnsureSuccessStatusCode(); //add check-in

            return Ok();
        }


        [HttpPost]
        [Route("/projects/to-all-approve/{projectId}/{role}")]
        public async Task<IActionResult> ToApproveProject(string projectId, string role)
        {
            var project = await ProjectDAO.GetProjectByIdAsync(projectId);

            project.ApproveStatus |= role switch
            {
                "Financier" => 0b001,
                "Lawyer" => 0b010,
                "Builder" => 0b100,
                _ => 0b1111
            };

            if (project.ApproveStatus == Mask)
            {
                //HttpClient httpClient = new HttpClient();

                var notificationJSON = JsonSerializer.Serialize($"Проект был согласован финансистом, юристом, застройщиком, ссылка на проект "); //Here you need to insert a link to receive the project
                var content = new StringContent(notificationJSON, Encoding.UTF8, "application/json");
                var serviceKey = _config["NotificationService"];
                content.Headers.Add("X-KEY", serviceKey);


                var response = await _httpClient.PostAsync($"https://localhost:7118/user/ОлежикНавсегдаНаЕРП", content); //in parentheses must be login or name of Timur Rashidovich
                response.EnsureSuccessStatusCode(); //add check-in

                return Ok();
            }

            if (project.ApproveStatus == 0b1111)
            {
                project.NowStatus = "In progress";
            }

            await ProjectDAO.EditProjectAsync(project, null); //the same thing with role

            return Ok();    

        }

        [HttpGet]
        [Route("/projects/role={role}")]
        public async Task<IActionResult> GetProjectsByRole(string role)
        {
            return Ok(await ProjectDAO.GetAllProjectsByRoleAsync(role));
        }

        [HttpGet]
        [Route("/projects/status={status}")]
        public async Task<JsonResult> GetProjectsByStatus(string status)
        {
            return Json(await ProjectDAO.GetAllProjectsByStatus(status));
        }



        //[HttpPost]
        //[Route("/projects")]
        //[Authorize(Roles = "ProjectManager")]
        ////[Authorize(Policy = "OnlyForPM")]
        //public async Task<StatusCodeResult> AddProjectInDB() //READY
        //{
        //    try
        //    {
        //        var pmCard = await Request.ReadFromJsonAsync<string>(); //read project from form
        //                                                                //Project newProject = new();


        //        //await projectValidator.ValidateAndThrowAsync(project); //validate project data

        //        ProjectDAO.AddProjectInDB(pmCard);

        //        return Ok();
        //    }
        //    catch (ValidationException exception)
        //    {
        //        foreach (var error in exception.Errors)
        //        {
        //            _logger.LogError(error.ErrorMessage);
        //        }

        //        return BadRequest();
        //    }
        //}

        //[HttpGet]
        //[Route("/projects/{role}/{id}")]
        //[Authorize(Roles = "ProjectManager")]
        ////[Authorize(Roles = "Builder")]
        ////[Authorize(Roles = "Financier")]
        ////[Authorize(Roles = "Lawyer")]
        ////[Authorize(Policy = "OnlyForPM")]
        //public async Task<IActionResult> GetProjectCard(string role, int id) //NEED TEST //params string accessibleStatus
        //{
        //    var project = await ProjectDAO.GetCardByRoleAndIdAsync(id, role);
        //    return Json(project);

        //    //return result.IsSuccess ? Json(result.Value) : BadRequest();
        //}

        //[HttpGet]
        //[Authorize(Roles = "ProjectManager")]
        //[Route("/projects/{id}")]
        //public async Task<IActionResult> GetProject(int id)
        //{
        //    var project = await ProjectDAO.GetProjectAsync(id);

        //    return project == null ? BadRequest() : Json(project);
        //}

        //[HttpPut]
        //[Route("/projects/{role}")]
        //public async Task<StatusCodeResult> EditProject(string role) //NEED TEST
        //{
        //    try
        //    {
        //        var modifiedProject = await Request.ReadFromJsonAsync<Project>(); //get from form
        //        await projectValidator.ValidateAndThrowAsync(modifiedProject);

        //        await ProjectDAO.EditProjectAsync(modifiedProject, role);

        //        return Ok();

        //    }
        //    catch (ValidationException exception)
        //    {
        //        foreach (var error in exception.Errors)
        //        {
        //            _logger.LogError(error.ErrorMessage);
        //        }

        //        return BadRequest();
        //    }
        //}

        //[HttpPut]
        //[Route("change-status")]
        //public async Task<StatusCodeResult> ChangeProjectStatus(string nextStatus)
        //{
        //    var project = await Request.ReadFromJsonAsync<Project>();
        //    ProjectDAO.ChangeProjectStatus(project, nextStatus);

        //    return Ok();
        //}



        //[HttpDelete]
        //[Route("/projects/{id}")]
        //public async Task<StatusCodeResult> DeleteProject(int id)
        //{
        //    return await ProjectDAO.DeleteProjectAsync(id) ? Ok() : BadRequest();
        //}
    }
}
