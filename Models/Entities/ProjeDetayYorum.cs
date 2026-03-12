namespace ProjeGorevYonetimi.Models.Entities;

public class ProjeDetayYorum : BaseEntity
{
    public int Id { get; set; }
    public int ProjeDetayId { get; set; }
    public string YorumMetni { get; set; } = string.Empty;

    public virtual ProjeDetay ProjeDetay { get; set; } = null!;
    public virtual Kullanici? InsertedByUser { get; set; }
    public virtual ICollection<ProjeDetayYorumDosya>? Dosyalar { get; set; }
}
