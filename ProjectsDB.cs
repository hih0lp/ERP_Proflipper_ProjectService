using ERP_Proflipper_ProjectService.Models;
using ERP_Proflipper_WorkspaceService.Models;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.EntityFrameworkCore;

namespace ERP_Proflipper_WorkspaceService
{
    public class ProjectsDB : DbContext
    {
        //public ProjectsDB() : base() { }

        public ProjectsDB(DbContextOptions<ProjectsDB> options) : base(options)
        {
            Console.WriteLine("dfkbnfoib");
            Database.EnsureCreated();
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<RolesRules> RolesRules { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=195.54.178.243; Port=27031; Database=ERP_PROJECTS; Username=admin; Password=Tandem_2025; Encoding=UTF8; Pooling=true");
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>().HasKey(p => p.Id);
            modelBuilder.Entity<RolesRules>().HasKey(r => r.Id);

            modelBuilder.Entity<Project>()
                .HasMany(p => p.Rules)
                .WithOne(r => r.Project)
                .HasForeignKey(r => r.ProjectId);
        }
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Project>()
        //        .HasOne(p => p.PMCardModel)
        //        .WithOne(p => p.Project)
        //        .HasForeignKey<PMCardModel>(t => t.ProjectId)
        //        .HasPrincipalKey<Project>(e => e.Id)
        //        .OnDelete(DeleteBehavior.Restrict);

        //    modelBuilder.Entity<Project>()
        //        .HasOne(p => p.FinancierCardModel)
        //        .WithOne(p => p.Project)
        //        .HasForeignKey<FinancierCardModel>(t => t.ProjectId)
        //        .HasPrincipalKey<Project>(e => e.Id)
        //        .OnDelete(DeleteBehavior.Restrict);


        //    modelBuilder.Entity<Project>()
        //        .HasOne(p => p.BuilderCardModel)
        //        .WithOne(p => p.Project)
        //        .HasForeignKey<BuilderCardModel>(t => t.ProjectId)
        //        .HasPrincipalKey<Project>(e => e.Id)
        //        .OnDelete(DeleteBehavior.Restrict);


        //    modelBuilder.Entity<Project>()
        //        .HasOne(p => p.LawyerCardModel)
        //        .WithOne(p => p.Project)
        //        .HasForeignKey<LawyerCardModel>(t => t.ProjectId)
        //        .HasPrincipalKey<Project>(e => e.Id)
        //        .OnDelete(DeleteBehavior.Restrict);
        //}
    }
}
