namespace ProjeGorevYonetimi.Models.Entities;

public class GorevYorum : BaseEntity
{
    public int Id { get; set; }
    public int GorevId { get; set; }
    public string YorumMetni { get; set; } = string.Empty;

    public virtual Gorev Gorev { get; set; } = null!;
    public virtual ICollection<GorevYorumDosya>? Dosyalar { get; set; }
}
