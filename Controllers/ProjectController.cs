using System.Diagnostics;
using System.Runtime.CompilerServices;
using ERP_Proflipper_WorkspaceService.Models;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Proflipper_WorkspaceService.Controllers
{
    public class ProjectController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
