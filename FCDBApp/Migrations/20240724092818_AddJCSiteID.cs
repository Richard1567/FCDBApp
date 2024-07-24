using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCDBApi.Migrations
{
    /// <inheritdoc />
    public partial class AddJCSiteID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SiteID",
                table: "JobCards",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SiteID",
                table: "JobCards");
        }
    }
}
