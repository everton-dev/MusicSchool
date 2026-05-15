using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicSchool.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherAvailabilityAndScheduleMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AbsenceReason",
                table: "Teachers",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Teachers",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "RecurrenceRule",
                table: "Lessons",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Weekly");

            migrationBuilder.CreateTable(
                name: "TeacherPauses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    StartsOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndsOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherPauses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherPauses_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherPauses_TeacherId",
                table: "TeacherPauses",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherPauses_TenantId_TeacherId_IsActive",
                table: "TeacherPauses",
                columns: new[] { "TenantId", "TeacherId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeacherPauses");

            migrationBuilder.DropColumn(
                name: "AbsenceReason",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "RecurrenceRule",
                table: "Lessons");
        }
    }
}
