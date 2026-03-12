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
        var cerezdenId = GetCurrentCerezdenKullaniciId();
        if (cerezdenId == null) return 1;
        var user = _db.Kullanicilar.IgnoreQueryFilters().FirstOrDefault(k => k.CerezdenKullaniciId == cerezdenId);
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
