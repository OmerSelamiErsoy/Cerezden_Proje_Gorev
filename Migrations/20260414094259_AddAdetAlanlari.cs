using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjeGorevYonetimi.Migrations
{
    /// <inheritdoc />
    public partial class AddAdetAlanlari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Adet",
                table: "ProjeSablonDetaylar",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdetBirimiId",
                table: "ProjeSablonDetaylar",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Adet",
                table: "ProjeDetaylar",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdetBirimiId",
                table: "ProjeDetaylar",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjeSablonDetaylar_AdetBirimiId",
                table: "ProjeSablonDetaylar",
                column: "AdetBirimiId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjeDetaylar_AdetBirimiId",
                table: "ProjeDetaylar",
                column: "AdetBirimiId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjeDetaylar_AdetBirimleri_AdetBirimiId",
                table: "ProjeDetaylar",
                column: "AdetBirimiId",
                principalTable: "AdetBirimleri",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjeSablonDetaylar_AdetBirimleri_AdetBirimiId",
                table: "ProjeSablonDetaylar",
                column: "AdetBirimiId",
                principalTable: "AdetBirimleri",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjeDetaylar_AdetBirimleri_AdetBirimiId",
                table: "ProjeDetaylar");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjeSablonDetaylar_AdetBirimleri_AdetBirimiId",
                table: "ProjeSablonDetaylar");

            migrationBuilder.DropIndex(
                name: "IX_ProjeSablonDetaylar_AdetBirimiId",
                table: "ProjeSablonDetaylar");

            migrationBuilder.DropIndex(
                name: "IX_ProjeDetaylar_AdetBirimiId",
                table: "ProjeDetaylar");

            migrationBuilder.DropColumn(
                name: "Adet",
                table: "ProjeSablonDetaylar");

            migrationBuilder.DropColumn(
                name: "AdetBirimiId",
                table: "ProjeSablonDetaylar");

            migrationBuilder.DropColumn(
                name: "Adet",
                table: "ProjeDetaylar");

            migrationBuilder.DropColumn(
                name: "AdetBirimiId",
                table: "ProjeDetaylar");
        }
    }
}
