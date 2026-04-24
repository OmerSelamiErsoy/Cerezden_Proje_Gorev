using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjeGorevYonetimi.Migrations
{
    /// <inheritdoc />
    public partial class AddSahaDenetimKapaliLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GeriAcanKullaniciId",
                table: "SahaDenetimler",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GeriAcmaTarihi",
                table: "SahaDenetimler",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "KapaliMi",
                table: "SahaDenetimler",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "KapatanKullaniciId",
                table: "SahaDenetimler",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "KapatmaTarihi",
                table: "SahaDenetimler",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SahaDenetimler_GeriAcanKullaniciId",
                table: "SahaDenetimler",
                column: "GeriAcanKullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_SahaDenetimler_KapatanKullaniciId",
                table: "SahaDenetimler",
                column: "KapatanKullaniciId");

            migrationBuilder.AddForeignKey(
                name: "FK_SahaDenetimler_Kullanicilar_GeriAcanKullaniciId",
                table: "SahaDenetimler",
                column: "GeriAcanKullaniciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SahaDenetimler_Kullanicilar_KapatanKullaniciId",
                table: "SahaDenetimler",
                column: "KapatanKullaniciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SahaDenetimler_Kullanicilar_GeriAcanKullaniciId",
                table: "SahaDenetimler");

            migrationBuilder.DropForeignKey(
                name: "FK_SahaDenetimler_Kullanicilar_KapatanKullaniciId",
                table: "SahaDenetimler");

            migrationBuilder.DropIndex(
                name: "IX_SahaDenetimler_GeriAcanKullaniciId",
                table: "SahaDenetimler");

            migrationBuilder.DropIndex(
                name: "IX_SahaDenetimler_KapatanKullaniciId",
                table: "SahaDenetimler");

            migrationBuilder.DropColumn(
                name: "GeriAcanKullaniciId",
                table: "SahaDenetimler");

            migrationBuilder.DropColumn(
                name: "GeriAcmaTarihi",
                table: "SahaDenetimler");

            migrationBuilder.DropColumn(
                name: "KapaliMi",
                table: "SahaDenetimler");

            migrationBuilder.DropColumn(
                name: "KapatanKullaniciId",
                table: "SahaDenetimler");

            migrationBuilder.DropColumn(
                name: "KapatmaTarihi",
                table: "SahaDenetimler");
        }
    }
}
