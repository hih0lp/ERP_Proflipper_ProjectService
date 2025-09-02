using ERP_Proflipper_WorkspaceService.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP_Proflipper_WorkspaceService
{
    public class ProjectsDB : DbContext
    {
        public ProjectsDB()
        {
            Console.WriteLine("dfkbnfoib");
            Database.EnsureCreated();
        }

        public DbSet<Project> Projects { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=195.54.178.243; Port=27031; Database=ERP_PROJECTS; Username=admin; Password=Tandem_2025; Encoding=UTF8; Pooling=true");
        }
    }
}
