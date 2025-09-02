using ERP_Proflipper_ProjectService;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Builder.Extensions;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;

namespace ERP_Proflipper_WorkspaceService.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NowStatus { get; set; }
        public bool IsPaused { get; set; }
        public bool IsFinished { get; set; }
        public double Area { get; set; }
        public double Price { get; set; }
        public string Location { get; set; }
        public string Condition { get; set; }
        public string? Comment { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string Responsible { get; set; }
    }

    public static class ProjectDAO
    {
        public static async void AddProjectInDB(Project project)
        {
            using (var db = new ProjectsDB())
            {
                db.Projects.Add(project);
                await db.SaveChangesAsync();
            }
          
        }
      
        ////////каждая отдельная роль получает проекты согласно своей роли
        public static async Task<List<Project>> GetProjectsAsync()
        {
            //projects can be seen by all people who have access to the projects
            using (var db = new ProjectsDB())
            {
                var projects = db.Projects.Where(x => !x.IsFinished).ToList();

                return projects;
            }
        }

        public static async Task<Project> GetProjectAsync(int id)
        {
            using (var db = new ProjectsDB())
            {
                var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == id);

                return project;
            }
        }

        public static async Task EditProjectAsync(Project modifiedProject) //mb need to add something like a check an accessibility of db
        {
            using(var db = new ProjectsDB())
            {
                var changableProject = await db.Projects.FirstOrDefaultAsync(x => x.Id == modifiedProject.Id);

                changableProject.IsFinished = modifiedProject.IsFinished;
                changableProject.Name = modifiedProject.Name;
                changableProject.Area = modifiedProject.Area;
                changableProject.IsPaused = modifiedProject.IsPaused;
                changableProject.Comment = modifiedProject.Comment;
                changableProject.Condition = modifiedProject.Condition;
                changableProject.CreatedAt = modifiedProject.CreatedAt;
                changableProject.UpdatedAt = modifiedProject.UpdatedAt;
                changableProject.Price = modifiedProject.Price;
                changableProject.Location = modifiedProject.Location;
                changableProject.NowStatus = modifiedProject.NowStatus;
                changableProject.Responsible = modifiedProject.Responsible;

                //db.Update(changableProject); //1
                //db.Attach(changableProject); //2
                //db.Entry(changableProject).State = EntityState.Modified; //3

                //db.Entry(changableProject).CurrentValues.SetValues(modifiedProject);

                await db.SaveChangesAsync(); //4
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
              
              
              
        public static async Task<bool> DeleteProjectAsync(int id)
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
