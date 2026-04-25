using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GP.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSuspiciousEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Confidence",
                table: "SuspiciousEvents");

            migrationBuilder.DropColumn(
                name: "EventType",
                table: "SuspiciousEvents");

            migrationBuilder.DropColumn(
                name: "FrameUrl",
                table: "SuspiciousEvents");

            migrationBuilder.DropColumn(
                name: "RiskScoreContribution",
                table: "SuspiciousEvents");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "SuspiciousEvents");

            migrationBuilder.RenameColumn(
                name: "MetadataJson",
                table: "SuspiciousEvents",
                newName: "ScreenshotsUrl");

            migrationBuilder.AddColumn<int>(
                name: "CheatingCount",
                table: "SuspiciousEvents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LeavingExamCount",
                table: "SuspiciousEvents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StudentName",
                table: "SuspiciousEvents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StudentUniversityID",
                table: "SuspiciousEvents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheatingCount",
                table: "SuspiciousEvents");

            migrationBuilder.DropColumn(
                name: "LeavingExamCount",
                table: "SuspiciousEvents");

            migrationBuilder.DropColumn(
                name: "StudentName",
                table: "SuspiciousEvents");

            migrationBuilder.DropColumn(
                name: "StudentUniversityID",
                table: "SuspiciousEvents");

            migrationBuilder.RenameColumn(
                name: "ScreenshotsUrl",
                table: "SuspiciousEvents",
                newName: "MetadataJson");

            migrationBuilder.AddColumn<double>(
                name: "Confidence",
                table: "SuspiciousEvents",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "SuspiciousEvents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FrameUrl",
                table: "SuspiciousEvents",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RiskScoreContribution",
                table: "SuspiciousEvents",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0.00m);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "SuspiciousEvents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "AI");
        }
    }
}
