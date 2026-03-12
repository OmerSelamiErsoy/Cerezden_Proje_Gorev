namespace ProjeGorevYonetimi.Services;

public interface IYetkiService
{
    /// <summary>Genel yetkili kullanıcı (GeneralAuthority) her şeyi görebilir.</summary>
    bool GenelYetkiliMi();
    /// <summary>Kullanıcının projeyi görme yetkisi var mı?</summary>
    Task<bool> ProjeGorebilirMiAsync(int projeId, CancellationToken ct = default);
}
