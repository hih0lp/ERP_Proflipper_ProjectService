using FluentValidation;
using ERP_Proflipper_WorkspaceService.Models;


namespace ERP_Proflipper_ProjectService
{
    public class ProjectValidator : AbstractValidator<Project>
    {
        public ProjectValidator()
        {
            RuleFor(project => project.Name).NotEmpty();
            RuleFor(project => project.Status).NotEmpty();
            RuleFor(project => project.Area).NotEqual(0);
            RuleFor(project => project.Price).NotEqual(0);
            RuleFor(project => project.Location).NotEmpty();
            RuleFor(project => project.Condition).NotEmpty();
            RuleFor(project => project.Comment).NotEmpty();
            RuleFor(project => project.CreatedAt).NotEmpty();
            RuleFor(project => project.UpdatedAt).NotEmpty();
            RuleFor(project => project.UpdatedAt).NotEmpty();
        }
    }
}
