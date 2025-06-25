//using System.ComponentModel.DataAnnotations;
using ERP_Proflipper_ProjectService;
using ERP_Proflipper_WorkspaceService.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ERP_Proflipper_WorkspaceService.Controllers
{
    public class ProjectController : Controller
    {
        ProjectValidator projectValidator = new ProjectValidator(); //custom validator

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("/projects")]
        public async Task<StatusCodeResult> AddProductsInDB(ILogger<ProjectController> logger)
        {
            try
            {
                var project = await Request.ReadFromJsonAsync<Project>(); //read project from form
                await projectValidator.ValidateAndThrowAsync(project); //validate project data

                ProjectDAO.AddProjectInDB(project);

                return Ok();
            }
            catch (ValidationException exception)
            {
                foreach (var error in exception.Errors)
                {
                    logger.LogError(error.ErrorMessage);
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

        [HttpPut]
        [Route("/projects")]
        public async Task<StatusCodeResult> EditProject(ILogger<ProjectController> logger)
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
                    logger.LogError(error.ErrorMessage);
                }

                return BadRequest();
            }
        }
    }
}
