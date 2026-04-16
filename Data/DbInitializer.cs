using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Models.Entities;

namespace ProjeGorevYonetimi.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (await db.Kullanicilar.IgnoreQueryFilters().AnyAsync())
            return;

        var kullanici = new Kullanici
        {
            CerezdenKullaniciId = 1,
            AdSoyad = "Ömer Ersoy",
            CepTel = "5556255246",
            SirketKodu = "001",
            AktifMi = true,
            InsertDate = DateTime.UtcNow,
            IsDeleted = false
        };
        db.Kullanicilar.Add(kullanici);

        var durumlar = new[]
        {
            new Durum { Ad = "İşlem Bekliyor", Sira = 1, InsertDate = DateTime.UtcNow, IsDeleted = false },
            new Durum { Ad = "Beklemede", Sira = 2, InsertDate = DateTime.UtcNow, IsDeleted = false },
            new Durum { Ad = "İşleme Alındı", Sira = 3, InsertDate = DateTime.UtcNow, IsDeleted = false },
            new Durum { Ad = "İptal Edildi", Sira = 4, InsertDate = DateTime.UtcNow, IsDeleted = false },
            new Durum { Ad = "Tamamlandı", Sira = 5, InsertDate = DateTime.UtcNow, IsDeleted = false }
        };
        db.Durumlar.AddRange(durumlar);

        await db.SaveChangesAsync();
    }

    public static async Task EnsureAdetBirimleriAsync(ApplicationDbContext db)
    {
        if (await db.AdetBirimleri.IgnoreQueryFilters().AnyAsync())
            return;

        var now = DateTime.UtcNow;
        var list = new[]
        {
            new AdetBirimi { Ad = "Adet", Sira = 1, InsertDate = now, IsDeleted = false },
            new AdetBirimi { Ad = "Metre", Sira = 2, InsertDate = now, IsDeleted = false },
            new AdetBirimi { Ad = "Kilogram", Sira = 3, InsertDate = now, IsDeleted = false },
            new AdetBirimi { Ad = "Santim", Sira = 4, InsertDate = now, IsDeleted = false },
            new AdetBirimi { Ad = "Milimetre", Sira = 5, InsertDate = now, IsDeleted = false },
            new AdetBirimi { Ad = "Kilometre", Sira = 6, InsertDate = now, IsDeleted = false },
            new AdetBirimi { Ad = "Litre", Sira = 7, InsertDate = now, IsDeleted = false }
        };

        db.AdetBirimleri.AddRange(list);
        await db.SaveChangesAsync();
    }
}
