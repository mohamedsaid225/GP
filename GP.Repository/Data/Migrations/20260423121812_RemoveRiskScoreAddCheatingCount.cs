using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GP.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRiskScoreAddCheatingCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalRiskScore",
                table: "ExamSessions");

            migrationBuilder.DropColumn(
                name: "RiskLevel",
                table: "ExamSessions");

            migrationBuilder.DropColumn(
                name: "FinalRiskScore",
                table: "ExamReports");

            migrationBuilder.DropColumn(
                name: "RiskLevel",
                table: "ExamReports");

            migrationBuilder.AddColumn<int>(
                name: "CheatingCount",
                table: "ExamReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ScreenshotsUrl",
                table: "ExamReports",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheatingCount",
                table: "ExamReports");

            migrationBuilder.DropColumn(
                name: "ScreenshotsUrl",
                table: "ExamReports");

            migrationBuilder.AddColumn<decimal>(
                name: "FinalRiskScore",
                table: "ExamSessions",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0.00m);

            migrationBuilder.AddColumn<string>(
                name: "RiskLevel",
                table: "ExamSessions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Low");

            migrationBuilder.AddColumn<decimal>(
                name: "FinalRiskScore",
                table: "ExamReports",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0.00m);

            migrationBuilder.AddColumn<string>(
                name: "RiskLevel",
                table: "ExamReports",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Low");
        }
    }
}
