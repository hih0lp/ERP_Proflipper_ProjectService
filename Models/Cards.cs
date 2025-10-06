using ERP_Proflipper_WorkspaceService.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP_Proflipper_ProjectService.Models
{
    public interface CardModel { }

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



    //public class a
    //{
    //    public async void ajaj()
    //    {
    //        var t = new RolesRules()
    //    }
    //}


    public class PMCardModel : CardModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string PMCardJSON { get; set; }
        public Project Project { get; set; }
        public Rule PMCard { get; set; } = new();
    }

    public class FinancierCardModel : CardModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string FinancierCardJSON { get; set; }
        public Project Project { get; set; }
        public Rule FinancierCard { get; set; } = new();
    }

    public class BuilderCardModel : CardModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string BuilderCardJSON { get; set; }
        public Project Project { get; set; }
        public Rule BuilderCard { get; set; } = new();
    }

    public class LawyerCardModel : CardModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string LawyerCardJSON { get; set; }
        public Project Project { get; set; }
        public Rule LawyerCard { get; set; } = new();
    }

    public class Rule
    {
        public bool CanRead { get; set; } = false; //сделать систему доступа к проектам по ролям. НЕОБХОДИМЫ ПРАВИЛА!!!1
        public bool CanWrite { get; set; } = false;
    }
}
