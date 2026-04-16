using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjeGorevYonetimi.Migrations
{
    /// <inheritdoc />
    public partial class AddProjeDetayFiyatAlanlari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CikisFiyati",
                table: "ProjeDetaylar",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaliyetFiyati",
                table: "ProjeDetaylar",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CikisFiyati",
                table: "ProjeDetaylar");

            migrationBuilder.DropColumn(
                name: "MaliyetFiyati",
                table: "ProjeDetaylar");
        }
    }
}
