using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjeGorevYonetimi.Migrations
{
    /// <inheritdoc />
    public partial class AddSahaDenetimModuleCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SahaDenetimler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OlusturanKullaniciId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_SahaDenetimler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SahaDenetimler_Kullanicilar_OlusturanKullaniciId",
                        column: x => x.OlusturanKullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SahaDenetimAdimlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SahaDenetimId = table.Column<int>(type: "int", nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SahaDenetimKategoriId = table.Column<int>(type: "int", nullable: true),
                    Tipler = table.Column<int>(type: "int", nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    YapildiMi = table.Column<bool>(type: "bit", nullable: false),
                    Puan = table.Column<int>(type: "int", nullable: true),
                    PersonelYorum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Not = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_SahaDenetimAdimlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SahaDenetimAdimlar_SahaDenetimKategoriler_SahaDenetimKategoriId",
                        column: x => x.SahaDenetimKategoriId,
                        principalTable: "SahaDenetimKategoriler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SahaDenetimAdimlar_SahaDenetimler_SahaDenetimId",
                        column: x => x.SahaDenetimId,
                        principalTable: "SahaDenetimler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SahaDenetimAdimIlgiliKisiler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SahaDenetimAdimId = table.Column<int>(type: "int", nullable: false),
                    AdSoyad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_SahaDenetimAdimIlgiliKisiler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SahaDenetimAdimIlgiliKisiler_SahaDenetimAdimlar_SahaDenetimAdimId",
                        column: x => x.SahaDenetimAdimId,
                        principalTable: "SahaDenetimAdimlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SahaDenetimAdimIlgiliUrunler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SahaDenetimAdimId = table.Column<int>(type: "int", nullable: false),
                    StokKodu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Adet = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_SahaDenetimAdimIlgiliUrunler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SahaDenetimAdimIlgiliUrunler_SahaDenetimAdimlar_SahaDenetimAdimId",
                        column: x => x.SahaDenetimAdimId,
                        principalTable: "SahaDenetimAdimlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SahaDenetimAdimIlgiliKisiler_SahaDenetimAdimId",
                table: "SahaDenetimAdimIlgiliKisiler",
                column: "SahaDenetimAdimId");

            migrationBuilder.CreateIndex(
                name: "IX_SahaDenetimAdimIlgiliUrunler_SahaDenetimAdimId",
                table: "SahaDenetimAdimIlgiliUrunler",
                column: "SahaDenetimAdimId");

            migrationBuilder.CreateIndex(
                name: "IX_SahaDenetimAdimlar_SahaDenetimId",
                table: "SahaDenetimAdimlar",
                column: "SahaDenetimId");

            migrationBuilder.CreateIndex(
                name: "IX_SahaDenetimAdimlar_SahaDenetimKategoriId",
                table: "SahaDenetimAdimlar",
                column: "SahaDenetimKategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_SahaDenetimler_OlusturanKullaniciId",
                table: "SahaDenetimler",
                column: "OlusturanKullaniciId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SahaDenetimAdimIlgiliKisiler");

            migrationBuilder.DropTable(
                name: "SahaDenetimAdimIlgiliUrunler");

            migrationBuilder.DropTable(
                name: "SahaDenetimAdimlar");

            migrationBuilder.DropTable(
                name: "SahaDenetimler");
        }
    }
}
