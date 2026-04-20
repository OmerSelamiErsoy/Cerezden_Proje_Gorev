namespace ProjeGorevYonetimi.Models.Entities;

public class NotDosya : BaseEntity
{
    public int Id { get; set; }
    public int NotId { get; set; }
    public string DosyaAdi { get; set; } = string.Empty;
    public string DosyaYolu { get; set; } = string.Empty;

    public virtual Not Not { get; set; } = null!;
}

