using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FCDBApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InspectionCategories",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionCategories", x => x.CategoryID);
                });

            migrationBuilder.CreateTable(
                name: "InspectionTables",
                columns: table => new
                {
                    InspectionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Branch = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VehicleReg = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextInspectionDue = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmissionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionTables", x => x.InspectionID);
                });

            migrationBuilder.CreateTable(
                name: "InspectionTypes",
                columns: table => new
                {
                    InspectionTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionTypes", x => x.InspectionTypeID);
                });

            migrationBuilder.CreateTable(
                name: "JobCards",
                columns: table => new
                {
                    JobCardID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Site = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Engineer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustOrderNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Odometer = table.Column<long>(type: "bigint", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Time = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmissionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobCards", x => x.JobCardID);
                });

            migrationBuilder.CreateTable(
                name: "InspectionItems",
                columns: table => new
                {
                    InspectionItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    ItemDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InspectionTypeIndicator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InspectionTypeID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionItems", x => x.InspectionItemID);
                    table.ForeignKey(
                        name: "FK_InspectionItems_InspectionCategories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "InspectionCategories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InspectionItems_InspectionTypes_InspectionTypeID",
                        column: x => x.InspectionTypeID,
                        principalTable: "InspectionTypes",
                        principalColumn: "InspectionTypeID");
                });

            migrationBuilder.CreateTable(
                name: "PartsUsed",
                columns: table => new
                {
                    PartUsedID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobCardID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartsUsed", x => x.PartUsedID);
                    table.ForeignKey(
                        name: "FK_PartsUsed_JobCards_JobCardID",
                        column: x => x.JobCardID,
                        principalTable: "JobCards",
                        principalColumn: "JobCardID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InspectionDetails",
                columns: table => new
                {
                    InspectionDetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspectionItemID = table.Column<int>(type: "int", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionDetails", x => x.InspectionDetailID);
                    table.ForeignKey(
                        name: "FK_InspectionDetails_InspectionItems_InspectionItemID",
                        column: x => x.InspectionItemID,
                        principalTable: "InspectionItems",
                        principalColumn: "InspectionItemID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionDetails_InspectionTables_InspectionID",
                        column: x => x.InspectionID,
                        principalTable: "InspectionTables",
                        principalColumn: "InspectionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "InspectionCategories",
                columns: new[] { "CategoryID", "CategoryName" },
                values: new object[,]
                {
                    { 1, "Load Mission Disc and Log to Vehicle" },
                    { 2, "General Security Conditions" },
                    { 3, "Trunk Normal/Depot Mode Tests" },
                    { 4, "Trunk Unit Checks" },
                    { 5, "TrunkMode/Trunk Setting Test" },
                    { 6, "Return to Depot Mode Test" },
                    { 7, "H&S" },
                    { 8, "Depot Mode Settings" }
                });

            migrationBuilder.InsertData(
                table: "InspectionTypes",
                columns: new[] { "InspectionTypeID", "Frequency", "TypeName" },
                values: new object[,]
                {
                    { 1, "every 8 weeks or every 4 weeks", "OPV - every 8 Weeks or ExSSG - every 4 weeks" },
                    { 2, "every 26 weeks", "Coin vehicles - every 26 weeks" },
                    { 3, "every 4 weeks", "Trunkers - every 4 weeks" },
                    { 4, "every 8 weeks or every 4 weeks", "CIT - every 8 weeks or 816 - every 4 weeks" }
                });

            migrationBuilder.InsertData(
                table: "InspectionItems",
                columns: new[] { "InspectionItemID", "CategoryID", "InspectionTypeID", "InspectionTypeIndicator", "ItemDescription" },
                values: new object[,]
                {
                    { 1, 1, null, "134", "Open escape hatch check that the alarm sounds and engine immobilised" },
                    { 2, 1, null, "34", "Check engine won't start when immobiliser key is vertical" },
                    { 3, 1, null, "34", "Check 112/160 meters immobilised and alarms operates at correct distance" },
                    { 4, 1, null, "134", "Check all off mode escape hatches overlock" },
                    { 5, 1, null, "14", "Check pavement alarm system" },
                    { 6, 1, null, "134", "Check bulk head door locks are secure and immobilises when open" },
                    { 7, 1, null, "4", "Check and record date/time on computer. Reset if needed" },
                    { 8, 1, null, "134", "Check and record date and time on Finger scanner" },
                    { 9, 1, null, "34", "Check PQ bolts on outside of the door, operate alarm with 15mm+10mm of thread showing" },
                    { 10, 1, null, "34", "Check PQ internal bolts operate alarm when released" },
                    { 11, 1, null, "34", "Check Escape Hatch shoot bolt test activates alarm and remote panel LED" },
                    { 12, 1, null, "34", "Transfer Hatch locks" },
                    { 13, 1, null, "1234", "Check red Hijack buttons operate - GPS Tracking" },
                    { 14, 1, null, "1234", "Press green siren only button, alarm activates when button latches in and stops when button is out" },
                    { 15, 1, null, "1234", "Check black vehicle track buttons, operate GPS Tracking" },
                    { 16, 1, null, "124", "Check External Door latch and cylinder" },
                    { 17, 1, null, "24", "Check red Power light under LED Panel" },
                    { 18, 1, null, "24", "Check green lamp - external door button" },
                    { 19, 1, null, "24", "Check green lamp - Access Control door" },
                    { 20, 2, null, "1234", "Glazing" },
                    { 21, 2, null, "1234", "Cab doors" },
                    { 22, 2, null, "134", "Air lock doors" },
                    { 23, 2, null, "1234", "Rear doors" },
                    { 24, 2, null, "4", "Coin Pass through unit" },
                    { 25, 2, null, "3", "Under vehicle protection incl. tail lift" },
                    { 26, 2, null, "134", "Escape Hatch operation" },
                    { 27, 2, null, "134", "Escape Hatch security" },
                    { 28, 2, null, "1234", "Alarm system" },
                    { 29, 2, null, "1234", "Vehicle immobilisation" },
                    { 30, 2, null, "1234", "GPS tracking check to ARC" },
                    { 31, 2, null, "14", "Check CCTV operative and recording" },
                    { 32, 7, null, "1234", "Fire Extinguisher" },
                    { 33, 7, null, "1234", "First Aid Box" },
                    { 34, 7, null, "1234", "Check general interior panel condition, drivers seat and cab floor mat condition" },
                    { 35, 7, null, "14", "Check entrance door grab handle fitted, condition and it is secure" },
                    { 36, 8, null, "34", "Shut down" },
                    { 37, 8, null, "1234", "Vault lights" },
                    { 38, 3, null, "34", "Roof escape hatch" },
                    { 39, 3, null, "134", "Side door and escape route (full alarm immobilisation)" },
                    { 40, 3, null, "34", "Roof hatch spin bolts" },
                    { 41, 3, null, "34", "Door escape spin bolts" },
                    { 42, 3, null, "34", "Sounder Button (siren only)" },
                    { 43, 3, null, "34", "Vault 1 door open" },
                    { 44, 3, null, "34", "Vault 2 door open (cab module warn lamp operates)" },
                    { 45, 4, null, "34", "Warning lights working correctly" },
                    { 46, 4, null, "34", "Internal sounder operates" },
                    { 47, 4, null, "34", "Operation of custodian key switch" },
                    { 48, 5, null, "34", "No operation of doors/vaults door" },
                    { 49, 5, null, "34", "Hi-jack reset function" },
                    { 50, 5, null, "34", "Drivers door/airlock open" },
                    { 51, 5, null, "34", "Passenger door/airlock open" },
                    { 52, 5, null, "34", "Vault 1 door open" },
                    { 53, 5, null, "34", "Vault 2 door open" },
                    { 54, 5, null, "34", "Immobilisation (alarms activate mayday sent)" },
                    { 55, 5, null, "34", "Access limited 816" },
                    { 56, 5, null, "34", "Reset on key" },
                    { 57, 6, null, "34", "Tacho distance/speed activates" },
                    { 58, 6, null, "34", "Rear door opening" },
                    { 59, 6, null, "3", "Smoke cloak operation" },
                    { 60, 6, null, "34", "Distance travelled in TRUNK Mode" },
                    { 61, 6, null, "1234", "Check external Kaba key readers and cover condition" },
                    { 62, 6, null, "14", "Check entrance door lock operation and inner striker condition, check OEM check strap" },
                    { 63, 6, null, "1234", "Check internal escape overrides on cab doors, realign if required" },
                    { 64, 6, null, "14", "Test drop safe operation, and backplate is present and secure" },
                    { 65, 6, null, "14", "Check safe doors operation and backplate is present and secure" },
                    { 66, 6, null, "14", "Check rear coin area condition and divider secure" },
                    { 67, 6, null, "1234", "Check Vehicle & auxiliary battery condition, drop test" },
                    { 68, 6, null, "14", "Check all emergency security plates are present and secure" },
                    { 69, 6, null, "1234", "Lubricate locks and components as required" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionDetails_InspectionID",
                table: "InspectionDetails",
                column: "InspectionID");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionDetails_InspectionItemID",
                table: "InspectionDetails",
                column: "InspectionItemID");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionItems_CategoryID",
                table: "InspectionItems",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionItems_InspectionTypeID",
                table: "InspectionItems",
                column: "InspectionTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_PartsUsed_JobCardID",
                table: "PartsUsed",
                column: "JobCardID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InspectionDetails");

            migrationBuilder.DropTable(
                name: "PartsUsed");

            migrationBuilder.DropTable(
                name: "InspectionItems");

            migrationBuilder.DropTable(
                name: "InspectionTables");

            migrationBuilder.DropTable(
                name: "JobCards");

            migrationBuilder.DropTable(
                name: "InspectionCategories");

            migrationBuilder.DropTable(
                name: "InspectionTypes");
        }
    }
}
