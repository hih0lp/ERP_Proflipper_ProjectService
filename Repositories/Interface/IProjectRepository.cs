using ERP_Proflipper_WorkspaceService.Models;

namespace ERP_Proflipper_ProjectService.Repositories.Interface
{
    public interface IProjectRepository
    {
        public Task AddProjectInDB(Project project);
        public Task UpdateAsync(Project modifiedProject);
        //public void ChangeProjectStatus(Project project, string nextStatus);
        public Task<bool> DeleteProjectAsync(string id);
        public Task<Project> GetProjectByIdAsync(string id);
        public Task<List<Project>> GetAllProjectsAsync();
        public Task<List<Project>> GetAllProjectsByRoleAsync(string role);
        public Task<string> GetProjectCardByRoleAndId(string projectId, string role);
        public Task<List<Project>> GetAllProjectsByStatus(string status);
        public Task<List<Project>> GetProjectsByUserLogin(string login);
    }
}
