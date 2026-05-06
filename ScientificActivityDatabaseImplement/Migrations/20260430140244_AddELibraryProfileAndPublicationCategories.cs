using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ScientificActivityDatabaseImplement.Migrations
{
    /// <inheritdoc />
    public partial class AddELibraryProfileAndPublicationCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Publications_ResearcherId",
                table: "Publications");

            migrationBuilder.AddColumn<int>(
                name: "CitationsRincCount",
                table: "Publications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ELibraryId",
                table: "Publications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInCoreRinc",
                table: "Publications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInRinc",
                table: "Publications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRsci",
                table: "Publications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsScopusQ1",
                table: "Publications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsScopusQ2",
                table: "Publications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsScopusQ3",
                table: "Publications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsScopusQ4",
                table: "Publications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVak",
                table: "Publications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVakCategory1",
                table: "Publications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVakCategory2",
                table: "Publications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVakCategory3",
                table: "Publications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWebOfScienceNoQuartile",
                table: "Publications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWebOfScienceQ1",
                table: "Publications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWebOfScienceQ2",
                table: "Publications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWebOfScienceQ3",
                table: "Publications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWebOfScienceQ4",
                table: "Publications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWhiteListLevel1",
                table: "Publications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWhiteListLevel2",
                table: "Publications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWhiteListLevel3",
                table: "Publications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWhiteListLevel4",
                table: "Publications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RubricAsjc",
                table: "Publications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RubricGrnti",
                table: "Publications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RubricOecd",
                table: "Publications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VakSpecialty",
                table: "Publications",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ELibraryAuthorProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResearcherId = table.Column<int>(type: "integer", nullable: false),
                    AuthorId = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Organization = table.Column<string>(type: "text", nullable: true),
                    Department = table.Column<string>(type: "text", nullable: true),
                    SpinCode = table.Column<string>(type: "text", nullable: true),
                    PublicationsCountElibrary = table.Column<int>(type: "integer", nullable: true),
                    PublicationsCountRinc = table.Column<int>(type: "integer", nullable: true),
                    PublicationsCoreRincCount = table.Column<int>(type: "integer", nullable: true),
                    CitationsCountElibrary = table.Column<int>(type: "integer", nullable: true),
                    CitationsCountRinc = table.Column<int>(type: "integer", nullable: true),
                    CitationsCoreRincCount = table.Column<int>(type: "integer", nullable: true),
                    HIndexElibrary = table.Column<int>(type: "integer", nullable: true),
                    HIndexRinc = table.Column<int>(type: "integer", nullable: true),
                    HIndexCoreRinc = table.Column<int>(type: "integer", nullable: true),
                    HIndexWithoutSelfCitations = table.Column<int>(type: "integer", nullable: true),
                    PublicationsCitingAuthorCount = table.Column<int>(type: "integer", nullable: true),
                    MostCitedPublicationCitationsCount = table.Column<int>(type: "integer", nullable: true),
                    CitedPublicationsCount = table.Column<int>(type: "integer", nullable: true),
                    AverageCitationsPerPublication = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FirstPublicationYear = table.Column<int>(type: "integer", nullable: true),
                    SelfCitationsCount = table.Column<int>(type: "integer", nullable: true),
                    CoauthorCitationsCount = table.Column<int>(type: "integer", nullable: true),
                    CoauthorsCount = table.Column<int>(type: "integer", nullable: true),
                    ForeignArticlesCount = table.Column<int>(type: "integer", nullable: true),
                    RussianArticlesCount = table.Column<int>(type: "integer", nullable: true),
                    VakArticlesCount = table.Column<int>(type: "integer", nullable: true),
                    ImpactFactorArticlesCount = table.Column<int>(type: "integer", nullable: true),
                    ForeignJournalCitationsCount = table.Column<int>(type: "integer", nullable: true),
                    RussianJournalCitationsCount = table.Column<int>(type: "integer", nullable: true),
                    VakJournalCitationsCount = table.Column<int>(type: "integer", nullable: true),
                    ImpactFactorJournalCitationsCount = table.Column<int>(type: "integer", nullable: true),
                    AverageWeightedImpactFactorPublished = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    AverageWeightedImpactFactorCited = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PublicationsRincLast5YearsCount = table.Column<int>(type: "integer", nullable: true),
                    PublicationsCoreRincLast5YearsCount = table.Column<int>(type: "integer", nullable: true),
                    CitationsRincLast5YearsCount = table.Column<int>(type: "integer", nullable: true),
                    CitationsCoreRincLast5YearsCount = table.Column<int>(type: "integer", nullable: true),
                    CitationsAllLast5YearsCount = table.Column<int>(type: "integer", nullable: true),
                    MainRubricGrnti = table.Column<string>(type: "text", nullable: true),
                    MainRubricOecd = table.Column<string>(type: "text", nullable: true),
                    PercentileCoreRinc = table.Column<int>(type: "integer", nullable: true),
                    PublicationsRincByYearJson = table.Column<string>(type: "text", nullable: true),
                    PublicationsCoreRincByYearJson = table.Column<string>(type: "text", nullable: true),
                    CitationsRincByYearJson = table.Column<string>(type: "text", nullable: true),
                    CitationsCoreRincByYearJson = table.Column<string>(type: "text", nullable: true),
                    HIndexRincByYearJson = table.Column<string>(type: "text", nullable: true),
                    HIndexCoreRincByYearJson = table.Column<string>(type: "text", nullable: true),
                    PercentileCoreRincByYearJson = table.Column<string>(type: "text", nullable: true),
                    PublicationsRinc5YearsByEndYearJson = table.Column<string>(type: "text", nullable: true),
                    PublicationsCoreRinc5YearsByEndYearJson = table.Column<string>(type: "text", nullable: true),
                    CitationsRinc5YearsByEndYearJson = table.Column<string>(type: "text", nullable: true),
                    CitationsCoreRinc5YearsByEndYearJson = table.Column<string>(type: "text", nullable: true),
                    ResearchTopics = table.Column<string>(type: "text", nullable: true),
                    ImportedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ELibraryAuthorProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ELibraryAuthorProfiles_Researchers_ResearcherId",
                        column: x => x.ResearcherId,
                        principalTable: "Researchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Publications_ELibraryId",
                table: "Publications",
                column: "ELibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_Publications_ResearcherId_ELibraryId",
                table: "Publications",
                columns: new[] { "ResearcherId", "ELibraryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ELibraryAuthorProfiles_AuthorId",
                table: "ELibraryAuthorProfiles",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_ELibraryAuthorProfiles_ResearcherId",
                table: "ELibraryAuthorProfiles",
                column: "ResearcherId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ELibraryAuthorProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Publications_ELibraryId",
                table: "Publications");

            migrationBuilder.DropIndex(
                name: "IX_Publications_ResearcherId_ELibraryId",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "CitationsRincCount",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "ELibraryId",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "IsInCoreRinc",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "IsInRinc",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "IsRsci",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "IsScopusQ1",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "IsScopusQ2",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "IsScopusQ3",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "IsScopusQ4",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "IsVak",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "IsVakCategory1",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "IsVakCategory2",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "IsVakCategory3",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "IsWebOfScienceNoQuartile",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "IsWebOfScienceQ1",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "IsWebOfScienceQ2",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "IsWebOfScienceQ3",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "IsWebOfScienceQ4",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "IsWhiteListLevel1",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "IsWhiteListLevel2",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "IsWhiteListLevel3",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "IsWhiteListLevel4",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "RubricAsjc",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "RubricGrnti",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "RubricOecd",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "VakSpecialty",
                table: "Publications");

            migrationBuilder.CreateIndex(
                name: "IX_Publications_ResearcherId",
                table: "Publications",
                column: "ResearcherId");
        }
    }
}
