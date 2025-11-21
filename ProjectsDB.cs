using ERP_Proflipper_ProjectService.Models;
using ERP_Proflipper_ProjectService.Models;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ERP_Proflipper_ProjectService
{
    public class ProjectsDB : DbContext
    {

        //public ProjectsDB() { Database.EnsureCreated(); }
        public ProjectsDB(DbContextOptions<ProjectsDB> options) : base(options) { }

        public DbSet<Project> Projects { get; set; }
        public DbSet<RolesRules> RolesRules { get; set; }
        public DbSet<RolesLogins> RolesLogins { get; set; }
        //public DbSet<ProjectResponsibles> ProjectResponsibles { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseNpgsql("Host=195.54.178.243; Port=27031; Database=ERP_PROJECTS; Username=admin; Password=Tandem_2025; Encoding=UTF8; Pooling=true");
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>().HasKey(p => p.Id);
            modelBuilder.Entity<RolesRules>().HasKey(r => r.Id);
            modelBuilder.Entity<RolesLogins>().HasKey(r => r.Id);
            modelBuilder.Entity<ProjectResponsibles>().HasKey(r => r.Id);

            modelBuilder.Entity<Project>()
                .HasMany(p => p.Rules)
                .WithOne(r => r.Project)
                .HasForeignKey(r => r.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Project>()
                .HasOne(t => t.RolesLogins)
                .WithOne(p => p.Project)
                .HasForeignKey<RolesLogins>(k => k.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
