using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillNet.Infrastructure.Migrations;

public partial class RestoreEvaluationPrecisionAndAddAdminTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(
            name: "OverallScore",
            table: "InterviewEvaluation",
            type: "decimal(4,2)",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.CreateTable(
            name: "AuditLog",
            columns: table => new
            {
                AuditLogId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<int>(type: "int", nullable: true),
                Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Entity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                EntityId = table.Column<int>(type: "int", nullable: true),
                OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                IPAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_AuditLog", x => x.AuditLogId));

        migrationBuilder.CreateTable(
            name: "Department",
            columns: table => new
            {
                DepartmentId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                OrganizationId = table.Column<int>(type: "int", nullable: false),
                DepartmentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Department", x => x.DepartmentId);
                table.ForeignKey("FK_Department_Organization_OrganizationId", x => x.OrganizationId,
                    "Organization", "OrganizationId", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "SystemConfiguration",
            columns: table => new
            {
                Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_SystemConfiguration", x => x.Key));

        migrationBuilder.CreateIndex("IX_AuditLog_Timestamp", "AuditLog", "Timestamp");
        migrationBuilder.CreateIndex("IX_AuditLog_UserId", "AuditLog", "UserId");
        migrationBuilder.CreateIndex(
            "IX_Department_OrganizationId_DepartmentName",
            "Department",
            new[] { "OrganizationId", "DepartmentName" },
            unique: true);

        migrationBuilder.InsertData(
            table: "SystemConfiguration",
            columns: new[] { "Key", "Value", "Description" },
            values: new object[,]
            {
                { "AllowMultipleApplications", "false", "Allow candidates to apply to the same job more than once." },
                { "RequireStrongPassword", "true", "Require the standard SkillNet password policy." }
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("AuditLog");
        migrationBuilder.DropTable("Department");
        migrationBuilder.DropTable("SystemConfiguration");

        migrationBuilder.AlterColumn<int>(
            name: "OverallScore",
            table: "InterviewEvaluation",
            type: "int",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(4,2)");
    }
}
