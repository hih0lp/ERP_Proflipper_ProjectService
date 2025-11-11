using ERP_Proflipper_WorkspaceService.Models;
using System.Text.Json.Serialization;

namespace ERP_Proflipper_ProjectService.Models
{
    public class ProjectResponsibles
    {
        public int Id { get; set; }
        public string? ProjectId { get; set; }
        public string? ResponsibleName { get; set; }
        public string ResponsibleRole { get; set; }
        [JsonIgnore]
        public Project? Project { get; set; }
    }
}
