using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseLeaningPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedCourses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssignedCourses",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartedTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HoursCompleted = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignedCourses", x => new { x.UserId, x.CourseId });
                    table.ForeignKey(
                        name: "FK_AssignedCourses_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssignedCourses_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignedCourses_CourseId",
                table: "AssignedCourses",
                column: "CourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignedCourses");
        }
    }
}
