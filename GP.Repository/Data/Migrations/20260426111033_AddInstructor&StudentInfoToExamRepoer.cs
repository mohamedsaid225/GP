using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GP.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInstructorStudentInfoToExamRepoer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstructorCode",
                table: "ExamReports",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstructorName",
                table: "ExamReports",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentName",
                table: "ExamReports",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentUniversityId",
                table: "ExamReports",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstructorCode",
                table: "ExamReports");

            migrationBuilder.DropColumn(
                name: "InstructorName",
                table: "ExamReports");

            migrationBuilder.DropColumn(
                name: "StudentName",
                table: "ExamReports");

            migrationBuilder.DropColumn(
                name: "StudentUniversityId",
                table: "ExamReports");
        }
    }
}
