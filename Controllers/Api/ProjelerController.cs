using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Data;
using ProjeGorevYonetimi.Services;

namespace ProjeGorevYonetimi.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[ServiceFilter(typeof(Filters.ApiKeyAuthFilter))]
public class ProjelerController : ControllerBase
{
    private readonly IProjeService _projeService;
    private readonly ICurrentUserService _currentUser;
    private readonly IYetkiService _yetki;
    private readonly ApplicationDbContext _db;

    public ProjelerController(IProjeService projeService, ICurrentUserService currentUser, IYetkiService yetki, ApplicationDbContext db)
    {
        _projeService = projeService;
        _currentUser = currentUser;
        _yetki = yetki;
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProjeListDto>>> List(CancellationToken ct)
    {
        var list = await _projeService.GetVisibleProjelerAsync(ct);
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjeDetayDto>> Get(int id, CancellationToken ct)
    {
        var dto = await _projeService.GetProjeDetayAsync(id, ct);
        if (dto == null) return NotFound();
        return Ok(dto);
    }

    [HttpGet("{id}/progress")]
    public async Task<ActionResult<int>> Progress(int id, CancellationToken ct)
    {
        var yuzde = await _projeService.ProgressYuzdeAsync(id, ct);
        return Ok(yuzde);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] ProjeCreateDto dto, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var id = await _projeService.CreateAsync(dto, userId, ct);
        return Ok(new { id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProjeUpdateDto dto, CancellationToken ct)
    {
        await _projeService.UpdateAsync(id, dto, ct);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var proje = await _db.Projeler.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (proje == null) return NotFound();

        var userId = _currentUser.GetCurrentUserId();
        var genelYetkili = _yetki.GenelYetkiliMi();
        var silinebilir = genelYetkili || proje.OlusturanKullaniciId == userId;
        if (!silinebilir)
            return StatusCode(403, new { error = "Bu projeyi silme yetkiniz yok. Sadece genel yetkili kullanıcılar veya projeyi oluşturan kullanıcı silebilir." });

        await _projeService.SoftDeleteAsync(id, userId, ct);
        return Ok();
    }
}
