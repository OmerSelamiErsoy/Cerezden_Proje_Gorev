using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Data;
using ProjeGorevYonetimi.Models.Entities;

namespace ProjeGorevYonetimi.Services;

public class YetkiService : IYetkiService
{
    private readonly ICurrentUserService _currentUser;
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _db;

    public YetkiService(ICurrentUserService currentUser, IConfiguration config, ApplicationDbContext db)
    {
        _currentUser = currentUser;
        _config = config;
        _db = db;
    }

    public bool GenelYetkiliMi()
    {
        var cerezdenId = _currentUser.GetCurrentCerezdenKullaniciId();
        if (cerezdenId == null) return false;
        var ids = _config["GeneralAuthority"]?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s.Trim(), out var n) ? n : (int?)null)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToHashSet() ?? new HashSet<int>();
        return ids.Contains(cerezdenId.Value);
    }

    public async Task<bool> ProjeGorebilirMiAsync(int projeId, CancellationToken ct = default)
    {
        if (GenelYetkiliMi()) return true;
        var proje = await _db.Projeler
            .AsNoTracking()
            .Include(p => p.YetkiKullanicilar)
            .FirstOrDefaultAsync(p => p.Id == projeId, ct);
        if (proje == null) return false;
        if (proje.YetkiTipi == 0) return true; // Genel
        var userId = _currentUser.GetCurrentUserId();
        return proje.YetkiKullanicilar!.Any(y => y.KullaniciId == userId);
    }
}
