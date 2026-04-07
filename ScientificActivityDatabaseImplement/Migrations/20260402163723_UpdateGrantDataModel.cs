using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScientificActivityDatabaseImplement.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGrantDataModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContestNumber",
                table: "Grants",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContestNumber",
                table: "Grants");
        }
    }
}
