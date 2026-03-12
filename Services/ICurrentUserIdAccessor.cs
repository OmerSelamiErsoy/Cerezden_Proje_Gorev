namespace ProjeGorevYonetimi.Services;

/// <summary>
/// Request bazlı mevcut kullanıcı Id'sini tutar. Middleware tarafından set edilir, DbContext audit için kullanır.
/// </summary>
public interface ICurrentUserIdAccessor
{
    int CurrentUserId { get; set; }
}
