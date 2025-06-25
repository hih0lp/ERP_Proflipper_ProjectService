//using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ERP_Proflipper_ProjectService;
using ERP_Proflipper_WorkspaceService.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Proflipper_WorkspaceService.Controllers
{
    public class ProjectController : Controller
    {
        ProjectValidator projectValidator = new ProjectValidator(); //валидатор данных для project
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
                var project = await Request.ReadFromJsonAsync<Project>(); //получение данных из формы
                await projectValidator.ValidateAndThrowAsync(project); //валидирование данные

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
