using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Data;

namespace ProjeGorevYonetimi.Middleware;

public class CurrentUserMiddleware
{
    public const string SessionCerezdenKullaniciIdKey = "CerezdenKullaniciId";
    // Aynı CerezdenKullaniciId için DB'de birden fazla kullanıcı kaydı olasılığında
    // hangi Kullanici.Id'nin kullanıldığını session'da sabitler.
    public const string SessionKullaniciIdKey = "KullaniciId";

    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;

    public CurrentUserMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context, Services.ICurrentUserIdAccessor accessor, ApplicationDbContext db)
    {
        var cookieKey = _config.GetValue<string>("UserCookieKey") ?? "UCKAXDFT";
        int? cerezdenId = null;

        // Session'da direkt Kullanici.Id varsa audit için accessor'ü hızlıca doldur.
        var sessionUserId = context.Session.GetInt32(SessionKullaniciIdKey);
        if (sessionUserId.HasValue)
            accessor.CurrentUserId = sessionUserId.Value;

        // 1) Session'dan oku (öncelik)
        var sessionId = context.Session.GetInt32(SessionCerezdenKullaniciIdKey);
        if (sessionId.HasValue)
            cerezdenId = sessionId.Value;
        else
        {
            // 2) Session yoksa cookie'den oku; varsa session'ı 2 saat yenile
            var cookie = context.Request.Cookies[cookieKey];
            if (!string.IsNullOrEmpty(cookie) && int.TryParse(cookie, out var cid))
            {
                cerezdenId = cid;
                context.Session.SetInt32(SessionCerezdenKullaniciIdKey, cid);
            }
        }

        if (cerezdenId.HasValue)
        {
            var user = await db.Kullanicilar.IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.CerezdenKullaniciId == cerezdenId);
            if (user != null)
            {
                accessor.CurrentUserId = user.Id;
                context.Session.SetInt32(SessionKullaniciIdKey, user.Id);
            }
        }

        // 3) Session ve cookie yoksa, API dışı istekleri ana projeye yönlendir
        if (!cerezdenId.HasValue && !context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
            && !context.Request.Path.StartsWithSegments("/Home/Giriş", StringComparison.OrdinalIgnoreCase)
            && !context.Request.Path.StartsWithSegments("/Home/Giris", StringComparison.OrdinalIgnoreCase))
        {
            var redirectMain = _config.GetValue<string>("redirectMainProjectUrl");
            if (!string.IsNullOrEmpty(redirectMain))
            {
                context.Response.Redirect(redirectMain);
                return;
            }
        }

        await _next(context);
    }
}
