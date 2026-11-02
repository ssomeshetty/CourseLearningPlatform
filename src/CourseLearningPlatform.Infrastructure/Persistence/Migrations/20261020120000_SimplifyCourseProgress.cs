using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseLeaningPlatform.Infrastructure.Persistence.Migrations;

public sealed partial class SimplifyCourseProgress : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Status-only progress no longer needs session history or accumulated study hours.
        migrationBuilder.DropTable(name: "LearningSessions");

        migrationBuilder.AddColumn<DateTime>(
            name: "CompletedAtUtc",
            table: "AssignedCourses",
            type: "datetime2",
            nullable: true);

        migrationBuilder.Sql("UPDATE [AssignedCourses] SET [CompletedAtUtc] = [EndedTimestamp] WHERE [Status] = N'Completed' AND [EndedTimestamp] IS NOT NULL");

        migrationBuilder.DropColumn(name: "StartedTimestamp", table: "AssignedCourses");
        migrationBuilder.DropColumn(name: "EndedTimestamp", table: "AssignedCourses");
        migrationBuilder.DropColumn(name: "HoursCompleted", table: "AssignedCourses");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "StartedTimestamp",
            table: "AssignedCourses",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<DateTime>(
            name: "EndedTimestamp",
            table: "AssignedCourses",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "HoursCompleted",
            table: "AssignedCourses",
            type: "decimal(8,2)",
            precision: 8,
            scale: 2,
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.Sql("UPDATE [AssignedCourses] SET [EndedTimestamp] = [CompletedAtUtc] WHERE [Status] = N'Completed' AND [CompletedAtUtc] IS NOT NULL");
        migrationBuilder.DropColumn(name: "CompletedAtUtc", table: "AssignedCourses");

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
                table.PrimaryKey("PK_LearningSessions", row => row.Id);
                table.ForeignKey(
                    name: "FK_LearningSessions_AssignedCourses_UserId_CourseId",
                    columns: row => new { row.UserId, row.CourseId },
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
}
