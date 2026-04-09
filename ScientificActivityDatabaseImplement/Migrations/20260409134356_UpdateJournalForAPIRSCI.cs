using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScientificActivityDatabaseImplement.Migrations
{
    /// <inheritdoc />
    public partial class UpdateJournalForAPIRSCI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quartile",
                table: "Journals");

            migrationBuilder.AddColumn<DateTime>(
                name: "WhiteListAcceptedDate",
                table: "Journals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WhiteListDiscontinuedDate",
                table: "Journals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WhiteListLevel2023",
                table: "Journals",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WhiteListLevel2025",
                table: "Journals",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhiteListNotice",
                table: "Journals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhiteListState",
                table: "Journals",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WhiteListAcceptedDate",
                table: "Journals");

            migrationBuilder.DropColumn(
                name: "WhiteListDiscontinuedDate",
                table: "Journals");

            migrationBuilder.DropColumn(
                name: "WhiteListLevel2023",
                table: "Journals");

            migrationBuilder.DropColumn(
                name: "WhiteListLevel2025",
                table: "Journals");

            migrationBuilder.DropColumn(
                name: "WhiteListNotice",
                table: "Journals");

            migrationBuilder.DropColumn(
                name: "WhiteListState",
                table: "Journals");

            migrationBuilder.AddColumn<int>(
                name: "Quartile",
                table: "Journals",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
