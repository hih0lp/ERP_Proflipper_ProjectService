using ERP_Proflipper_ProjectService.Repositories.Interface;
using ERP_Proflipper_WorkspaceService;
using ERP_Proflipper_WorkspaceService.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP_Proflipper_ProjectService.Repositories.Ports
{
    public class ProjectRepository : IProjectRepository //TODO
    {
        private ProjectsDB _context;
        public ProjectRepository(ProjectsDB context)
        {
            _context = context;
        }

        public async Task AddProjectInDB(Project project)
        {

            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Project modifiedProject)
        {

            //var changableProject = await _context.Projects.FirstOrDefaultAsync(x => x.Id == modifiedProject.Id);

            _context.Projects.Update(modifiedProject);
            await _context.SaveChangesAsync();

        }

        public async Task<bool> DeleteProjectAsync(string id)
        {
            var project = await _context.Projects.Include(p => p.Rules).FirstOrDefaultAsync(p => p.Id == id);
            if (project == null) return false;

            _context.Remove<Project>(project);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Project> GetProjectByIdAsync(string id)
        {
            try
            {
                return await _context.Projects
                .Include(x => x.Rules)
                .Include(x => x.RolesLogins)
                .FirstOrDefaultAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
            
        }

        public async Task<List<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects
                .Include(x => x.Rules)
                .Include(x => x.RolesLogins)
                .ToListAsync();
        }

        public async Task<List<Project>> GetAllProjectsByRoleAsync(string role) //getting all projects by user role
        {
            var projectsList = await _context.Projects
                .Where(x => x.Rules.Any(r => r.RoleName == role && r.CanRead == true) && x.IsArchived == false && x.IsFinished == false)
                .Include(x => x.Rules)
                .Include(x => x.RolesLogins)
                .ToListAsync();

            return projectsList;
        }

        public async Task<string> GetProjectCardByRoleAndId(string projectId, string role) //getting card project by role and project id
        {
            var project = await GetProjectByIdAsync(projectId);
            if (project is null) return null;

            return role switch
            {
                "ProjectManager" => project.PMCardJson,
                "Builder" => project.BuilderCardJson,
                "Financier" => project.FinancierCardJson,
                "Lawyer" => project.LawyerCardJson
            };
        }

        public async Task<List<Project>> GetProjectsByUserLogin(string login)
        {
            return await _context.Projects
                .Include(x => x.Rules)
                .Include(x => x.RolesLogins)
                .Where(x => (x.RolesLogins.FinancierLogin == login || x.RolesLogins.BuilderLogin == login || x.RolesLogins.LawyerLogin == login || x.RolesLogins.ProjectManagerLogin == login))
                .ToListAsync();
        }

        public async Task<List<Project>> GetAllProjectsByStatus(string status)
        {
            var projects = await _context.Projects.Where(x => x.NowStatus == status).Include(x => x.Rules).Include(x => x.RolesLogins).ToListAsync();

            return projects;
        }
    }
}
