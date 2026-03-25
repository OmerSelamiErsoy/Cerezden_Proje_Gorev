using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Data;

namespace ProjeGorevYonetimi.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _db;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, Data.ApplicationDbContext db)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _db = db;
    }

    public int GetCurrentUserId()
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx != null)
        {
            // Session'da Kullanici.Id sabitse doğrudan onu kullan.
            var sessionUserId = ctx.Session.GetInt32(Middleware.CurrentUserMiddleware.SessionKullaniciIdKey);
            if (sessionUserId.HasValue) return sessionUserId.Value;
        }

        var cerezdenId = GetCurrentCerezdenKullaniciId();
        if (cerezdenId == null) return 1;

        // Aynı CerezdenKullaniciId için birden fazla kayıt varsa deterministik olsun.
        var user = _db.Kullanicilar
            .AsNoTracking()
            .IgnoreQueryFilters()
            .OrderByDescending(k => k.Id)
            .FirstOrDefault(k => k.CerezdenKullaniciId == cerezdenId);

        return user?.Id ?? 1;
    }

    public int? GetCurrentCerezdenKullaniciId()
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx == null) return null;

        var sessionId = ctx.Session.GetInt32(Middleware.CurrentUserMiddleware.SessionCerezdenKullaniciIdKey);
        if (sessionId.HasValue) return sessionId.Value;

        var cookieKey = _configuration.GetValue<string>("UserCookieKey") ?? "UCKAXDFT";
        var cookie = ctx.Request.Cookies[cookieKey];
        if (string.IsNullOrEmpty(cookie) || !int.TryParse(cookie, out var id)) return null;
        return id;
    }
}
