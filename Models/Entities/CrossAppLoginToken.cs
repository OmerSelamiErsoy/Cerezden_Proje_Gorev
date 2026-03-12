namespace ProjeGorevYonetimi.Models.Entities;

public class CrossAppLoginToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
}
