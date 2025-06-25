using ERP_Proflipper_WorkspaceService.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP_Proflipper_WorkspaceService
{
    public class ProjectsDB : DbContext
    {
        public ProjectsDB()
        {
            Database.EnsureCreated();
        }

        public DbSet<ProjectModel> Projects { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
    }
}
