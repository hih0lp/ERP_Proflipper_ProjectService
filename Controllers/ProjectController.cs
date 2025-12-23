using ERP_Proflipper_ProjectService;
using ERP_Proflipper_ProjectService.Models;
using ERP_Proflipper_ProjectService.Models;
using ERP_Proflipper_ProjectService.Repositories.Interface;
using ERP_Proflipper_ProjectService.Repositories.Ports;
using ERP_Proflipper_ProjectService.Services;
using FluentResults;
using Google.Apis.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;



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
            var id = await _projectService.CreateProjectInDB(project); //Ò˛‰‡ Â˘Â ÎÓ„ËÌ ÂÒÚ¸

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
            _logger.LogInformation("IOGBNROIMBPGNPIOTMSBSIHPHYUNPBISGTRPISIBPPIHJ");
            return Ok();
        }

        [HttpPost]
        [Route("/project/to-pm-approve")]
        public async Task<IActionResult> SendToApproveWithOpenAccess()
        {
            try
            {
                var project = await Request.ReadFromJsonAsync<Project>();
                if (project is null) return BadRequest("invalid json");
                if (project.NowStatus.Equals("Approving")) return BadRequest("project already in approving status");

                var authHeader = Request.Headers["Authorization"].ToString();
                var parsedUserModel = await GetUserModelJsonAsync(authHeader);

                if (parsedUserModel is null) 
                    return BadRequest("Failed get user rights");

                if (parsedUserModel["gendirRole"].ToObject<bool>() == false && parsedUserModel["canSendToApproveProject"].ToObject<bool>() == false)
                {
                    _logger.LogError($"No rights, userLogin: {parsedUserModel["login"]}");
                    return BadRequest("No rights");
                }

                project.NowStatus = "Approving";
                project.RolesLogins.ProjectManagerLogin = parsedUserModel["login"].ToString();

                await _projectService.EditProjectAsync(project, null);

                var content = CreateContentWithURI("œÓÂÍÚ ÓÚÔ‡‚ÎÂÌ Ì‡ ÒÓ„Î‡ÒÓ‚‡ÌËÂ!", $"ProjectsAndDeals/projectCard?id={project.Id}");
                var result = await _projectService.NotificateAsync(parsedUserModel["login"].ToString(), content);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                return BadRequest("Logic error");
            }
        }


        [HttpPut]
        [Route("/projects/disapprove-project/{projectId}/{role}/{userLogin}")]
        public async Task<IActionResult> DisapproveProject(string projectId, string role, string userLogin)
        {
            try ///TODO : œ–Œ—“Œ Õ¿’”… «¿œ–Œ— Õ≈ œ–»’Œƒ»“ ¡Àﬂ“‹ ƒ¿∆≈. «¿œ–Œ— Õ≈  »ƒ¿≈“—ﬂ —ﬁƒ¿ ¿ÀŒ ¿ÀŒ ¿ÀŒ 
            {
                //_logger.LogInformation("dfpbmrspijnbprsmtnibprsui9gjnb0s[igjrnp9srutnmpbsi9p9bijgtp9s8isnp");
                _logger.LogError("EOIHJORIHMBORIMNBROIUNB");

                if (projectId is null) return BadRequest();
                var project = await _projectRepository.GetProjectByIdAsync(projectId);
                _logger.LogError("Ok");

                var userParsedModel = await GetUserModelJsonAsync(Request.Headers["Authorization"].ToString());
                if (userParsedModel is null)
                    return BadRequest("user not found");
                if (userParsedModel["canBuilderApproveFinalizeDisapproveProject"].ToObject<bool>() == false &&
                        userParsedModel["canFinancierApproveFinalizeDisapproveProject"].ToObject<bool>() == false &&
                        userParsedModel["canLawyerApproveFinalizeDisapproveProject"].ToObject<bool>() == false &&
                        userParsedModel["gendirRole"].ToObject<bool>() == false)
                {
                    _logger.LogError("No rights");
                    return BadRequest("No rights");
                }

                //await _projectService.EditPropertiesAsync(userParsedModel["role"].ToString(), "Archived", userParsedModel["login"].ToString(), project);

                _logger.LogError($"Project: {project.Id} sending to archive");

                var content = CreateContentWithURI("’”… œ»«ƒ¿ «¿À”œ¿!", $"ProjectsAndDeals/projectCard?id={project.Id}");
                var result = await _projectService.NotificateAsync(userParsedModel["login"].ToString(), content);

                var dbProject = await _projectRepository.GetProjectByIdAsync(projectId);

                dbProject.IsArchived = true;
                await _projectRepository.UpdateAsync(dbProject);

                await _projectService.EditProjectAsync(project, null); //null must be a role when we will deploy or test with many roles

                return Ok();
            }///TODO : Õ≈ –¿¡Œ“¿≈“ ¡Àﬂ“‹, ◊≈ «¿ ’”…Õﬂ, ≈√Œ– ﬁƒ»Õ
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest("Logic error");
            }
        }


        [HttpPost]
        //[Authorize("OnlyForPM")]
        [Route("/projects/finalize-project/{projectId}/{role}/{userLogin}")]
        public async Task<IActionResult> ToFinalizeProject(string projectId, string role, string userLogin)
        {
            try
            {
                if (projectId is null || role is null || userLogin is null)
                    return BadRequest();

                var project = await _projectRepository.GetProjectByIdAsync(projectId);//get project
                if (project is null)
                    return BadRequest("project not found");

                var userParsedModel = await GetUserModelJsonAsync(Request.Headers["Authorization"].ToString());

                if (userParsedModel["canBuilderApproveFinalizeDisapproveProject"].ToObject<bool>() == false &&
                        userParsedModel["canFinancierApproveFinalizeDisapproveProject"].ToObject<bool>() == false &&
                        userParsedModel["canLawyerApproveFinalizeDisapproveProject"].ToObject<bool>() == false &&
                        userParsedModel["gendirRole"].ToObject<bool>() == false)
                {
                    return BadRequest("No rights");
                }

                string? message = (await new StreamReader(Request.Body).ReadToEndAsync()); //read message from json
                if (message is null)
                    return BadRequest();


                _logger.LogInformation($"Project {projectId} has been sending to finalize by {role}");


                await _projectService.EditPropertiesAsync(role, "Finalize", userLogin, project);

                var content = CreateContentWithURI(message, $"ProjectsAndDeals/projectCard?id={project.Id}");
                var result = await _projectService.NotificateAsync("OlegAss", content);

                return result.IsSuccess ? Ok() : BadRequest(result.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest("Logic error");
            }
        }


        [HttpPost]
        [Route("/projects/to-all-approve/{projectId}/{role}/{userLogin}")]
        public async Task<IActionResult> ToApproveProject(string projectId, string role, string userLogin)
        {
            try
            {
                if (projectId is null || role is null || userLogin is null) return BadRequest();
                _logger.LogInformation("PIKEMNBHIURDNMNBOIDTJNBIURMT[OBNIPDSTRBHPOSIJTRHNPIBTRMN;OSITDJNISRUTNGLIRSTHUJNPSRITHUNB;OTHIJNYP9DSTUHJNP;TRSM");
                //var project = await _projectRepository.GetProjectByIdAsync(projectId);
                //_logger.LogInformation($"Project:{project.Id}");

                ////await _projectService.EditPropertiesAsync(role, "Approved", userLogin, project);
                //project.LawyerStatus = "Approved";
                //project.FinancierStatus = "Approved";
                //project.BuilderStatus = "Approved";
                //project.NowStatus = "Approved";
                //// await _projectService.EditProjectAsync(project, null);
                //await _projectRepository.UpdateAsync(project);

                //_logger.LogInformation("Properties updated");

                //await ChangeStatusAndNotificateIfApproved(project);
                //await _projectService.EditProjectAsync(project, null); //the same thing with role
                return Ok();

            }
            catch (Exception)
            {

                throw;
            }
        }

        //[HttpPost("/projects/GeneralDirector-Approve")]
        //public async Task<IActionResult> ToApproveGenDirProject() //Timur R approved project
        //{
        //    _logger.LogInformation("pomhborimth");
        //    var project = await Request.ReadFromJsonAsync<Project>();
        //    if (project.FullApproveComment is null) return BadRequest();

        //    await NotificateAllResponsibleAsync(project);

        //    project.NowStatus = "In Progress";
        //    await _projectService.EditProjectAsync(project, null);

        //    return Ok();
        //}

        //[HttpPost("/projects/GeneralDirector-Disapprove")]//Timur R disapproved project
        //public async Task<IActionResult> ToDisapproveGenDirProject() 
        //{
        //    _logger.LogError("dfpkmbsrthbmrthmnsprothunrptu");
        //    var project = await Request.ReadFromJsonAsync<Project>();
        //    if (project.FullApproveComment is null) return BadRequest();

        //    await NotificateAllResponsibleAsync(project);

        //    //project.NowStatus = "";
        //    project.IsArchived = true;
        //    project.NowStatus = "Archived";
        //    await _projectService.EditProjectAsync(project, null);

        //    return Ok();
        //}

        [HttpGet]
        //[Authorize("OnlyForPM")]
        [Route("/projects/role={role}")]
        public async Task<IActionResult> GetProjectsByRole(string role)
        {
            return Ok(await _projectRepository.GetAllProjectsByRoleAsync(role));
        }

        [HttpGet]
        //[Authorize("OnlyForPM")]
        [Route("/projects/status={status}")]// /role={role}/login={userLogin}
        public async Task<IActionResult> GetProjectsByStatus(string status, string role, string userLogin)
        {
            try
            {
                var jsonList = await _projectRepository.GetAllProjectsByStatus(status);
                //jsonList = jsonList
                //    .Where(x => x.Rules.Any(t => t.RoleName == role && t.CanRead))
                //    .Where(x => x.RolesLogins.BuilderLogin == userLogin 
                //        || x.RolesLogins.FinancierLogin == userLogin 
                //        || x.LawyerCardJson == userLogin 
                //        || x.RolesLogins.ProjectManagerLogin == userLogin)
                    //.ToList();


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
                var content = CreateContentWithURI($"œÓÂÍÚ ÒÓ„Î‡ÒÓ‚‡Ì!", $"ProjectsAndDeals/projectCard?id={project.Id}");
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



        private async Task<JObject> GetUserModelJsonAsync(string authHeader)
        {
            try
            {

                var token = authHeader.Substring(7);
                _logger.LogInformation(token);



                var decodedTokenResponse = await _httpClient.PostAsync($"https://localhost:7253/decode-token/{token}", null);
                var decodeToken = await decodedTokenResponse.Content.ReadAsStringAsync();

                _logger.LogInformation(decodeToken);

                var parsedDecodedToken = JObject.Parse(decodeToken);

                _logger.LogInformation(parsedDecodedToken.ToString());

                var userModelResponse = await _httpClient.GetAsync($"https://localhost:7237/get-rights/{parsedDecodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]?.ToString()}");
                var userRightsJson = await userModelResponse.Content.ReadAsStringAsync();


                return JObject.Parse(userRightsJson);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }
    }
}
