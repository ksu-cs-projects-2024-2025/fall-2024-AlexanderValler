using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlockOut.Migrations
{
    /// <inheritdoc />
    public partial class AddShiftConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.RenameColumn(
                name: "MinWorkers",
                table: "Shifts",
                newName: "TotalWeekHours");

            migrationBuilder.RenameColumn(
                name: "MaxWorkers",
                table: "Shifts",
                newName: "MaxWeeklyHoursPerPerson");

            migrationBuilder.AddColumn<string>(
                name: "BusinessId",
                table: "Shifts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MaxDailyHoursPerPerson",
                table: "Shifts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ShiftHourRequirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ShiftId = table.Column<int>(type: "INTEGER", nullable: false),
                    HourStartTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    HourEndTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    MinWorkers = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxWorkers = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftHourRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShiftHourRequirements_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_BusinessId",
                table: "Shifts",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftHourRequirements_ShiftId",
                table: "ShiftHourRequirements",
                column: "ShiftId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Businesses_BusinessId",
                table: "Shifts",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Businesses_BusinessId",
                table: "Shifts");

            migrationBuilder.DropTable(
                name: "ShiftHourRequirements");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_BusinessId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "MaxDailyHoursPerPerson",
                table: "Shifts");

            migrationBuilder.RenameColumn(
                name: "TotalWeekHours",
                table: "Shifts",
                newName: "MinWorkers");

            migrationBuilder.RenameColumn(
                name: "MaxWeeklyHoursPerPerson",
                table: "Shifts",
                newName: "MaxWorkers");

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GroupName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Time = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                });
        }
    }
}
