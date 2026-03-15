using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjeGorevYonetimi.Migrations
{
    /// <inheritdoc />
    public partial class GorevSorumluKullaniciId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SorumluKullaniciId",
                table: "Gorevler",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Gorevler_SorumluKullaniciId",
                table: "Gorevler",
                column: "SorumluKullaniciId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevler_Kullanicilar_SorumluKullaniciId",
                table: "Gorevler",
                column: "SorumluKullaniciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gorevler_Kullanicilar_SorumluKullaniciId",
                table: "Gorevler");

            migrationBuilder.DropIndex(
                name: "IX_Gorevler_SorumluKullaniciId",
                table: "Gorevler");

            migrationBuilder.DropColumn(
                name: "SorumluKullaniciId",
                table: "Gorevler");
        }
    }
}
