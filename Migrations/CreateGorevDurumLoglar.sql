-- GorevDurumLoglar tablosu yoksa oluşturur. Log kayıtlarının görünmesi için bu script'i veritabanınızda çalıştırın.
-- SSMS veya Azure Data Studio'da appsettings.json'daki ConnectionString ile bağlanıp bu dosyayı çalıştırın.

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

    PRINT 'GorevDurumLoglar tablosu oluşturuldu.';
END
ELSE
    PRINT 'GorevDurumLoglar tablosu zaten mevcut.';
