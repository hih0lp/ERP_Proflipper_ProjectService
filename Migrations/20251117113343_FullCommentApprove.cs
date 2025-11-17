using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_Proflipper_ProjectService.Migrations
{
    /// <inheritdoc />
    public partial class FullCommentApprove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullApproveComment",
                table: "Projects",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullApproveComment",
                table: "Projects");
        }
    }
}
