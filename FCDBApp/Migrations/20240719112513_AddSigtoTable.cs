using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCDBApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSigtoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InspectionTables_Signatures_BranchManagerSignatureID",
                table: "InspectionTables");

            migrationBuilder.DropForeignKey(
                name: "FK_InspectionTables_Signatures_EngineerSignatureID",
                table: "InspectionTables");

            migrationBuilder.DropIndex(
                name: "IX_InspectionTables_BranchManagerSignatureID",
                table: "InspectionTables");

            migrationBuilder.DropIndex(
                name: "IX_InspectionTables_EngineerSignatureID",
                table: "InspectionTables");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_InspectionTables_BranchManagerSignatureID",
                table: "InspectionTables",
                column: "BranchManagerSignatureID");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTables_EngineerSignatureID",
                table: "InspectionTables",
                column: "EngineerSignatureID");

            migrationBuilder.AddForeignKey(
                name: "FK_InspectionTables_Signatures_BranchManagerSignatureID",
                table: "InspectionTables",
                column: "BranchManagerSignatureID",
                principalTable: "Signatures",
                principalColumn: "SignatureID");

            migrationBuilder.AddForeignKey(
                name: "FK_InspectionTables_Signatures_EngineerSignatureID",
                table: "InspectionTables",
                column: "EngineerSignatureID",
                principalTable: "Signatures",
                principalColumn: "SignatureID");
        }
    }
}
