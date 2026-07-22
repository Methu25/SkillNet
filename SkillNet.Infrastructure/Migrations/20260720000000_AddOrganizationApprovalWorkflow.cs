using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SkillNet.Infrastructure.Data;

#nullable disable

namespace SkillNet.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260720000000_AddOrganizationApprovalWorkflow")]
    public partial class AddOrganizationApprovalWorkflow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "Organization",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Approved");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Organization",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "Organization",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "Organization",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApprovalStatus",
                table: "Organization",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Draft",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_Organization_ApprovalStatus",
                table: "Organization",
                column: "ApprovalStatus");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Organization_ApprovalStatus",
                table: "Organization");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Organization");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Organization");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Organization");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "Organization");
        }
    }
}
