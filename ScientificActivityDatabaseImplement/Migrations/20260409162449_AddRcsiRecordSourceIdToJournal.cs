using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScientificActivityDatabaseImplement.Migrations
{
    /// <inheritdoc />
    public partial class AddRcsiRecordSourceIdToJournal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RcsiRecordSourceId",
                table: "Journals",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Journals_Issn",
                table: "Journals",
                column: "Issn");

            migrationBuilder.CreateIndex(
                name: "IX_Journals_RcsiRecordSourceId",
                table: "Journals",
                column: "RcsiRecordSourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Journals_Issn",
                table: "Journals");

            migrationBuilder.DropIndex(
                name: "IX_Journals_RcsiRecordSourceId",
                table: "Journals");

            migrationBuilder.DropColumn(
                name: "RcsiRecordSourceId",
                table: "Journals");
        }
    }
}
