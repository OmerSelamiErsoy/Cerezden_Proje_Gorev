using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjeGorevYonetimi.Migrations
{
    /// <inheritdoc />
    public partial class AddSahaDenetimLokasyon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LokasyonAdi",
                table: "SahaDenetimler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LokasyonId",
                table: "SahaDenetimler",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LokasyonAdi",
                table: "SahaDenetimler");

            migrationBuilder.DropColumn(
                name: "LokasyonId",
                table: "SahaDenetimler");
        }
    }
}
