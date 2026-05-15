using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicSchool.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdentityFieldsAndRelationshipConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "BirthDate",
                table: "Users",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentType",
                table: "Users",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "CC");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_UserId",
                table: "Teachers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_UserId",
                table: "Students",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyRelationships_GuardianUserId",
                table: "FamilyRelationships",
                column: "GuardianUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyRelationships_StudentId",
                table: "FamilyRelationships",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyRelationships_Students_StudentId",
                table: "FamilyRelationships",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyRelationships_Users_GuardianUserId",
                table: "FamilyRelationships",
                column: "GuardianUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Users_UserId",
                table: "Students",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Users_UserId",
                table: "Teachers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FamilyRelationships_Students_StudentId",
                table: "FamilyRelationships");

            migrationBuilder.DropForeignKey(
                name: "FK_FamilyRelationships_Users_GuardianUserId",
                table: "FamilyRelationships");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Users_UserId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Users_UserId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_UserId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Students_UserId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_FamilyRelationships_GuardianUserId",
                table: "FamilyRelationships");

            migrationBuilder.DropIndex(
                name: "IX_FamilyRelationships_StudentId",
                table: "FamilyRelationships");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DocumentType",
                table: "Users");
        }
    }
}
