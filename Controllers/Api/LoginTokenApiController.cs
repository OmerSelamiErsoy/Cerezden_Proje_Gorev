using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Data;
using ProjeGorevYonetimi.Models.Entities;

namespace ProjeGorevYonetimi.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[ServiceFilter(typeof(Filters.ApiKeyAuthFilter))]
public class LoginTokenApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public LoginTokenApiController(ApplicationDbContext db) => _db = db;

    /// <summary>
    /// CrossAppLoginTokens tablosuna yeni token kaydı ekler.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CrossAppLoginTokenDto>> Create([FromBody] LoginTokenCreateRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request?.Token))
            return BadRequest("Token zorunludur.");

        var entity = new CrossAppLoginToken
        {
            Token = request.Token.Trim(),
            Expires = request.Expires
        };
        _db.CrossAppLoginTokens.Add(entity);
        await _db.SaveChangesAsync(ct);

        return Ok(new CrossAppLoginTokenDto { Id = entity.Id, Token = entity.Token, Expires = entity.Expires });
    }
}

public class LoginTokenCreateRequest
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
}

public class CrossAppLoginTokenDto
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
}
