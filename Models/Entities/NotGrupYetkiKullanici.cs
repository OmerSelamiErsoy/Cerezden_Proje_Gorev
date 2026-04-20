namespace ProjeGorevYonetimi.Models.Entities;

public class NotGrupYetkiKullanici : BaseEntity
{
    public int Id { get; set; }
    public int NotGrupId { get; set; }
    public int KullaniciId { get; set; }

    public virtual NotGrup NotGrup { get; set; } = null!;
    public virtual Kullanici Kullanici { get; set; } = null!;
}

