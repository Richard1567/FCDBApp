using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCDBApi.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionTypeIDToInspectionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InspectionTypeID",
                table: "InspectionTables",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InspectionTypeID",
                table: "InspectionTables");
        }
    }
}
