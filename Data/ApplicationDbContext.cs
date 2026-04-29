using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Models.Entities;

namespace ProjeGorevYonetimi.Data;

public class ApplicationDbContext : DbContext
{
    private readonly Services.ICurrentUserIdAccessor? _userIdAccessor;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, Services.ICurrentUserIdAccessor? userIdAccessor = null)
        : base(options)
    {
        _userIdAccessor = userIdAccessor;
    }

    public DbSet<Kullanici> Kullanicilar => Set<Kullanici>();
    public DbSet<Durum> Durumlar => Set<Durum>();
    public DbSet<ProjeDetayKategori> ProjeDetayKategoriler => Set<ProjeDetayKategori>();
    public DbSet<AdetBirimi> AdetBirimleri => Set<AdetBirimi>();
    public DbSet<Proje> Projeler => Set<Proje>();
    public DbSet<ProjeYetkiKullanici> ProjeYetkiKullanicilar => Set<ProjeYetkiKullanici>();
    public DbSet<ProjeDetay> ProjeDetaylar => Set<ProjeDetay>();
    public DbSet<ProjeDetayYorum> ProjeDetayYorumlar => Set<ProjeDetayYorum>();
    public DbSet<ProjeDetayYorumDosya> ProjeDetayYorumDosyalar => Set<ProjeDetayYorumDosya>();
    public DbSet<ProjeSablon> ProjeSablonlar => Set<ProjeSablon>();
    public DbSet<ProjeSablonDetay> ProjeSablonDetaylar => Set<ProjeSablonDetay>();
    public DbSet<GorevGrubu> GorevGruplar => Set<GorevGrubu>();
    public DbSet<Gorev> Gorevler => Set<Gorev>();
    public DbSet<GorevAtama> GorevAtamalar => Set<GorevAtama>();
    public DbSet<GorevYorum> GorevYorumlar => Set<GorevYorum>();
    public DbSet<GorevYorumDosya> GorevYorumDosyalar => Set<GorevYorumDosya>();
    public DbSet<GorevDurumLog> GorevDurumLoglar => Set<GorevDurumLog>();
    public DbSet<ProjeDetayDurumLog> ProjeDetayDurumLoglar => Set<ProjeDetayDurumLog>();
    public DbSet<CrossAppLoginToken> CrossAppLoginTokens => Set<CrossAppLoginToken>();
    public DbSet<NotGrup> NotGruplar => Set<NotGrup>();
    public DbSet<NotGrupYetkiKullanici> NotGrupYetkiKullanicilar => Set<NotGrupYetkiKullanici>();
    public DbSet<Not> Notlar => Set<Not>();
    public DbSet<NotDosya> NotDosyalar => Set<NotDosya>();
    public DbSet<NotYetkiKullanici> NotYetkiKullanicilar => Set<NotYetkiKullanici>();
    public DbSet<SahaDenetimKategori> SahaDenetimKategoriler => Set<SahaDenetimKategori>();
    public DbSet<SahaDenetim> SahaDenetimler => Set<SahaDenetim>();
    public DbSet<SahaDenetimAdim> SahaDenetimAdimlar => Set<SahaDenetimAdim>();
    public DbSet<SahaDenetimAdimIlgiliKisi> SahaDenetimAdimIlgiliKisiler => Set<SahaDenetimAdimIlgiliKisi>();
    public DbSet<SahaDenetimAdimIlgiliUrun> SahaDenetimAdimIlgiliUrunler => Set<SahaDenetimAdimIlgiliUrun>();
    public DbSet<SahaDenetimAdimFoto> SahaDenetimAdimFotolar => Set<SahaDenetimAdimFoto>();
    public DbSet<SahaDenetimYetkiKullanici> SahaDenetimYetkiKullanicilar => Set<SahaDenetimYetkiKullanici>();

    private int DefaultUserId => _userIdAccessor?.CurrentUserId ?? 1;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Kullanici>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Durum>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProjeDetayKategori>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<AdetBirimi>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Proje>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProjeYetkiKullanici>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProjeDetay>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProjeDetayYorum>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProjeDetayYorumDosya>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProjeSablon>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProjeSablonDetay>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<GorevGrubu>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Gorev>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<GorevAtama>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<GorevYorum>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<GorevYorumDosya>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<NotGrup>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<NotGrupYetkiKullanici>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Not>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<NotDosya>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<NotYetkiKullanici>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SahaDenetimKategori>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SahaDenetim>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SahaDenetimAdim>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SahaDenetimAdimIlgiliKisi>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SahaDenetimAdimIlgiliUrun>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SahaDenetimAdimFoto>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SahaDenetimYetkiKullanici>().HasQueryFilter(e => !e.IsDeleted);

        modelBuilder.Entity<ProjeDetayYorum>()
            .HasOne(y => y.InsertedByUser)
            .WithMany()
            .HasForeignKey(nameof(BaseEntity.InsertedByUserId))
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Kullanici>()
            .HasIndex(k => k.CerezdenKullaniciId);

        modelBuilder.Entity<Proje>()
            .HasOne(p => p.SorumluKullanici)
            .WithMany(k => k!.ProjelerSorumlu)
            .HasForeignKey(p => p.SorumluKullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Proje>()
            .HasOne(p => p.OlusturanKullanici)
            .WithMany()
            .HasForeignKey(p => p.OlusturanKullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProjeDetay>()
            .HasOne(d => d.SorumluKullanici)
            .WithMany(k => k!.ProjeDetaylarSorumlu)
            .HasForeignKey(d => d.SorumluKullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GorevGrubu>()
            .HasOne(g => g.OlusturanKullanici)
            .WithMany()
            .HasForeignKey(g => g.OlusturanKullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Gorev>()
            .HasOne(g => g.GorevGrubu)
            .WithMany(gr => gr!.Gorevler)
            .HasForeignKey(g => g.GorevGrubuId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Gorev>()
            .HasOne(g => g.OlusturanKullanici)
            .WithMany(k => k!.OlusturulanGorevler)
            .HasForeignKey(g => g.OlusturanKullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Gorev>()
            .HasOne(g => g.SorumluKullanici)
            .WithMany()
            .HasForeignKey(g => g.SorumluKullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GorevAtama>()
            .HasOne(a => a.Kullanici)
            .WithMany(k => k!.GorevAtamalari)
            .HasForeignKey(a => a.KullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Proje>()
            .HasMany(p => p.Detaylar)
            .WithOne(d => d.Proje)
            .HasForeignKey(d => d.ProjeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Proje>()
            .HasMany(p => p.YetkiKullanicilar)
            .WithOne(y => y.Proje)
            .HasForeignKey(y => y.ProjeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NotGrup>()
            .HasOne(g => g.OlusturanKullanici)
            .WithMany()
            .HasForeignKey(g => g.OlusturanKullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NotGrup>()
            .HasMany(g => g.YetkiKullanicilar)
            .WithOne(y => y.NotGrup)
            .HasForeignKey(y => y.NotGrupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NotGrupYetkiKullanici>()
            .HasOne(y => y.Kullanici)
            .WithMany()
            .HasForeignKey(y => y.KullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Not>()
            .HasOne(n => n.NotGrup)
            .WithMany(g => g.Notlar)
            .HasForeignKey(n => n.NotGrupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Not>()
            .HasOne(n => n.OlusturanKullanici)
            .WithMany()
            .HasForeignKey(n => n.OlusturanKullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Not>()
            .HasMany(n => n.Dosyalar)
            .WithOne(d => d.Not)
            .HasForeignKey(d => d.NotId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Not>()
            .HasMany(n => n.YetkiKullanicilar)
            .WithOne(y => y.Not)
            .HasForeignKey(y => y.NotId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NotYetkiKullanici>()
            .HasOne(y => y.Kullanici)
            .WithMany()
            .HasForeignKey(y => y.KullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SahaDenetim>()
            .HasOne(x => x.OlusturanKullanici)
            .WithMany()
            .HasForeignKey(x => x.OlusturanKullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SahaDenetim>()
            .HasOne(x => x.KapatanKullanici)
            .WithMany()
            .HasForeignKey(x => x.KapatanKullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SahaDenetim>()
            .HasOne(x => x.GeriAcanKullanici)
            .WithMany()
            .HasForeignKey(x => x.GeriAcanKullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SahaDenetim>()
            .HasMany(x => x.Adimlar)
            .WithOne(a => a.SahaDenetim)
            .HasForeignKey(a => a.SahaDenetimId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SahaDenetimAdim>()
            .HasOne(a => a.Kategori)
            .WithMany()
            .HasForeignKey(a => a.SahaDenetimKategoriId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SahaDenetimAdim>()
            .HasMany(a => a.IlgiliKisiler)
            .WithOne(k => k.SahaDenetimAdim)
            .HasForeignKey(k => k.SahaDenetimAdimId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SahaDenetimAdim>()
            .HasMany(a => a.IlgiliUrunler)
            .WithOne(u => u.SahaDenetimAdim)
            .HasForeignKey(u => u.SahaDenetimAdimId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SahaDenetimAdim>()
            .HasMany(a => a.Fotograflar)
            .WithOne(f => f.SahaDenetimAdim)
            .HasForeignKey(f => f.SahaDenetimAdimId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SahaDenetim>()
            .HasMany(x => x.YetkiKullanicilar)
            .WithOne(y => y.SahaDenetim)
            .HasForeignKey(y => y.SahaDenetimId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SahaDenetimYetkiKullanici>()
            .HasOne(y => y.Kullanici)
            .WithMany()
            .HasForeignKey(y => y.KullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProjeDetay>()
            .HasMany(d => d.Yorumlar)
            .WithOne(y => y.ProjeDetay)
            .HasForeignKey(y => y.ProjeDetayId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Gorev>()
            .HasMany(g => g.Atamalar)
            .WithOne(a => a.Gorev)
            .HasForeignKey(a => a.GorevId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Gorev>()
            .HasMany(g => g.Yorumlar)
            .WithOne(y => y.Gorev)
            .HasForeignKey(y => y.GorevId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GorevDurumLog>()
            .HasOne(l => l.Gorev)
            .WithMany()
            .HasForeignKey(l => l.GorevId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<GorevDurumLog>()
            .HasOne(l => l.DegistirenKullanici)
            .WithMany()
            .HasForeignKey(l => l.DegistirenKullaniciId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<GorevDurumLog>()
            .HasOne(l => l.EskiDurum)
            .WithMany()
            .HasForeignKey(l => l.EskiDurumId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<GorevDurumLog>()
            .HasOne(l => l.YeniDurum)
            .WithMany()
            .HasForeignKey(l => l.YeniDurumId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProjeDetay>()
            .Property(p => p.Adet)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ProjeDetay>()
            .Property(p => p.MaliyetFiyati)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ProjeDetay>()
            .Property(p => p.CikisFiyati)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ProjeSablonDetay>()
            .Property(p => p.Adet)
            .HasPrecision(18, 2);

        modelBuilder.Entity<SahaDenetimAdimIlgiliUrun>()
            .Property(p => p.Adet)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ProjeDetayDurumLog>()
            .HasOne(l => l.ProjeDetay)
            .WithMany()
            .HasForeignKey(l => l.ProjeDetayId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ProjeDetayDurumLog>()
            .HasOne(l => l.DegistirenKullanici)
            .WithMany()
            .HasForeignKey(l => l.DegistirenKullaniciId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ProjeDetayDurumLog>()
            .HasOne(l => l.EskiDurum)
            .WithMany()
            .HasForeignKey(l => l.EskiDurumId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ProjeDetayDurumLog>()
            .HasOne(l => l.YeniDurum)
            .WithMany()
            .HasForeignKey(l => l.YeniDurumId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public override int SaveChanges()
    {
        ApplyAudit();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAudit();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAudit()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.InsertDate = now;
                entry.Entity.InsertedByUserId ??= DefaultUserId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdateDate = now;
                entry.Entity.UpdatedByUserId = DefaultUserId;
                if (entry.Entity.IsDeleted)
                {
                    entry.Entity.DeleteDate = now;
                    entry.Entity.DeletedByUserId = DefaultUserId;
                }
            }
        }
    }
}
