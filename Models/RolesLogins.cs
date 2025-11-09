using ERP_Proflipper_WorkspaceService.Models;
using System.Text.Json.Serialization;

namespace ERP_Proflipper_ProjectService.Models
{
    public class RolesLogins
    {
        public string? Id { get; set; }
        public string? ProjectId { get; set; }
        public string? BuilderLogin { get; set; }
        public string? LawyerLogin { get; set; }
        public string? FinancierLogin { get; set; }
        public string? ProjectManagerLogin { get; set; }

        [JsonIgnore]
        public Project Project { get; set; }
    }
}
