using ERP_Proflipper_ProjectService.Models;
using ERP_Proflipper_ProjectService.Repositories.Interface;
using ERP_Proflipper_ProjectService.Repositories.Ports;
using ERP_Proflipper_WorkspaceService;
using ERP_Proflipper_WorkspaceService.Models;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace ERP_Proflipper_ProjectService.Services
{
    public class ProjectService
    {
        private readonly ProjectsDB _context;
        private readonly HttpClient _httpClient = new();
        private readonly IProjectRepository _repository;
        private readonly IConfiguration _config;
        private readonly ILogger<ProjectService> _logger;
        public ProjectService(ProjectsDB context, IProjectRepository repository, IConfiguration config, ILogger<ProjectService> logger)
        {
            _context = context;
            _repository = repository;
            _config = config;
            _logger = logger;
        }

        public async Task<string> CreateProjectInDB(Project project)
        {
            project.Id = Guid.NewGuid().ToString();
            project.NowStatus = "Potential";

            project.Rules = new List<RolesRules>()
            {
                new RolesRules { ProjectId = project.Id, RoleName = "ProjectManager", CanRead = true, CanWrite = true },
                new RolesRules { ProjectId = project.Id, RoleName = "Lawyer", CanRead = false, CanWrite = false },
                new RolesRules { ProjectId = project.Id, RoleName = "Financier", CanRead = false, CanWrite = false },
                new RolesRules { ProjectId = project.Id, RoleName = "Builder", CanRead = false, CanWrite = false }
            };

            project.RolesLogins = new();

            await _repository.AddProjectInDB(project);

            return project.Id;
        }

        public async Task EditProjectAsync(Project modifiedProject, string role) //mb need to add something like a check an accessibility of db, params string modifable project card
        {
            
            var changableProject = await _repository.GetProjectByIdAsync(modifiedProject.Id);
            changableProject.FinancierCardJson = modifiedProject.FinancierCardJson;
            changableProject.PMCardJson = modifiedProject.PMCardJson;
            changableProject.BuilderCardJson = modifiedProject.BuilderCardJson;
            changableProject.LawyerCardJson = modifiedProject.LawyerCardJson;

            _logger.LogInformation(modifiedProject.Id);
            _logger.LogInformation(changableProject.Id);
            _logger.LogInformation(modifiedProject.NowStatus);
            //UPDATE
            //switch (role)
            //{
            //    case "ProjectManager": 
            //        changableProject.PMCardJson = modifiedProject.PMCardJson;
            //        break;
            //    case "Financier":
            //        changableProject.FinancierCardJson = modifiedProject.FinancierCardJson;
            //        break;
            //    case "Builder":
            //        changableProject.BuilderCardJson = modifiedProject.BuilderCardJson;
            //        break;
            //    case "Lawyer":
            //        changableProject.LawyerCardJson = modifiedProject.LawyerCardJson;
            //        break;
            //}


            changableProject.SellerCheckJson = modifiedProject.SellerCheckJson;
            changableProject.NowStatus = modifiedProject.NowStatus;
            //changableProject.Rules = modifiedProject.Rules;
            changableProject.IsFinished = modifiedProject.IsFinished;
            changableProject.IsArchived = modifiedProject.IsArchived;

            await _repository.UpdateAsync(changableProject); //change to user role
            
        }

        public async Task<Result> NotificateAsync(string userLogin, HttpContent content)
        {
            var response = await _httpClient.PostAsync($"http://localhost:5079/user/{userLogin}", content); //in parentheses must be login or name of Timur Rashidovich
            try
            {
                response.EnsureSuccessStatusCode();

                return Result.Ok();

            }
            catch (HttpRequestException e)
            {
                return Result.Fail(e.Message);
            }
        }

        public async Task SendToApproveWithOpenAccess(Project project)
        {
            project.NowStatus = "Approving";

            foreach (var rule in project.Rules)
            {
                rule.CanRead = true;
                rule.CanWrite = true;
            }

            //project.Rules.First(x => x.RoleName == "ProjectManager").CanRead = false; //now all roles excluding others project manager can read and write project
            //project.Rules.First(x => x.RoleName == "ProjectManager").CanWrite = false; //now all roles excluding others project manager can read and write project
            await _repository.UpdateAsync(project);
        }

        public async Task EditPropertiesAsync(string role, string status, string userLogin, Project project)
        {
            switch (role)
            {
                case "Financier":
                    project.NowStatus = status;
                    project.RolesLogins.FinancierLogin = userLogin;
                    project.Rules.First(x => x.RoleName == role).CanRead = false;
                    break;
                case "Lawyer":
                    project.NowStatus = status;
                    project.RolesLogins.LawyerLogin = userLogin;
                    project.Rules.First(x => x.RoleName == role).CanRead = false;
                    break;
                case "Builder":
                    project.NowStatus = status;
                    project.RolesLogins.BuilderLogin = userLogin;
                    project.Rules.First(x => x.RoleName == role).CanRead = false;
                    break;
                case "ProjectManager":
                    project.RolesLogins.ProjectManagerLogin = userLogin;
                    project.Rules.First(x => x.RoleName == role).CanRead = false;
                    break;
                default:
                    project.NowStatus = status;
                    break;
            }

            await _repository.UpdateAsync(project);
        }

        public async Task<bool> CheckAccessAndRules(string projectId, string role, string userLogin)
        {
            var project = await _repository.GetProjectByIdAsync(projectId);

            if (project.Responsibles.Any(x => x.ResponsibleRole == role) == default) project.Responsibles.Add(new ProjectResponsibles()
            {
                ProjectId = projectId,
                ResponsibleName = userLogin,
                ResponsibleRole = role,
            });
            else return false;

            if (project.Rules.Any(x => x.RoleName == role && (!x.CanWrite || !x.CanRead))) return false; //check for rules

            return true;
        }
    }
}