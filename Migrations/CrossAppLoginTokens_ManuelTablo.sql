-- CrossAppLoginTokens tablosu yoksa oluşturmak için bu script'i veritabanında çalıştırın.
-- (Migration daha önce boş Up ile uygulandıysa tablo oluşmamış olabilir.)

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CrossAppLoginTokens')
BEGIN
    CREATE TABLE [dbo].[CrossAppLoginTokens] (
        [Id]       INT            IDENTITY (1, 1) NOT NULL,
        [Token]    NVARCHAR(MAX)  NOT NULL,
        [Expires]  DATETIME2(7)   NOT NULL,
        CONSTRAINT [PK_CrossAppLoginTokens] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO
