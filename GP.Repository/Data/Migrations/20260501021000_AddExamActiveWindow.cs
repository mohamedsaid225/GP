using System;
using GP.Repository.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GP.Repository.Data.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GPContext))]
    [Migration("20260501021000_AddExamActiveWindow")]
    public partial class AddExamActiveWindow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActiveFrom",
                table: "Exams",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActiveTo",
                table: "Exams",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveFrom",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "ActiveTo",
                table: "Exams");
        }
    }
}
