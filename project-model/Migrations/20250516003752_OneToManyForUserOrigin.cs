using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace project_model.Migrations
{
    /// <inheritdoc />
    public partial class OneToManyForUserOrigin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserOrigin_EntryOrigin",
                table: "UserOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_UserOrigin_EntryOrigin",
                table: "UserOrigin",
                column: "EntryOrigin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserOrigin_EntryOrigin",
                table: "UserOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_UserOrigin_EntryOrigin",
                table: "UserOrigin",
                column: "EntryOrigin",
                unique: true);
        }
    }
}
