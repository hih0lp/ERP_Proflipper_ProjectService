using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP_Proflipper_ProjectService.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectResponsiblesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ТОЛЬКО создание новой таблицы ProjectResponsibles
            migrationBuilder.CreateTable(
                name: "ProjectResponsibles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<string>(type: "text", nullable: false),
                    ResponsibleName = table.Column<string>(type: "text", nullable: true),
                    ResponsibleRole = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectResponsibles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectResponsibles_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Создаем индекс для улучшения производительности
            migrationBuilder.CreateIndex(
                name: "IX_ProjectResponsibles_ProjectId",
                table: "ProjectResponsibles",
                column: "ProjectId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectResponsibles");
        }
    }
}
