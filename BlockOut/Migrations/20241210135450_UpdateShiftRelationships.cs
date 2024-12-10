using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlockOut.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShiftRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Calendars_CalendarId",
                table: "Shifts");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Calendars_CalendarId",
                table: "Shifts",
                column: "CalendarId",
                principalTable: "Calendars",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Calendars_CalendarId",
                table: "Shifts");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Calendars_CalendarId",
                table: "Shifts",
                column: "CalendarId",
                principalTable: "Calendars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
