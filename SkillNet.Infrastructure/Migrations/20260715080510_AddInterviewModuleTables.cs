using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillNet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewModuleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Interview",
                columns: table => new
                {
                    InterviewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    InterviewType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InterviewRound = table.Column<int>(type: "int", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MeetingLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interview", x => x.InterviewId);
                });

            migrationBuilder.CreateTable(
                name: "Interviewer",
                columns: table => new
                {
                    InterviewerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interviewer", x => x.InterviewerId);
                });

            migrationBuilder.CreateTable(
                name: "InterviewAssignment",
                columns: table => new
                {
                    InterviewId = table.Column<int>(type: "int", nullable: false),
                    InterviewerId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewAssignment", x => new { x.InterviewId, x.InterviewerId });
                    table.ForeignKey(
                        name: "FK_InterviewAssignment_Interview_InterviewId",
                        column: x => x.InterviewId,
                        principalTable: "Interview",
                        principalColumn: "InterviewId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InterviewAssignment_Interviewer_InterviewerId",
                        column: x => x.InterviewerId,
                        principalTable: "Interviewer",
                        principalColumn: "InterviewerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterviewEvaluation",
                columns: table => new
                {
                    EvaluationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InterviewId = table.Column<int>(type: "int", nullable: false),
                    InterviewerId = table.Column<int>(type: "int", nullable: false),
                    TechnicalScore = table.Column<int>(type: "int", nullable: false),
                    CommunicationScore = table.Column<int>(type: "int", nullable: false),
                    ProblemSolvingScore = table.Column<int>(type: "int", nullable: false),
                    CultureFitScore = table.Column<int>(type: "int", nullable: false),
                    OverallScore = table.Column<int>(type: "int", nullable: false),
                    Recommendation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewEvaluation", x => x.EvaluationId);
                    table.ForeignKey(
                        name: "FK_InterviewEvaluation_Interview_InterviewId",
                        column: x => x.InterviewId,
                        principalTable: "Interview",
                        principalColumn: "InterviewId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InterviewEvaluation_Interviewer_InterviewerId",
                        column: x => x.InterviewerId,
                        principalTable: "Interviewer",
                        principalColumn: "InterviewerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterviewFeedbackHistory",
                columns: table => new
                {
                    HistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluationId = table.Column<int>(type: "int", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewFeedbackHistory", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_InterviewFeedbackHistory_InterviewEvaluation_EvaluationId",
                        column: x => x.EvaluationId,
                        principalTable: "InterviewEvaluation",
                        principalColumn: "EvaluationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewAssignment_InterviewerId",
                table: "InterviewAssignment",
                column: "InterviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewEvaluation_InterviewerId",
                table: "InterviewEvaluation",
                column: "InterviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewEvaluation_InterviewId",
                table: "InterviewEvaluation",
                column: "InterviewId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewFeedbackHistory_EvaluationId",
                table: "InterviewFeedbackHistory",
                column: "EvaluationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterviewAssignment");

            migrationBuilder.DropTable(
                name: "InterviewFeedbackHistory");

            migrationBuilder.DropTable(
                name: "InterviewEvaluation");

            migrationBuilder.DropTable(
                name: "Interview");

            migrationBuilder.DropTable(
                name: "Interviewer");
        }
    }
}
