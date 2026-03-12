namespace ProjeGorevYonetimi.Models.Entities;

public class GorevYorumDosya : BaseEntity
{
    public int Id { get; set; }
    public int GorevYorumId { get; set; }
    public string DosyaAdi { get; set; } = string.Empty;
    public string DosyaYolu { get; set; } = string.Empty;

    public virtual GorevYorum GorevYorum { get; set; } = null!;
}
