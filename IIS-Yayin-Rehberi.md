# ProjeGorevYonetimi - IIS ile Windows Server’da Yayına Alma

## 1. Sunucuda Gereksinimler

### 1.1 .NET 8 Runtime (Hosting Bundle)
- **İndir:** https://dotnet.microsoft.com/download/dotnet/8.0  
  “Hosting Bundle” (ASP.NET Core Runtime + IIS desteği) sürümünü indirin.
- Kurulumu yapın. Bu, IIS için **AspNetCoreModuleV2** modülünü de ekler.

### 1.2 IIS
- **Sunucu Yöneticisi** → Rol ve Özellik Ekle → **Web Sunucusu (IIS)** ve **Uygulama Geliştirme** altında **ASP.NET 4.8** (veya sunucudaki sürüm) kurulu olsun.
- Hosting Bundle kurulduktan sonra IIS’te ASP.NET Core uygulaması çalıştırılabilir.

---

## 2. Projeyi Yayına Hazırlamak (Publish)

Bilgisayarınızda (geliştirme ortamında) proje klasöründe:

```powershell
cd c:\repos\ProjeGorevYonetimi

# Release modda yayın klasörüne çıktı al
dotnet publish -c Release -o .\publish
```

Çıktı `.\publish` klasöründe oluşur. Bu klasörün tamamını sunucuya kopyalayacaksınız.

---

## 3. Sunucuda Klasör ve İzinler

1. Örneğin uygulama için bir klasör açın:  
   `C:\inetpub\ProjeGorevYonetimi`
2. Publish çıktısındaki **tüm dosya ve klasörleri** bu dizine kopyalayın.
3. Klasöre IIS’in erişebilmesi için izin verin:
   - `C:\inetpub\ProjeGorevYonetimi` → Sağ tık → **Özellikler** → **Güvenlik**
   - **Düzenle** → **Ekle** → `IIS AppPool\ProjeGorevYonetimi` yazın (Application Pool adını bir sonraki adımda bu isimle oluşturacağınızı varsayıyoruz) → **Tamam**
   - Bu kullanıcıya **Okuma ve Yürütme** izni verin → **Tamam**

---

## 4. IIS’te Site ve Application Pool

### 4.1 Application Pool
1. **IIS Yöneticisi** → **Application Pools** → **Add Application Pool**
2. **İsim:** `ProjeGorevYonetimi`
3. **.NET CLR version:** **No Managed Code** (ASP.NET Core zorunlu)
4. **Managed pipeline mode:** Integrated
5. **OK** → Yeni pool’a sağ tık → **Advanced Settings**
   - **Start Mode:** AlwaysRunning (isteğe bağlı)
   - **Idle Time-out:** 0 veya 20 (isteğe bağlı)

### 4.2 Site
1. **Sites** → **Add Website**
2. **Site name:** `ProjeGorevYonetimi` (veya istediğiniz ad)
3. **Application pool:** Az önce oluşturduğunuz `ProjeGorevYonetimi`
4. **Physical path:** `C:\inetpub\ProjeGorevYonetimi`
5. **Binding:**  
   - Type: `http` (veya https için bir sertifika ekleyin)  
   - Port: `80` (veya 8080 vb.)  
   - Host name: Örn. `personel.cerezdenyonetim.com`
6. **OK**

---

## 5. Yapılandırma (appsettings.json)

Sunucudaki `C:\inetpub\ProjeGorevYonetimi\appsettings.json` dosyasını sunucu ortamına göre düzenleyin:

- **ConnectionStrings:** Sunucudaki SQL Server bağlantı bilgileri
- **isDev:** `false`
- **redirectMainProjectUrl:** Ana proje URL’i (örn. `http://cerezdenyonetim.com`)
- **ApiAuthGuid** / **UserCookieKey:** Gerekirse değiştirin

Örnek (içerik sadece örnek, kendi değerlerinizi yazın):

```json
{
  "isDev": false,
  "ApiAuthGuid": "A1B2C3D4-E5F6-4789-A012-3456789ABCDE",
  "UserCookieKey": "UCKAXDFT",
  "redirectMainProjectUrl": "http://cerezdenyonetim.com",
  "ConnectionStrings": {
    "DefaultConnection": "Server=SUNUCU;Database=ProjeGorevDB;User Id=db_user;Password=***;TrustServerCertificate=True;"
  }
}
```

**Not:** Hassas bilgileri (şifre, API anahtarı) production’da `appsettings.Production.json` veya ortam değişkenleri ile yönetmek daha güvenlidir.

---

## 6. web.config (Publish ile Otomatik)

`dotnet publish` ile oluşan `web.config` genelde aşağıdakine benzer. Elle kopyalamak zorunda değilsiniz; publish çıktısında gelir.

Log açmak isterseniz sunucudaki `web.config` içinde şunu kullanabilirsiniz:

```xml
<aspNetCore processPath="dotnet" 
            arguments=".\ProjeGorevYonetimi.dll" 
            stdoutLogEnabled="true" 
            stdoutLogFile=".\logs\stdout" 
            hostingModel="inprocess" />
```

`logs` klasörünü site fiziksel yolunda oluşturup IIS App Pool kullanıcısına yazma izni verin.

---

## 7. Kontrol ve Sorun Giderme

1. Application Pool’u **Start** edin, siteyi **Start** edin.
2. Tarayıcıdan site adresini açın (örn. `http://personel.cerezdenyonetim.com`).
3. **503 / 500 hatası:**
   - Hosting Bundle’ın (.NET 8) kurulu olduğundan emin olun.
   - Application Pool’un **No Managed Code** olduğunu kontrol edin.
   - `logs` klasöründeki stdout log’lara bakın.
4. **Veritabanı hatası:** Connection string’i ve SQL Server erişimini (güvenlik duvarı, kullanıcı adı/şifre) kontrol edin. İlk çalıştırmada migration’lar otomatik uygulanır.

---

## Özet Komutlar (Geliştirme PC’de)

```powershell
cd c:\repos\ProjeGorevYonetimi
dotnet publish -c Release -o .\publish
# .\publish içeriğini sunucuya C:\inetpub\ProjeGorevYonetimi gibi bir klasöre kopyalayın
```

Sonrasında IIS’te Application Pool (**No Managed Code**) ve Site’i oluşturup fiziksel yolu bu klasöre verin; `appsettings.json`’ı sunucuya göre düzenleyin.
