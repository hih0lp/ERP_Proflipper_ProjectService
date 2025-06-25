using ERP_Proflipper_ProjectService;

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

    public class ProjectDAO
    {
        ProjectsDB _db = new ProjectsDB();
        ProjectValidator projectValidator = new ProjectValidator();
        public async void AddProjectInDB(Project project)
        {
            _db.Projects.Add(project);
            await _db.SaveChangesAsync();
        }
    }
}
