using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace project_model.Migrations
{
    /// <inheritdoc />
    public partial class UserOriginTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserOrigin",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    EntryOrigin = table.Column<string>(type: "character varying(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOrigin", x => new { x.UserId, x.EntryOrigin });
                    table.ForeignKey(
                        name: "FK_UserOrigin_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserOrigin_Entry_EntryOrigin",
                        column: x => x.EntryOrigin,
                        principalTable: "Entry",
                        principalColumn: "origin",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserOrigin_EntryOrigin",
                table: "UserOrigin",
                column: "EntryOrigin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserOrigin_UserId",
                table: "UserOrigin",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserOrigin");
        }
    }
}
