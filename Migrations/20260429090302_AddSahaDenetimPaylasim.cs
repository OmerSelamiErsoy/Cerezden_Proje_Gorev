using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjeGorevYonetimi.Migrations
{
    /// <inheritdoc />
    public partial class AddSahaDenetimPaylasim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "YetkiTipi",
                table: "SahaDenetimler",
                type: "int",
                nullable: false,
                // Var olan denetimler daha önce sadece oluşturan kişi tarafından görülüyordu.
                // 1 = Kişi bazlı (oluşturan kişi yine görünür/yetkili).
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "SahaDenetimYetkiKullanicilar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SahaDenetimId = table.Column<int>(type: "int", nullable: false),
                    KullaniciId = table.Column<int>(type: "int", nullable: false),
                    InsertDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InsertedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    DeletedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SahaDenetimYetkiKullanicilar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SahaDenetimYetkiKullanicilar_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SahaDenetimYetkiKullanicilar_SahaDenetimler_SahaDenetimId",
                        column: x => x.SahaDenetimId,
                        principalTable: "SahaDenetimler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SahaDenetimYetkiKullanicilar_KullaniciId",
                table: "SahaDenetimYetkiKullanicilar",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_SahaDenetimYetkiKullanicilar_SahaDenetimId",
                table: "SahaDenetimYetkiKullanicilar",
                column: "SahaDenetimId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SahaDenetimYetkiKullanicilar");

            migrationBuilder.DropColumn(
                name: "YetkiTipi",
                table: "SahaDenetimler");
        }
    }
}
