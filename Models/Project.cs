using ERP_Proflipper_ProjectService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ERP_Proflipper_WorkspaceService.Models
{
    public class Project
    {
        public string? Id { get; set; }
        public string? PMCardJson { get; set; }
        public string? FinancierCardJson { get; set; }
        public string? BuilderCardJson { get; set; }
        public string? LawyerCardJson { get; set; }
        public string NowStatus { get; set; } = "Потенциальный"; //put in some status
        public bool IsFinished { get; set; } = false;
        public bool IsArchived { get; set; } = false;
        public List<RolesRules> Rules { get; set; } = new()
        {
            new RolesRules() {RoleName = "ProjectManager", CanRead = true, CanWrite = true},
            new RolesRules() {RoleName = "Lawyer", CanRead = false, CanWrite = false},
            new RolesRules() {RoleName = "Financier", CanRead = false, CanWrite = false},
            new RolesRules() {RoleName = "Builder", CanRead = false, CanWrite = false}
        };
        public string? SubStatus { get; set; }
        public string? Responsible { get; set; }
        public int? ApproveStatus { get; set; } = 0;
    }



    public static class ProjectDAO
    {
        

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
