using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseLeaningPlatform.Infrastructure.Persistence.Migrations;

public sealed partial class AddPasswordHash : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "Password",
            table: "UserProfiles",
            newName: "PasswordHash");

        // Clear any legacy plaintext password values. They cannot be used as hashes.
        migrationBuilder.Sql("UPDATE [UserProfiles] SET [PasswordHash] = NULL WHERE [PasswordHash] IS NOT NULL AND [PasswordHash] NOT LIKE 'PBKDF2-SHA256$%'");

        migrationBuilder.AlterColumn<string>(
            name: "PasswordHash",
            table: "UserProfiles",
            type: "nvarchar(512)",
            maxLength: 512,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "PasswordHash",
            table: "UserProfiles",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(512)",
            oldMaxLength: 512,
            oldNullable: true);

        migrationBuilder.RenameColumn(
            name: "PasswordHash",
            table: "UserProfiles",
            newName: "Password");
    }
}
