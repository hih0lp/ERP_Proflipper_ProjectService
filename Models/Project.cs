using ERP_Proflipper_ProjectService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ERP_Proflipper_ProjectService.Models
{
    public class Project
    {
        public string? Id { get; set; }
        public string? PMCardJson { get; set; }
        public string? FinancierCardJson { get; set; }
        public string? BuilderCardJson { get; set; }
        public string? LawyerCardJson { get; set; }
        public string NowStatus { get; set; } = "Potential"; 
        public bool IsFinished { get; set; } = false;
        public bool IsArchived { get; set; } = false;
        public string? SubStatus { get; set; }
        public int? ApproveStatus { get; set; } = 0;
        public string? FullApproveComment { get; set; }
        public List<ProjectResponsibles>? Responsibles { get; set; } = new();
        public List<RolesRules>? Rules { get; set; }
        public RolesLogins? RolesLogins { get; set; } 
        public string? SellerCheckJson { get; set; }
        public string? CreatedAt { get; set; }
        public string? FinancierStatus { get; set; }
        public string? BuilderStatus { get; set; }
        public string? LawyerStatus { get; set; }
    }
}
