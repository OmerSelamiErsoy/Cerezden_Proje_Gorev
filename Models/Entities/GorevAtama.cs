namespace ProjeGorevYonetimi.Models.Entities;

public class GorevAtama : BaseEntity
{
    public int Id { get; set; }
    public int GorevId { get; set; }
    public int KullaniciId { get; set; }

    public virtual Gorev Gorev { get; set; } = null!;
    public virtual Kullanici Kullanici { get; set; } = null!;
}
