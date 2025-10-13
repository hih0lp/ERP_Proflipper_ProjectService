using ERP_Proflipper_ProjectService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ERP_Proflipper_WorkspaceService.Models
{
    public class Project
    {
        public string? Id { get; set; }
        public string? PMCardJson { get; set; }
        //public PMCardModel PMCardModel { get; set; }
        public string? FinancierCardJson { get; set; }
        //public FinancierCardModel FinancierCardModel { get; set; }
        public string? BuilderCardJson { get; set; }
        //public BuilderCardModel BuilderCardModel { get; set; }
        public string? LawyerCardJson { get; set; }
        //public LawyerCardModel LawyerCardModel { get; set; }+
        public string NowStatus { get; set; } = "Потенциальный"; //put in some status
        public bool IsFinished { get; set; } = false;
        public bool IsArchived { get; set; } = false;
        public List<RolesRules> Rules { get; set; } = new()
        {
            new RolesRules() {RoleName = "ProjectManager", CanRead= true, CanWrite = true},
            new RolesRules() {RoleName = "Lawyer", CanRead = false, CanWrite = false},
            new RolesRules() {RoleName = "Financier", CanRead = false, CanWrite = false},
            new RolesRules() {RoleName = "Builder", CanRead = false, CanWrite = false}
        };
        public string? Responsible { get; set; }
        public int? ApproveStatus { get; set; } = 0;
    }



    public static class ProjectDAO
    {
        public static async Task<string> AddProjectInDB(Project project)
        {
            using (var db = new ProjectsDB())
            {
                project.Id = Guid.NewGuid().ToString();

                foreach (var rule in project.Rules)
                {
                    rule.ProjectId = project.Id;
                }

                await db.Projects.AddAsync(project);
                await db.SaveChangesAsync();

                return project.Id;
            }

            //using (var db = new ProjectsDB())
            //{
            //    Project project = new Project();
            //    project.PMCardJson = pmCard;

            //    db.Projects.Add(project);
            //    await db.SaveChangesAsync();
            //}
        }

        //public async Task<Result> 


        //public static async Task<Project> GetCardByRoleAndIdAsync(int id, string role) //Task<Result<string>>
        //{
        //    using (var db = new ProjectsDB())
        //    {
        //        var project = db.Projects.FirstOrDefault(x => x.Id == id);
        //        return project;
        //        //if (project is not null)
        //        //{
        //        //    string cardData = role switch
        //        //    {
        //        //        "ProjectManager" => project.PMCardJson,
        //        //        "Financier" => project.PMCardJson,
        //        //        "Builder" => project.BuilderCardJson,
        //        //        "Lawyer" => project.LawyerCardJson
        //        //    };

        //        //    return Result.Ok(cardData);
        //        //}

        //        //return Result.Fail("Failed");
        //    }   
        //}



        //public static async Task<Project> GetProjectAsync(int id)
        //{
        //    using (var db = new ProjectsDB())
        //    {
        //        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == id);

        //        return project;
        //    }
        //}

        public static async Task EditProjectAsync(Project modifiedProject, string role) //mb need to add something like a check an accessibility of db, params string modifable project card
        {
            using(var db = new ProjectsDB())
            {
                var changableProject = await db.Projects.FirstOrDefaultAsync(x => x.Id == modifiedProject.Id);
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

                await db.SaveChangesAsync(); 
            }
        }
      
      
        public static async void ChangeProjectStatus(Project project, string nextStatus)
        {
            //only director can finish project
            using (var db = new ProjectsDB())
            {
                project.NowStatus = nextStatus;
                db.Projects.Update(project);
                db.SaveChanges();
              
            }
        }
              
              
              
        public static async Task<bool> DeleteProjectAsync(string id)
        {
            using (var db = new ProjectsDB())
            {
                var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == id);

                if (project == null) return false;

                db.Remove<Project>(project);
                await db.SaveChangesAsync();

                return true;
            }
        }

        public static async Task<Project> GetProjectByIdAsync(string id)
        {
            using var db = new ProjectsDB();
            return db.Projects.FirstOrDefault(p => p.Id == id);


        }

        public static async Task<List<Project>> GetAllProjectsAsync()
        {
            using var db = new ProjectsDB();

            return await db.Projects.ToListAsync();
        }

        public static async Task<List<Project>> GetAllProjectsByRoleAsync(string role) //gettind all projects by user role
        {
            using var db = new ProjectsDB();
            var projectsList = await db.Projects.Where(x => x.Rules.Any(r => r.RoleName == role && r.CanRead == true) && x.IsArchived == false && x.IsFinished == false).ToListAsync();

            return projectsList;
        }

        public static async Task<string> GetProjectCardByRoleAndId(string projectId, string role) //getting card project by role and project id
        {
            var project = await ProjectDAO.GetProjectByIdAsync(projectId);

            return role switch
            {
                "ProjectManager" => project.PMCardJson,
                "Builder" => project.BuilderCardJson,
                "Financier" => project.FinancierCardJson,
                "Lawyer" => project.LawyerCardJson
            };
        }

        public static async Task<List<Project>> GetAllProjectsByStatus(string status)
        {
            using (var db = new ProjectsDB())
            {
                return db.Projects.Where(x => x.NowStatus == status).ToList();
            }
        }

        //public static async Task<Result> CreateGoogleProjectSheet(IConfiguration configuration, string projectName, SheetsService sheetsService)
        //{
        //    var sheetId = configuration["sheet_id"];
        //    string credentialsPath = "input_path_of_credentials";

        //    var newSpreadSheet = new Spreadsheet
        //    {
        //        Properties = new SpreadsheetProperties
        //        {
        //            Title = projectName
        //        }
        //    };

        //    try
        //    {
        //        var createdSpreadsheet = sheetsService.Spreadsheets.Create(newSpreadSheet).Execute();
        //        await FillNowCreatedTable(createdSpreadsheet, sheetsService);

        //        return Result.Ok();
                
        //    }
        //    catch (Exception ex)
        //    {
        //        return Result.Fail(ex.Message);
        //    }           
        //}


        //private static async Task FillNowCreatedTable(Spreadsheet newTable, SheetsService sheetsService)
        //{
        //    var sheetId = newTable.SpreadsheetId;
        //    SpreadsheetsResource.ValuesResource sheetsResource = sheetsService.Spreadsheets.Values;

        //    var valueRange = new ValueRange
        //    {
        //        Values = new List<IList<object>>()
        //        {
        //            new List<object>() { "НАЗВАНИЕ ПРОЕКТА" },
        //            new List<object>() { "НЕОБХОДИМО СРЕДСТВ" },
        //            new List<object>() { "СОБРАНО" }
        //        }
        //    };

        //    var updateRequest = sheetsResource.Update(valueRange, sheetId, "A1:C1");
        //    updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
        //    await updateRequest.ExecuteAsync();
        //}
    }
}
