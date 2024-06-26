using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FCDBApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSitesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PassFailStatus",
                table: "InspectionTables",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SiteID",
                table: "InspectionTables",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Sites",
                columns: table => new
                {
                    SiteID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiteName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.SiteID);
                });

            migrationBuilder.InsertData(
                table: "Sites",
                columns: new[] { "SiteID", "SiteName" },
                values: new object[,]
                {
                    { 1, "Birmingham Loomis" },
                    { 2, "Colchester Loomis" },
                    { 3, "Dagenham Loomis" },
                    { 4, "Dunstable Loomis" },
                    { 5, "Edinburgh Loomis" },
                    { 6, "Elgin Loomis" },
                    { 7, "Exeter Loomis" },
                    { 8, "Glasgow Loomis" },
                    { 9, "Heathrow Loomis" },
                    { 10, "Leeds Loomis" },
                    { 11, "Maidstone Loomis" },
                    { 12, "Manchester Loomis" },
                    { 13, "Newcastle Loomis" },
                    { 14, "Newport Loomis" },
                    { 15, "Nottingham Loomis" },
                    { 16, "Sheppertion Loomis" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sites");

            migrationBuilder.DropColumn(
                name: "PassFailStatus",
                table: "InspectionTables");

            migrationBuilder.DropColumn(
                name: "SiteID",
                table: "InspectionTables");
        }
    }
}
