//using System.ComponentModel.DataAnnotations;
using ERP_Proflipper_ProjectService;
using ERP_Proflipper_WorkspaceService.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace ERP_Proflipper_WorkspaceService.Controllers
{
    public class ProjectController : Controller
    {
        ProjectValidator projectValidator = new ProjectValidator(); //custom validator
        private ILogger<ProjectController> _logger;

        public ProjectController(ILogger<ProjectController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("/projects")]
        public async Task<StatusCodeResult> AddProjectInDB()
        {
            try
            {
                var project = await Request.ReadFromJsonAsync<Project>(); //read project from form
                await projectValidator.ValidateAndThrowAsync(project); //validate project data

                Console.WriteLine(project.Comment);

                ProjectDAO.AddProjectInDB(project);

                return Ok();
            }
            catch (ValidationException exception)
            {
                foreach (var error in exception.Errors)
                {
                    _logger.LogError(error.ErrorMessage);
                }

                return BadRequest();
            }
        }

        [HttpGet]
        [Route("/projects")]
        public async Task<JsonResult> GetProjects()
        {
            var projects = await ProjectDAO.GetProjectsAsync();
            return Json(projects);
        }

        [HttpGet]
        [Route("/projects/{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            var project = await ProjectDAO.GetProjectAsync(id);
            
            return project == null ? BadRequest() : Json(project);
        }

        [HttpPut]
        [Route("/projects")]
        public async Task<StatusCodeResult> EditProject()
        {
            try
            {
                var modifiedProject = await Request.ReadFromJsonAsync<Project>(); //get from form
                await projectValidator.ValidateAndThrowAsync(modifiedProject);

                await ProjectDAO.EditProjectAsync(modifiedProject);

                return Ok();

            }
            catch (ValidationException exception)
            {
                foreach (var error in exception.Errors)
                {
                    _logger.LogError(error.ErrorMessage);
                }

                return BadRequest();
            }
        }

        [HttpDelete]
        [Route("/projects/{id}")]
        public async Task<StatusCodeResult> DeleteProject(int id)
        {
            return await ProjectDAO.DeleteProjectAsync(id) ? Ok() : BadRequest();
        }
    }
}
