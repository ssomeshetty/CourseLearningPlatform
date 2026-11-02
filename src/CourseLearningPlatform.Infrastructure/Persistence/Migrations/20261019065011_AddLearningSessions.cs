using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseLeaningPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearningSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningSessions_AssignedCourses_UserId_CourseId",
                        columns: x => new { x.UserId, x.CourseId },
                        principalTable: "AssignedCourses",
                        principalColumns: new[] { "UserId", "CourseId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningSessions_UserId_CourseId",
                table: "LearningSessions",
                columns: new[] { "UserId", "CourseId" },
                unique: true,
                filter: "[EndedAtUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LearningSessions_UserId_StartedAtUtc",
                table: "LearningSessions",
                columns: new[] { "UserId", "StartedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearningSessions");
        }
    }
}
