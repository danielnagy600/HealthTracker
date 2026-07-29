using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthTracker.Modules.Water.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "water");

            migrationBuilder.CreateTable(
                name: "intakes",
                schema: "water",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AmountMl = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_intakes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "settings",
                schema: "water",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DailyTargetMl = table.Column<int>(type: "integer", nullable: false),
                    WakeTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    SleepTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settings", x => x.UserId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_intakes_UserId_Date",
                schema: "water",
                table: "intakes",
                columns: new[] { "UserId", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "intakes",
                schema: "water");

            migrationBuilder.DropTable(
                name: "settings",
                schema: "water");
        }
    }
}
