using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace project_model.Migrations
{
    /// <inheritdoc />
    public partial class FixEntryOriginFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entry_Submission",
                table: "Values");

            migrationBuilder.RenameColumn(
                name: "EntryOrigin",
                table: "Values",
                newName: "entry_origin");

            migrationBuilder.RenameIndex(
                name: "IX_Values_EntryOrigin",
                table: "Values",
                newName: "IX_Values_entry_origin");

            migrationBuilder.AlterColumn<string>(
                name: "entry_origin",
                table: "Values",
                type: "character varying(255)",
                unicode: false,
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "entry_origin1",
                table: "Values",
                type: "integer",
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Entry_origin",
                table: "Entry",
                column: "origin");

            migrationBuilder.AddForeignKey(
                name: "FK_Entry_Submission",
                table: "Values",
                column: "entry_origin",
                principalTable: "Entry",
                principalColumn: "origin",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entry_Submission",
                table: "Values");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Entry_origin",
                table: "Entry");

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

            migrationBuilder.AlterColumn<int>(
                name: "EntryOrigin",
                table: "Values",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldUnicode: false,
                oldMaxLength: 255);

            migrationBuilder.AddForeignKey(
                name: "FK_Entry_Submission",
                table: "Values",
                column: "EntryOrigin",
                principalTable: "Entry",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
