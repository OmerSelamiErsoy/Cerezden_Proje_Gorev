using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjeGorevYonetimi.Migrations
{
    /// <inheritdoc />
    public partial class AddGorevGrubu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GorevGrubuId",
                table: "Gorevler",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GorevGruplar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_GorevGruplar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GorevGruplar_Kullanicilar_OlusturanKullaniciId",
                        column: x => x.OlusturanKullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gorevler_GorevGrubuId",
                table: "Gorevler",
                column: "GorevGrubuId");

            migrationBuilder.CreateIndex(
                name: "IX_GorevGruplar_OlusturanKullaniciId",
                table: "GorevGruplar",
                column: "OlusturanKullaniciId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevler_GorevGruplar_GorevGrubuId",
                table: "Gorevler",
                column: "GorevGrubuId",
                principalTable: "GorevGruplar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gorevler_GorevGruplar_GorevGrubuId",
                table: "Gorevler");

            migrationBuilder.DropTable(
                name: "GorevGruplar");

            migrationBuilder.DropIndex(
                name: "IX_Gorevler_GorevGrubuId",
                table: "Gorevler");

            migrationBuilder.DropColumn(
                name: "GorevGrubuId",
                table: "Gorevler");
        }
    }
}
