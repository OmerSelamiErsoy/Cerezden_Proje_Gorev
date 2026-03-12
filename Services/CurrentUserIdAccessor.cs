namespace ProjeGorevYonetimi.Services;

public class CurrentUserIdAccessor : ICurrentUserIdAccessor
{
    public int CurrentUserId { get; set; } = 1;
}
