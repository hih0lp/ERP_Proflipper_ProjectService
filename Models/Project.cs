using ERP_Proflipper_ProjectService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP_Proflipper_WorkspaceService.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public double Area { get; set; }
        public double Price { get; set; }
        public string Location { get; set; }
        public string Condition { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
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
      
        public static async Task<List<Project>> GetProjectsAsync()
        {
            using(var db = new ProjectsDB())
            {
                var projects = await db.Projects.ToListAsync();

                return projects;
            }
        }

        public static async Task EditProjectAsync(Project modifiedProject) //mb need to add something like a check an accessibility of db
        {
            using(var db = new ProjectsDB())
            {
                var changableProject = await db.Projects.FirstOrDefaultAsync(x => x.Id == modifiedProject.Id);
                changableProject = modifiedProject;

                db.Update(changableProject);
                await db.SaveChangesAsync();
            }
        }
    }
}
