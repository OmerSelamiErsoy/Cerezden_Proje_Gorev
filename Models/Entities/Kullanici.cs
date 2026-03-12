namespace ProjeGorevYonetimi.Models.Entities;

public class Kullanici : BaseEntity
{
    public int Id { get; set; }
    public int CerezdenKullaniciId { get; set; }
    public string AdSoyad { get; set; } = string.Empty;
    public string? CepTel { get; set; }
    public string? SirketKodu { get; set; }
    public bool AktifMi { get; set; }

    public virtual ICollection<Proje>? ProjelerSorumlu { get; set; }
    public virtual ICollection<ProjeDetay>? ProjeDetaylarSorumlu { get; set; }
    public virtual ICollection<GorevAtama>? GorevAtamalari { get; set; }
    public virtual ICollection<Gorev>? OlusturulanGorevler { get; set; }
}
