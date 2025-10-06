using FluentValidation;
using ERP_Proflipper_WorkspaceService.Models;


namespace ERP_Proflipper_ProjectService
{
    public class ProjectValidator : AbstractValidator<Project>
    {
        public ProjectValidator()
        {
            RuleFor(project => project.NowStatus).NotEmpty();
        }
    }
}
