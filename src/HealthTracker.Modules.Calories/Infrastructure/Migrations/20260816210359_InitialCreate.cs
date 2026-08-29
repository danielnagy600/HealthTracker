using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthTracker.Modules.Calories.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        // CA1861: a CreateIndex minden meghívásnál új tömböt allokálna a helyben írt
        // literálból – ez a migráció ugyan csak egyszer fut le, de a statikus mező
        // olcsóbb és ugyanígy olvasható.
        private static readonly string[] EntriesUserIdDateColumns = { "UserId", "Date" };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "calories");

            migrationBuilder.CreateTable(
                name: "entries",
                schema: "calories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Meal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Calories = table.Column<int>(type: "integer", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "goals",
                schema: "calories",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DailyTargetKcal = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goals", x => x.UserId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_entries_UserId_Date",
                schema: "calories",
                table: "entries",
                columns: EntriesUserIdDateColumns);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entries",
                schema: "calories");

            migrationBuilder.DropTable(
                name: "goals",
                schema: "calories");
        }
    }
}
