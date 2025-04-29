using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace project_model.Migrations
{
    /// <inheritdoc />
    public partial class FixDecimalValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "r_s",
                table: "Values",
                type: "numeric(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(3,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "n_e",
                table: "Values",
                type: "numeric(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(3,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "f_p",
                table: "Values",
                type: "numeric(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(3,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "f_l",
                table: "Values",
                type: "numeric(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(3,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "f_i",
                table: "Values",
                type: "numeric(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(3,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "f_c",
                table: "Values",
                type: "numeric(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(3,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "r_s",
                table: "Values",
                type: "numeric(3,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "n_e",
                table: "Values",
                type: "numeric(3,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "f_p",
                table: "Values",
                type: "numeric(3,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "f_l",
                table: "Values",
                type: "numeric(3,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "f_i",
                table: "Values",
                type: "numeric(3,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "f_c",
                table: "Values",
                type: "numeric(3,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)");
        }
    }
}
