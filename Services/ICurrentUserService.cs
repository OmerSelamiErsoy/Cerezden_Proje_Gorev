namespace ProjeGorevYonetimi.Services;

public interface ICurrentUserService
{
    int GetCurrentUserId();
    int? GetCurrentCerezdenKullaniciId();
}
