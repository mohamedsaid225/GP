using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GP.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeExamDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamSessions_Exams_ExamId",
                table: "ExamSessions");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSessions_Exams_ExamId",
                table: "ExamSessions",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamSessions_Exams_ExamId",
                table: "ExamSessions");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSessions_Exams_ExamId",
                table: "ExamSessions",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
