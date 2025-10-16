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
        private readonly ProjectRepository _repository;
        private readonly IConfiguration _config;

        public ProjectService(ProjectsDB context, ProjectRepository repository, IConfiguration config)
        {
            _context = context;
            _repository = repository;
            _config = config;
        }

        public async Task<string> CreateProjectInDB(Project project)
        {
            project.Id = Guid.NewGuid().ToString();
            project.NowStatus = "Potential";

            List<RolesRules> rolesRules = new List<RolesRules>()
            {
                new RolesRules { ProjectId = project.Id, RoleName = "ProjectManager", CanRead = true, CanWrite = true },
                new RolesRules { ProjectId = project.Id, RoleName = "Lawyer", CanRead = false, CanWrite = false },
                new RolesRules { ProjectId = project.Id, RoleName = "Financier", CanRead = false, CanWrite = false },
                new RolesRules { ProjectId = project.Id, RoleName = "Builder", CanRead = false, CanWrite = false }
            };

            await _repository.AddProjectInDB(project);

            return project.Id;
        }

        public async Task EditProjectAsync(Project modifiedProject, string role) //mb need to add something like a check an accessibility of db, params string modifable project card
        {
            
            var changableProject = await  _repository.GetProjectByIdAsync(modifiedProject.Id);
            changableProject.FinancierCardJson = modifiedProject.FinancierCardJson;
            changableProject.PMCardJson = modifiedProject.PMCardJson;
            changableProject.BuilderCardJson = modifiedProject.BuilderCardJson;
            changableProject.LawyerCardJson = modifiedProject.LawyerCardJson;



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


            changableProject.NowStatus = modifiedProject.NowStatus;
            //changableProject.Rules = modifiedProject.Rules;
            changableProject.IsFinished = modifiedProject.IsFinished;
            changableProject.IsArchived = modifiedProject.IsArchived;

            await _repository.UpdateAsync(changableProject, null); //change to user role
            
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
            await _repository.UpdateAsync(project, null);
        }
    }
}
