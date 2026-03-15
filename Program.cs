using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Data;
using ProjeGorevYonetimi.Filters;
using ProjeGorevYonetimi.Middleware;
using ProjeGorevYonetimi.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<ApiKeyAuthFilter>();
builder.Services.AddScoped<RequireSessionFilter>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<RequireSessionFilter>();
})
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddScoped<ICurrentUserIdAccessor, CurrentUserIdAccessor>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IYetkiService, YetkiService>();
builder.Services.AddScoped<IProjeService, ProjeService>();
var isDev = builder.Configuration.GetValue<bool>("isDev");
var connectionString = isDev
    ? builder.Configuration.GetConnectionString("DefaultConnection_Dev")
    : builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Sunucuda LocalDB kullanılmaya çalışılıyorsa net hata ver (LocalDB sadece geliştirme ortamında vardır)
var isProduction = builder.Environment.IsProduction() || !builder.Environment.IsDevelopment();
if (isProduction && !string.IsNullOrEmpty(connectionString) &&
    connectionString.Contains("localdb", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "Sunucuda (Production) LocalDB kullanılamaz. appsettings.json içinde \"isDev\": false olmalı ve " +
        "DefaultConnection gerçek bir SQL Server adresi göstermeli (örn. Server=.; veya Server=192.168.1.2;).");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.UseMiddleware<CurrentUserMiddleware>();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

// Uygulama başlarken bekleyen migration'ları uygula, CrossAppLoginTokens tablosu yoksa oluştur, gerekirse seed çalıştır
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    // Eski boş migration nedeniyle tablo oluşmamış olabilir; yoksa oluştur
    await db.Database.ExecuteSqlRawAsync(@"
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CrossAppLoginTokens')
        BEGIN
            CREATE TABLE [dbo].[CrossAppLoginTokens] (
                [Id] INT IDENTITY(1,1) NOT NULL,
                [Token] NVARCHAR(MAX) NOT NULL,
                [Expires] DATETIME2(7) NOT NULL,
                CONSTRAINT [PK_CrossAppLoginTokens] PRIMARY KEY ([Id])
            );
        END");
    // GorevDurumLoglar tablosu yoksa oluştur (görev durum değişiklik logu)
    await db.Database.ExecuteSqlRawAsync(@"
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GorevDurumLoglar')
        BEGIN
            CREATE TABLE [dbo].[GorevDurumLoglar] (
                [Id] int NOT NULL IDENTITY(1,1),
                [GorevId] int NOT NULL,
                [EskiDurumId] int NULL,
                [YeniDurumId] int NOT NULL,
                [DegistirenKullaniciId] int NOT NULL,
                [DegistirmeTarihi] datetime2 NOT NULL,
                CONSTRAINT [PK_GorevDurumLoglar] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_GorevDurumLoglar_Gorevler_GorevId] FOREIGN KEY ([GorevId]) REFERENCES [Gorevler] ([Id]) ON DELETE NO ACTION,
                CONSTRAINT [FK_GorevDurumLoglar_Durumlar_EskiDurumId] FOREIGN KEY ([EskiDurumId]) REFERENCES [Durumlar] ([Id]) ON DELETE NO ACTION,
                CONSTRAINT [FK_GorevDurumLoglar_Durumlar_YeniDurumId] FOREIGN KEY ([YeniDurumId]) REFERENCES [Durumlar] ([Id]) ON DELETE NO ACTION,
                CONSTRAINT [FK_GorevDurumLoglar_Kullanicilar_DegistirenKullaniciId] FOREIGN KEY ([DegistirenKullaniciId]) REFERENCES [Kullanicilar] ([Id]) ON DELETE NO ACTION
            );
            CREATE INDEX [IX_GorevDurumLoglar_GorevId] ON [GorevDurumLoglar] ([GorevId]);
            CREATE INDEX [IX_GorevDurumLoglar_EskiDurumId] ON [GorevDurumLoglar] ([EskiDurumId]);
            CREATE INDEX [IX_GorevDurumLoglar_YeniDurumId] ON [GorevDurumLoglar] ([YeniDurumId]);
            CREATE INDEX [IX_GorevDurumLoglar_DegistirenKullaniciId] ON [GorevDurumLoglar] ([DegistirenKullaniciId]);
        END");
    // Gorevler.SorumluKullaniciId kolonu yoksa ekle
    await db.Database.ExecuteSqlRawAsync(@"
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Gorevler') AND name = 'SorumluKullaniciId')
        BEGIN
            ALTER TABLE [Gorevler] ADD [SorumluKullaniciId] int NULL;
            CREATE INDEX [IX_Gorevler_SorumluKullaniciId] ON [Gorevler] ([SorumluKullaniciId]);
            ALTER TABLE [Gorevler] ADD CONSTRAINT [FK_Gorevler_Kullanicilar_SorumluKullaniciId] FOREIGN KEY ([SorumluKullaniciId]) REFERENCES [Kullanicilar] ([Id]) ON DELETE NO ACTION;
        END");
    await DbInitializer.SeedAsync(db);
}

app.Run();
