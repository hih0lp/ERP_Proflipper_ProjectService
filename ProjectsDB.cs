using ERP_Proflipper_ProjectService.Models;
using ERP_Proflipper_WorkspaceService.Models;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.EntityFrameworkCore;

namespace ERP_Proflipper_WorkspaceService
{
    public class ProjectsDB : DbContext
    {

        public ProjectsDB(DbContextOptions<ProjectsDB> options) : base(options) { }

        public DbSet<Project> Projects { get; set; }
        public DbSet<RolesRules> RolesRules { get; set; }
        public DbSet<RolesLogins> RolesLogins { get; set; }
        public DbSet<ProjectResponsibles> ProjectResponsibles { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>().HasKey(p => p.Id);
            modelBuilder.Entity<RolesRules>().HasKey(r => r.Id);
            modelBuilder.Entity<RolesLogins>().HasKey(r => r.Id);
            modelBuilder.Entity<ProjectResponsibles>().HasKey(r => r.Id);

            modelBuilder.Entity<Project>()
                .HasMany(p => p.Rules)
                .WithOne(r => r.Project)
                .HasForeignKey(r => r.ProjectId);

            modelBuilder.Entity<Project>()
                .HasOne(t => t.RolesLogins)
                .WithOne(p => p.Project)
                .HasForeignKey<RolesLogins>(k => k.ProjectId);

            modelBuilder.Entity<Project>()
                .HasMany(r => r.Responsibles)
                .WithOne(x => x.Project)
                .HasForeignKey(f => f.ProjectId);
        }
    }
}
