using ERP_Proflipper_ProjectService;
﻿using Microsoft.EntityFrameworkCore;

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
        ProjectsDB _db = new ProjectsDB();
        ProjectValidator projectValidator = new ProjectValidator();
      
        public static async void AddProjectInDB(Project project)
        {
            _db.Projects.Add(project);
            await _db.SaveChangesAsync();
          
        }
      
        public static async Task<List<Project>> GetProjects()
        {
            using(var db = new ProjectsDB())
            {
                var projects = await db.Projects.ToListAsync();

                return projects;
            }
        }
    }
}
