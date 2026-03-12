namespace ProjeGorevYonetimi.Models.Entities;

public class ProjeDetayYorumDosya : BaseEntity
{
    public int Id { get; set; }
    public int ProjeDetayYorumId { get; set; }
    public string DosyaAdi { get; set; } = string.Empty;
    public string DosyaYolu { get; set; } = string.Empty;

    public virtual ProjeDetayYorum ProjeDetayYorum { get; set; } = null!;
}
