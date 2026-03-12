# Proje ve Görev Yönetimi

.NET 8 MVC, SQL Server, Entity Framework Core, Vue.js (CDN) ile proje ve görev yönetim uygulaması.

## Gereksinimler

- .NET 8 SDK
- SQL Server (LocalDB veya tam instance)

## Çalıştırma

1. Bağlantı dizisini düzenleyin: `appsettings.json` içinde `ConnectionStrings:DefaultConnection`
2. `dotnet run` veya Visual Studio ile çalıştırın
3. Tarayıcıda açılacak adres: https://localhost:5xxx veya http://localhost:5xxx
4. Varsayılan giriş: Dashboard açılır. Sol menüden Projeler, Görevler, Şablonlar, Parametreler erişilir.

## Özellikler

- **Projeler**: CRUD, adımlar (todo), ilerleme %, yetkilendirme (Genel / Kişi bazlı), yorum ve dosya
- **Şablonlar**: Proje şablonu oluşturma, "Proje Başlat" ile klonlama
- **Görevler**: Kanban board, kendime/kişilere atama, yorum, renk, son 50 tamamlanan/iptal
- **Parametreler**: Durum ve Kategori (PR) yönetimi
- **Genel yetki**: `appsettings.json` → `GeneralAuthority`: virgülle ayrılmış CerezdenKullaniciId listesi (bu kullanıcılar tüm projeleri görür)
- Tüm silmeler **soft delete**; tablolarda Insert/Update/Delete tarih ve kullanıcı alanları

## Varsayılan kullanıcı (seed)

- CerezdenKullaniciId: 1, AdSoyad: Ömer Ersoy, CepTel: 5556255246, ŞirketKodu: 001, Aktif: true
- Insert/Update/Delete işlemlerinde varsayılan olarak bu kullanıcının Id'si kullanılır (cookie yoksa).

## Veritabanı

İlk çalıştırmada migration uygulanır ve seed (durumlar + varsayılan kullanıcı) çalışır.
