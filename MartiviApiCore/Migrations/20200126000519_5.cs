using Microsoft.EntityFrameworkCore.Migrations;

namespace MartiviApiCore.Migrations
{
    public partial class _5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Message",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "Side",
                table: "ChatMessage");

            migrationBuilder.AddColumn<string>(
                name: "DateTime",
                table: "ChatMessage",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImage",
                table: "ChatMessage",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Target",
                table: "ChatMessage",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TemplateType",
                table: "ChatMessage",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "ChatMessage",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "ChatMessage",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateTime",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "ProfileImage",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "Target",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "TemplateType",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "Text",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "ChatMessage");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "ChatMessage",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Side",
                table: "ChatMessage",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
