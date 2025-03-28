using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace project_model.Migrations
{
    /// <inheritdoc />
    public partial class VariablesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Entry",
                newName: "id");

            migrationBuilder.AddColumn<string>(
                name: "origin",
                table: "Entry",
                type: "character varying(255)",
                unicode: false,
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Values",
                columns: table => new
                {
                    submission_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    r_s = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    f_p = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    n_e = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    f_l = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    f_i = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    f_c = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    l = table.Column<long>(type: "bigint", nullable: false),
                    EntryOrigin = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Values", x => x.submission_id);
                    table.CheckConstraint("CK_NonNegative_FractionCommunication_Values_Only", "f_c >= 0");
                    table.CheckConstraint("CK_NonNegative_FractionIntelligence_Values_Only", "f_i >= 0");
                    table.CheckConstraint("CK_NonNegative_FractionLife_Values_Only", "f_l >= 0");
                    table.CheckConstraint("CK_NonNegative_FrequencyPlanet_Values_Only", "f_p >= 0");
                    table.CheckConstraint("CK_NonNegative_Length_Values_Only", "l >= 0");
                    table.CheckConstraint("CK_NonNegative_NearEarth_Values_Only", "n_e >= 0");
                    table.CheckConstraint("CK_NonNegative_RateStars_Values_Only", "r_s >= 0");
                    table.ForeignKey(
                        name: "FK_Entry_Submission",
                        column: x => x.EntryOrigin,
                        principalTable: "Entry",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Values_EntryOrigin",
                table: "Values",
                column: "EntryOrigin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Values");

            migrationBuilder.DropColumn(
                name: "origin",
                table: "Entry");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Entry",
                newName: "Id");
        }
    }
}
