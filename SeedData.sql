USE [LexiConnect]
GO

INSERT INTO [dbo].[University]
           ([Name], [ShortName], [CountryId], [City], [IsVerified], [LogoUrl], [CreatedAt])
VALUES
(N'Đại học Quốc gia Hà Nội', N'VNU', 1, N'Hà Nội', 1, N'/image/logo_VNU.png', SYSDATETIME()),

(N'Đại học Quốc gia Thành phố Hồ Chí Minh', N'VNUHCM', 1, N'TP. Hồ Chí Minh', 1, N'/image/logo_VNUHCM.png', SYSDATETIME()),

(N'Đại học FPT Đà Nẵng', N'FUDA', 1, N'TP. Đà Nẵng', 1, N'/image/logo_FPT.png', SYSDATETIME())
GO
