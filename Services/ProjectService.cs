using ERP_Proflipper_ProjectService.Models;
using ERP_Proflipper_ProjectService.Repositories.Interface;
using ERP_Proflipper_ProjectService.Repositories.Ports;
using ERP_Proflipper_ProjectService;
using ERP_Proflipper_ProjectService.Models;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using Microsoft.IdentityModel.Tokens;

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

        public async Task<string> CreateProjectInDB(Project project, string login)
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

            project.CreatedAt = DateTimeOffset.Now.ToString();
            project.RolesLogins = new();
            project.RolesLogins.ProjectId = project.Id;
            project.RolesLogins.ProjectManagerLogin = login;
            //project.RolesLogins.ProjectManagerLogin = PMLogin;
            //project.RolesLogins.ProjectManagerLogin = responsibleLogin;
            //project.
            await _repository.AddProjectInDB(project);

            return project.Id;
        }

        public async Task EditProjectAsync(Project modifiedProject, string role, string userLogin) //mb need to add something like a check an accessibility of db, params string modifable project card
        {
            
            var changableProject = await _repository.GetProjectByIdAsync(modifiedProject.Id);
            changableProject.FinancierCardJson = modifiedProject.FinancierCardJson;
            changableProject.PMCardJson = modifiedProject.PMCardJson;
            changableProject.BuilderCardJson = modifiedProject.BuilderCardJson;
            changableProject.LawyerCardJson = modifiedProject.LawyerCardJson;

            ///НИЖЕ МЕТОДЫ ДЛЯ ОБНОВЛЕНИЯ ПО ОТДЕЛЬНЫМ РОЛЯМ, НЕОБХОДИМО ДОБАВИТЬ ЛОГИКУ ПРОВЕРКУ НА ТО, ЕСТЬ ЛИ УЖЕ ОТВЕТСТВЕННАЯ РОЛЬ НА ЭТОМ ПРОЕКТЕ (ЕСЛИ НЕТ, ТО ДОБАВИТЬ), ИНАЧЕ КИНУТЬ ОШИБКУ, СЧАСТЛИВЫХ ГОЛОДНЫХ ИГР

            //_logger.LogInformation(modifiedProject.Id);
            //_logger.LogInformation(changableProject.Id);
            //_logger.LogInformation(modifiedProject.NowStatus);
            _logger.LogInformation(role + " " + userLogin);
            //_logger.LogInformation(changableProject.CreatedAt);
            //UPDATE
            switch (role)
            {
                //case "ProjectManager":
                //    if (changableProject.RolesLogins.ProjectManagerLogin.IsNullOrEmpty()) changableProject.RolesLogins.ProjectManagerLogin = userLogin;
                //    else if (!changableProject.RolesLogins.ProjectManagerLogin.Equals(userLogin)) return; //тут ошибку по идее нужно будет ввести, и все
                //    break;
                case "Financier":
                    //changableProject.FinancierCardJson = modifiedProject.FinancierCardJson;
                    if (changableProject.RolesLogins.FinancierLogin.IsNullOrEmpty()) changableProject.RolesLogins.FinancierLogin = userLogin;
                    else if (!changableProject.RolesLogins.FinancierLogin.Equals(userLogin)) return; //тут ошибку по идее нужно будет ввести, и все
                    break;
                case "Builder":
                    //changableProject.BuilderCardJson = modifiedProject.BuilderCardJson;
                    if(changableProject.RolesLogins.BuilderLogin.IsNullOrEmpty()) changableProject.RolesLogins.BuilderLogin = userLogin;
                    else if (!changableProject.RolesLogins.BuilderLogin.IsNullOrEmpty() && !changableProject.RolesLogins.BuilderLogin.Equals(userLogin)) return;
                    break;
                case "Lawyer":
                    //changableProject.LawyerCardJson = modifiedProject.LawyerCardJson;
                    if (changableProject.RolesLogins.LawyerLogin.IsNullOrEmpty()) changableProject.RolesLogins.LawyerLogin = userLogin;
                    else if (!changableProject.RolesLogins.LawyerLogin.Equals(userLogin)) return;
                    break;
            }


            changableProject.SellerCheckJson = modifiedProject.SellerCheckJson;
            changableProject.NowStatus = modifiedProject.NowStatus;
            //changableProject.Rules = modifiedProject.Rules;
            changableProject.IsFinished = modifiedProject.IsFinished;
            changableProject.IsArchived = modifiedProject.IsArchived;
            changableProject.ApproveStatus = modifiedProject.ApproveStatus;
            changableProject.SubStatus  = modifiedProject.SubStatus;
            changableProject.BuilderStatus  = modifiedProject.BuilderStatus;
            changableProject.LawyerStatus = modifiedProject.LawyerStatus;
            changableProject.FinancierStatus = modifiedProject.FinancierStatus;
            changableProject.FullApproveComment = modifiedProject.FullApproveComment;
            //changableProject.RolesLogins = modifiedProject.RolesLogins;
            //changableProject.CreatedAt = modifiedProject.CreatedAt;

            await _repository.UpdateAsync(changableProject); //change to user role
            
        }

        public async Task<Result> NotificateAsync(string userLogin, HttpContent content)
        {
            var response = await _httpClient.PostAsync($"https://localhost:7118/user/{userLogin}", content); //in parentheses must be login or name of Timur Rashidovich
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

        //public async Task<Result> ApplyRoleIfNeeded(string role, string login, string projectId)
        //{
        //    var project = _repository.GetProjectByIdAsync(projectId);

        //    if(project.)
        //}

        //public async Task SendToApproveWithOpenAccess(Project project)
        //{
        //    project.NowStatus = "TEST";

        //    foreach (var rule in project.Rules)
        //    {
        //        rule.CanRead = true;
        //        rule.CanWrite = true;
        //    }

        //    //project.Rules.First(x => x.RoleName == "ProjectManager").CanRead = false; //now all roles excluding others project manager can read and write project
        //    //project.Rules.First(x => x.RoleName == "ProjectManager").CanWrite = false; //now all roles excluding others project manager can read and write project
        //    await _repository.UpdateAsync(project);
        //}

        public async Task EditPropertiesAsync(string role, string status, string userLogin, Project _project)
        {
            var project = await _repository.GetProjectByIdAsync(_project.Id);
            project.LawyerStatus = status;
            project.BuilderStatus = status;
            project.FinancierStatus = status;

            //switch (role)
            //{
            //    case "Financier":
            //        project.FinancierStatus = status;
            //        project.RolesLogins.FinancierLogin = userLogin;
            //        project.Rules.First(x => x.RoleName == role).CanRead = false;
            //        break;
            //    case "Lawyer":
            //        project.LawyerStatus = status;
            //        project.RolesLogins.LawyerLogin = userLogin;
            //        project.Rules.First(x => x.RoleName == role).CanRead = false;
            //        break;
            //    case "Builder":
            //        project.BuilderStatus = status;
            //        project.RolesLogins.BuilderLogin = userLogin;
            //        project.Rules.First(x => x.RoleName == role).CanRead = false;
            //        break;
            //    case "ProjectManager":
            //        project.RolesLogins.ProjectManagerLogin = userLogin;
            //        project.Rules.First(x => x.RoleName == role).CanRead = false;
            //        break;
            //    case "GeneralDirector":
            //        project.NowStatus = "In Progress";
            //        break;
            //}

            await _repository.UpdateAsync(project);
        }


        //public async Task<bool> CheckAccessAndRules(string projectId, string role, string userLogin)
        //{
        //    var project = await _repository.GetProjectByIdAsync(projectId);

        //    if (project.Responsibles.Any(x => x.ResponsibleRole == role) == null) project.Responsibles.Add(new ProjectResponsibles()
        //    {
        //        ProjectId = projectId,
        //        ResponsibleName = userLogin,
        //        ResponsibleRole = role,
        //        Project = project
        //    });

        //    Console.WriteLine(1234567890);
        //    //else return false;

        //    if (project.Rules
        //        .Any(x => x.RoleName == role && (!x.CanWrite || !x.CanRead) || project.Responsibles
        //        .Any(x => x.ResponsibleName != userLogin))) return false; //check for rules

        //    Console.WriteLine(1234567890);

        //    return true;
        //}
    }
}