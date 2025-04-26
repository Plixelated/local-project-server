using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace project_model.Migrations
{
    /// <inheritdoc />
    public partial class FixDuplicateFKValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "entry_origin1",
                table: "Values");

            migrationBuilder.RenameColumn(
                name: "entry_origin",
                table: "Values",
                newName: "EntryOrigin");

            migrationBuilder.RenameIndex(
                name: "IX_Values_entry_origin",
                table: "Values",
                newName: "IX_Values_EntryOrigin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EntryOrigin",
                table: "Values",
                newName: "entry_origin");

            migrationBuilder.RenameIndex(
                name: "IX_Values_EntryOrigin",
                table: "Values",
                newName: "IX_Values_entry_origin");

            migrationBuilder.AddColumn<int>(
                name: "entry_origin1",
                table: "Values",
                type: "integer",
                nullable: true);
        }
    }
}
