using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ScientificActivityDatabaseImplement.Migrations
{
    /// <inheritdoc />
    public partial class UpdateJournalForParser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JournalVakSpecialties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JournalId = table.Column<int>(type: "integer", nullable: false),
                    SpecialtyCode = table.Column<string>(type: "text", nullable: false),
                    SpecialtyName = table.Column<string>(type: "text", nullable: false),
                    ScienceBranch = table.Column<string>(type: "text", nullable: false),
                    DateFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalVakSpecialties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalVakSpecialties_Journals_JournalId",
                        column: x => x.JournalId,
                        principalTable: "Journals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JournalVakSpecialties_JournalId_SpecialtyCode_ScienceBranch~",
                table: "JournalVakSpecialties",
                columns: new[] { "JournalId", "SpecialtyCode", "ScienceBranch", "DateFrom", "DateTo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JournalVakSpecialties");
        }
    }
}
