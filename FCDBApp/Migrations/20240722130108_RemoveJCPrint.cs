using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCDBApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveJCPrint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BranchManagerPrint",
                table: "JobCards");

            migrationBuilder.DropColumn(
                name: "EngineerPrint",
                table: "JobCards");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BranchManagerPrint",
                table: "JobCards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EngineerPrint",
                table: "JobCards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
