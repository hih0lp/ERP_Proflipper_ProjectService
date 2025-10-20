using ERP_Proflipper_WorkspaceService.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP_Proflipper_ProjectService.Models
{
    public abstract class Rules
    {
        public bool CanRead{ get; set; }
        public bool CanWrite{ get; set; }
    }

    public class RolesRules : Rules
    {
        public int Id { get; set; }
        public string ProjectId { get; set; }
        public Project Project { get; set; }
        public string RoleName;
    }
}
