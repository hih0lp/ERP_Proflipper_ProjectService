using ERP_Proflipper_ProjectService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ERP_Proflipper_WorkspaceService.Models
{
    public class Project
    {
        public string? Id { get; set; }
        public string? PMCardJson { get; set; }
        public string? FinancierCardJson { get; set; }
        public string? BuilderCardJson { get; set; }
        public string? LawyerCardJson { get; set; }
        public string NowStatus { get; set; } = "Потенциальный"; //put in some status
        public bool IsFinished { get; set; } = false;
        public bool IsArchived { get; set; } = false;
        public string? SubStatus { get; set; }
        public string? Responsible { get; set; }
        public int? ApproveStatus { get; set; } = 0;
        public List<RolesRules> Rules { get; set; }
        public string SellerCheckJson { get; set; }
    }
}
