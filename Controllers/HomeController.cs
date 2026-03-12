using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Data;
using ProjeGorevYonetimi.Middleware;
using ProjeGorevYonetimi.Models;

namespace ProjeGorevYonetimi.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, IConfiguration config)
    {
        _logger = logger;
        _db = db;
        _config = config;
    }

    /// <summary>
    /// Token ile doğrulama: token geçerliyse kullanıcıyı session (2h) ve cookie (10h) ile işaretleyip redirectUrl'e yönlendirir.
    /// </summary>
    [HttpGet]
    [Route("Home/Giriş")]
    [Route("Home/Giris")]
    public async Task<IActionResult> Giris(int kullaniciId, string token, string? redirectUrl, CancellationToken ct)
    {
        var redirectMain = _config.GetValue<string>("redirectMainProjectUrl") ?? "/";

        var tokenRecord = await _db.CrossAppLoginTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Token == token && t.Expires > DateTime.UtcNow, ct);
        if (tokenRecord == null)
        {
            _logger.LogWarning("Giris: Geçersiz veya süresi dolmuş token. Token değeri CrossAppLoginTokens tablosunda olmalı ve Expires gelecek bir tarih olmalı.");
            return Redirect(redirectMain);
        }

        var user = await _db.Kullanicilar
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.CerezdenKullaniciId == kullaniciId, ct);
        if (user == null)
        {
            _logger.LogWarning("Giris: Kullanici bulunamadı. CerezdenKullaniciId={KullaniciId}. Kullanicilar tablosunda bu CerezdenKullaniciId ile kayıt olmalı.", kullaniciId);
            return Redirect(redirectMain);
        }

        var cookieKey = _config.GetValue<string>("UserCookieKey") ?? "UCKAXDFT";
        HttpContext.Session.SetInt32(CurrentUserMiddleware.SessionCerezdenKullaniciIdKey, user.CerezdenKullaniciId);
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(10)
        };
        Response.Cookies.Append(cookieKey, user.CerezdenKullaniciId.ToString(), cookieOptions);

        // redirectUrl: tırnakları temizle; relative path ise başına / ekle; tam URL ile yönlendir (Home/Home/Index hatasını önler)
        var targetUrl = redirectMain;
        if (!string.IsNullOrWhiteSpace(redirectUrl))
        {
            var cleaned = redirectUrl.Trim().Trim('\'', '"');
            if (!string.IsNullOrWhiteSpace(cleaned))
            {
                var path = cleaned.StartsWith("/", StringComparison.Ordinal) ? cleaned : "/" + cleaned;
                // Relative path ise tam URL yap (tarayıcı mevcut path'e göre çözümlemesin)
                if (!cleaned.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !cleaned.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    targetUrl = baseUrl + path;
                }
                else
                {
                    targetUrl = cleaned;
                }
            }
        }
        return Redirect(targetUrl);
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
