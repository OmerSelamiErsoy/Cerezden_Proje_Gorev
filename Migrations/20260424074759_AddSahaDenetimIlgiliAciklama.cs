using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjeGorevYonetimi.Migrations
{
    /// <inheritdoc />
    public partial class AddSahaDenetimIlgiliAciklama : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Aciklama",
                table: "SahaDenetimAdimIlgiliUrunler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Aciklama",
                table: "SahaDenetimAdimIlgiliKisiler",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aciklama",
                table: "SahaDenetimAdimIlgiliUrunler");

            migrationBuilder.DropColumn(
                name: "Aciklama",
                table: "SahaDenetimAdimIlgiliKisiler");
        }
    }
}
