IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [Country] (
    [Id] int NOT NULL IDENTITY,
    [CountryName] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Country] PRIMARY KEY ([Id])
);

CREATE TABLE [SubscriptionPlan] (
    [PlanId] int NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    [DisplayName] nvarchar(100) NOT NULL,
    [Price] decimal(10,2) NOT NULL,
    [Currency] nvarchar(3) NOT NULL,
    [DurationMonths] int NOT NULL,
    [MaxDownloadsPerMonth] int NULL,
    [PremiumContentAccess] bit NOT NULL,
    [PrioritySupport] bit NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_SubscriptionPlan] PRIMARY KEY ([PlanId])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [University] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [ShortName] nvarchar(max) NULL,
    [CountryId] int NOT NULL,
    [City] nvarchar(max) NOT NULL,
    [IsVerified] bit NOT NULL,
    [LogoUrl] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_University] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_University_Country_CountryId] FOREIGN KEY ([CountryId]) REFERENCES [Country] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Major] (
    [MajorId] int NOT NULL IDENTITY,
    [Name] nvarchar(150) NOT NULL,
    [Code] nvarchar(20) NULL,
    [UniversityId] int NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Major] PRIMARY KEY ([MajorId]),
    CONSTRAINT [FK_Major_University_UniversityId] FOREIGN KEY ([UniversityId]) REFERENCES [University] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [AvatarUrl] nvarchar(max) NOT NULL,
    [UniversityId] int NOT NULL,
    [MajorId] int NOT NULL,
    [PointsBalance] int NOT NULL,
    [TotalPointsEarned] int NOT NULL,
    [SubscriptionPlanId] int NOT NULL,
    [SubscriptionStartDate] datetime2 NULL,
    [SubscriptionEndDate] datetime2 NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUsers_Major_MajorId] FOREIGN KEY ([MajorId]) REFERENCES [Major] ([MajorId]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUsers_SubscriptionPlan_SubscriptionPlanId] FOREIGN KEY ([SubscriptionPlanId]) REFERENCES [SubscriptionPlan] ([PlanId]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUsers_University_UniversityId] FOREIGN KEY ([UniversityId]) REFERENCES [University] ([Id])
);

CREATE TABLE [Course] (
    [CourseId] int NOT NULL IDENTITY,
    [CourseCode] nvarchar(20) NOT NULL,
    [CourseName] nvarchar(200) NOT NULL,
    [MajorId] int NOT NULL,
    [Semester] tinyint NULL,
    [AcademicYear] int NULL,
    [Description] nvarchar(1000) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Course] PRIMARY KEY ([CourseId]),
    CONSTRAINT [FK_Course_Major_MajorId] FOREIGN KEY ([MajorId]) REFERENCES [Major] ([MajorId]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Document] (
    [DocumentId] int NOT NULL IDENTITY,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [DocumentType] nvarchar(20) NOT NULL,
    [FilePath] nvarchar(500) NOT NULL,
    [FileSize] bigint NOT NULL,
    [FileType] nvarchar(10) NOT NULL,
    [ThumbnailUrl] nvarchar(500) NULL,
    [UploaderId] nvarchar(450) NOT NULL,
    [CourseId] int NOT NULL,
    [ApprovedBy] nvarchar(450) NULL,
    [PointsAwarded] int NOT NULL,
    [PointsToDownload] int NOT NULL,
    [DownloadCount] int NOT NULL,
    [ViewCount] int NOT NULL,
    [LikeCount] int NOT NULL,
    [IsPremiumOnly] bit NOT NULL,
    [IsAiGenerated] bit NOT NULL,
    [AiConfidenceScore] decimal(3,2) NULL,
    [Status] nvarchar(20) NOT NULL,
    [RejectionReason] nvarchar(500) NULL,
    [ApprovedAt] datetime2 NULL,
    [LanguageCode] nvarchar(5) NOT NULL,
    [PageCount] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Document] PRIMARY KEY ([DocumentId]),
    CONSTRAINT [FK_Document_AspNetUsers_ApprovedBy] FOREIGN KEY ([ApprovedBy]) REFERENCES [AspNetUsers] ([Id]),
    CONSTRAINT [FK_Document_AspNetUsers_UploaderId] FOREIGN KEY ([UploaderId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Document_Course_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Course] ([CourseId])
);

CREATE TABLE [DocumentTag] (
    [TagId] int NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    [Color] nvarchar(7) NULL,
    [Description] nvarchar(200) NULL,
    [IsSystemTag] bit NOT NULL,
    [UsageCount] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [DocumentId] int NULL,
    CONSTRAINT [PK_DocumentTag] PRIMARY KEY ([TagId]),
    CONSTRAINT [FK_DocumentTag_Document_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [Document] ([DocumentId])
);

CREATE TABLE [UserFavorite] (
    [user_id] int NOT NULL,
    [document_id] int NOT NULL,
    [created_at] datetime2 NOT NULL,
    [UserId1] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_UserFavorite] PRIMARY KEY ([user_id], [document_id]),
    CONSTRAINT [FK_UserFavorite_AspNetUsers_UserId1] FOREIGN KEY ([UserId1]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserFavorite_Document_document_id] FOREIGN KEY ([document_id]) REFERENCES [Document] ([DocumentId])
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'TagId', N'Color', N'CreatedAt', N'Description', N'DocumentId', N'IsSystemTag', N'Name', N'UsageCount') AND [object_id] = OBJECT_ID(N'[DocumentTag]'))
    SET IDENTITY_INSERT [DocumentTag] ON;
INSERT INTO [DocumentTag] ([TagId], [Color], [CreatedAt], [Description], [DocumentId], [IsSystemTag], [Name], [UsageCount])
VALUES (1, N'#3B82F6', '2025-01-01T00:00:00.0000000Z', N'Class lecture notes', NULL, CAST(1 AS bit), N'Lecture Notes', 0),
(2, N'#10B981', '2025-01-01T00:00:00.0000000Z', N'Study guides and summaries', NULL, CAST(1 AS bit), N'Study Guide', 0),
(3, N'#F59E0B', '2025-01-01T00:00:00.0000000Z', N'Previous examination papers', NULL, CAST(1 AS bit), N'Past Exam', 0),
(4, N'#EF4444', '2025-01-01T00:00:00.0000000Z', N'Homework and assignments', NULL, CAST(1 AS bit), N'Assignment', 0),
(5, N'#8B5CF6', '2025-01-01T00:00:00.0000000Z', N'Quizzes and short tests', NULL, CAST(1 AS bit), N'Quiz', 0),
(6, N'#6B7280', '2025-01-01T00:00:00.0000000Z', N'Academic research papers', NULL, CAST(1 AS bit), N'Research Paper', 0);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'TagId', N'Color', N'CreatedAt', N'Description', N'DocumentId', N'IsSystemTag', N'Name', N'UsageCount') AND [object_id] = OBJECT_ID(N'[DocumentTag]'))
    SET IDENTITY_INSERT [DocumentTag] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PlanId', N'CreatedAt', N'Currency', N'Description', N'DisplayName', N'DurationMonths', N'IsActive', N'MaxDownloadsPerMonth', N'Name', N'PremiumContentAccess', N'Price', N'PrioritySupport') AND [object_id] = OBJECT_ID(N'[SubscriptionPlan]'))
    SET IDENTITY_INSERT [SubscriptionPlan] ON;
INSERT INTO [SubscriptionPlan] ([PlanId], [CreatedAt], [Currency], [Description], [DisplayName], [DurationMonths], [IsActive], [MaxDownloadsPerMonth], [Name], [PremiumContentAccess], [Price], [PrioritySupport])
VALUES (1, '2025-01-01T00:00:00.0000000Z', N'USD', N'Basic access with limited downloads', N'Free Plan', 12, CAST(1 AS bit), 10, N'FREE', CAST(0 AS bit), 0.0, CAST(0 AS bit)),
(2, '2025-01-01T00:00:00.0000000Z', N'USD', N'Unlimited access with AI features', N'Premium Plan', 1, CAST(1 AS bit), NULL, N'PREMIUM', CAST(1 AS bit), 200.0, CAST(1 AS bit));
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PlanId', N'CreatedAt', N'Currency', N'Description', N'DisplayName', N'DurationMonths', N'IsActive', N'MaxDownloadsPerMonth', N'Name', N'PremiumContentAccess', N'Price', N'PrioritySupport') AND [object_id] = OBJECT_ID(N'[SubscriptionPlan]'))
    SET IDENTITY_INSERT [SubscriptionPlan] OFF;

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE INDEX [IX_AspNetUsers_MajorId] ON [AspNetUsers] ([MajorId]);

CREATE INDEX [IX_AspNetUsers_SubscriptionPlanId] ON [AspNetUsers] ([SubscriptionPlanId]);

CREATE INDEX [IX_AspNetUsers_UniversityId] ON [AspNetUsers] ([UniversityId]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE UNIQUE INDEX [IX_Course_CourseCode_MajorId] ON [Course] ([CourseCode], [MajorId]);

CREATE INDEX [IX_Course_MajorId] ON [Course] ([MajorId]);

CREATE INDEX [IX_Document_ApprovedBy] ON [Document] ([ApprovedBy]);

CREATE INDEX [IX_Document_CourseId] ON [Document] ([CourseId]);

CREATE INDEX [IX_Document_UploaderId] ON [Document] ([UploaderId]);

CREATE INDEX [IX_DocumentTag_DocumentId] ON [DocumentTag] ([DocumentId]);

CREATE UNIQUE INDEX [IX_DocumentTag_Name] ON [DocumentTag] ([Name]);

CREATE UNIQUE INDEX [IX_Major_Code_UniversityId] ON [Major] ([Code], [UniversityId]) WHERE [Code] IS NOT NULL;

CREATE INDEX [IX_Major_UniversityId] ON [Major] ([UniversityId]);

CREATE UNIQUE INDEX [IX_SubscriptionPlan_Name] ON [SubscriptionPlan] ([Name]);

CREATE INDEX [IX_University_CountryId] ON [University] ([CountryId]);

CREATE INDEX [IX_UserFavorite_document_id] ON [UserFavorite] ([document_id]);

CREATE INDEX [IX_UserFavorite_UserId1] ON [UserFavorite] ([UserId1]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250823072752_First', N'9.0.8');

ALTER TABLE [AspNetUsers] DROP CONSTRAINT [FK_AspNetUsers_Major_MajorId];

ALTER TABLE [AspNetUsers] DROP CONSTRAINT [FK_AspNetUsers_SubscriptionPlan_SubscriptionPlanId];

ALTER TABLE [AspNetUsers] DROP CONSTRAINT [FK_AspNetUsers_University_UniversityId];

ALTER TABLE [Course] DROP CONSTRAINT [FK_Course_Major_MajorId];

ALTER TABLE [Document] DROP CONSTRAINT [FK_Document_AspNetUsers_ApprovedBy];

ALTER TABLE [Document] DROP CONSTRAINT [FK_Document_AspNetUsers_UploaderId];

ALTER TABLE [Document] DROP CONSTRAINT [FK_Document_Course_CourseId];

ALTER TABLE [DocumentTag] DROP CONSTRAINT [FK_DocumentTag_Document_DocumentId];

ALTER TABLE [Major] DROP CONSTRAINT [FK_Major_University_UniversityId];

ALTER TABLE [University] DROP CONSTRAINT [FK_University_Country_CountryId];

ALTER TABLE [UserFavorite] DROP CONSTRAINT [FK_UserFavorite_AspNetUsers_UserId1];

ALTER TABLE [UserFavorite] DROP CONSTRAINT [FK_UserFavorite_Document_document_id];

ALTER TABLE [UserFavorite] DROP CONSTRAINT [PK_UserFavorite];

ALTER TABLE [University] DROP CONSTRAINT [PK_University];

ALTER TABLE [SubscriptionPlan] DROP CONSTRAINT [PK_SubscriptionPlan];

ALTER TABLE [Major] DROP CONSTRAINT [PK_Major];

ALTER TABLE [DocumentTag] DROP CONSTRAINT [PK_DocumentTag];

ALTER TABLE [Document] DROP CONSTRAINT [PK_Document];

ALTER TABLE [Course] DROP CONSTRAINT [PK_Course];

ALTER TABLE [Country] DROP CONSTRAINT [PK_Country];

EXEC sp_rename N'[UserFavorite]', N'UserFavorites', 'OBJECT';

EXEC sp_rename N'[University]', N'Universities', 'OBJECT';

EXEC sp_rename N'[SubscriptionPlan]', N'SubscriptionPlans', 'OBJECT';

EXEC sp_rename N'[Major]', N'Majors', 'OBJECT';

EXEC sp_rename N'[DocumentTag]', N'DocumentTags', 'OBJECT';

EXEC sp_rename N'[Document]', N'Documents', 'OBJECT';

EXEC sp_rename N'[Course]', N'Courses', 'OBJECT';

EXEC sp_rename N'[Country]', N'Countries', 'OBJECT';

EXEC sp_rename N'[UserFavorites].[IX_UserFavorite_UserId1]', N'IX_UserFavorites_UserId1', 'INDEX';

EXEC sp_rename N'[UserFavorites].[IX_UserFavorite_document_id]', N'IX_UserFavorites_document_id', 'INDEX';

EXEC sp_rename N'[Universities].[IX_University_CountryId]', N'IX_Universities_CountryId', 'INDEX';

EXEC sp_rename N'[SubscriptionPlans].[IX_SubscriptionPlan_Name]', N'IX_SubscriptionPlans_Name', 'INDEX';

EXEC sp_rename N'[Majors].[IX_Major_UniversityId]', N'IX_Majors_UniversityId', 'INDEX';

EXEC sp_rename N'[Majors].[IX_Major_Code_UniversityId]', N'IX_Majors_Code_UniversityId', 'INDEX';

EXEC sp_rename N'[DocumentTags].[IX_DocumentTag_Name]', N'IX_DocumentTags_Name', 'INDEX';

EXEC sp_rename N'[DocumentTags].[IX_DocumentTag_DocumentId]', N'IX_DocumentTags_DocumentId', 'INDEX';

EXEC sp_rename N'[Documents].[IX_Document_UploaderId]', N'IX_Documents_UploaderId', 'INDEX';

EXEC sp_rename N'[Documents].[IX_Document_CourseId]', N'IX_Documents_CourseId', 'INDEX';

EXEC sp_rename N'[Documents].[IX_Document_ApprovedBy]', N'IX_Documents_ApprovedBy', 'INDEX';

EXEC sp_rename N'[Courses].[IX_Course_MajorId]', N'IX_Courses_MajorId', 'INDEX';

EXEC sp_rename N'[Courses].[IX_Course_CourseCode_MajorId]', N'IX_Courses_CourseCode_MajorId', 'INDEX';

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UserFavorites]') AND [c].[name] = N'UserId1');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [UserFavorites] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [UserFavorites] ALTER COLUMN [UserId1] nvarchar(450) NULL;

ALTER TABLE [UserFavorites] ADD CONSTRAINT [PK_UserFavorites] PRIMARY KEY ([user_id], [document_id]);

ALTER TABLE [Universities] ADD CONSTRAINT [PK_Universities] PRIMARY KEY ([Id]);

ALTER TABLE [SubscriptionPlans] ADD CONSTRAINT [PK_SubscriptionPlans] PRIMARY KEY ([PlanId]);

ALTER TABLE [Majors] ADD CONSTRAINT [PK_Majors] PRIMARY KEY ([MajorId]);

ALTER TABLE [DocumentTags] ADD CONSTRAINT [PK_DocumentTags] PRIMARY KEY ([TagId]);

ALTER TABLE [Documents] ADD CONSTRAINT [PK_Documents] PRIMARY KEY ([DocumentId]);

ALTER TABLE [Courses] ADD CONSTRAINT [PK_Courses] PRIMARY KEY ([CourseId]);

ALTER TABLE [Countries] ADD CONSTRAINT [PK_Countries] PRIMARY KEY ([Id]);

CREATE TABLE [DocumentReviews] (
    [review_id] int NOT NULL IDENTITY,
    [document_id] int NOT NULL,
    [reviewer_id] int NOT NULL,
    [rating] tinyint NOT NULL,
    [comment] nvarchar(1000) NULL,
    [helpful_count] int NOT NULL,
    [created_at] datetime2 NOT NULL,
    [updated_at] datetime2 NOT NULL,
    [ReviewerId1] nvarchar(450) NULL,
    CONSTRAINT [PK_DocumentReviews] PRIMARY KEY ([review_id]),
    CONSTRAINT [FK_DocumentReviews_AspNetUsers_ReviewerId1] FOREIGN KEY ([ReviewerId1]) REFERENCES [AspNetUsers] ([Id]),
    CONSTRAINT [FK_DocumentReviews_Documents_document_id] FOREIGN KEY ([document_id]) REFERENCES [Documents] ([DocumentId]) ON DELETE CASCADE
);

CREATE TABLE [PointTransactions] (
    [transaction_id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [points_change] int NOT NULL,
    [transaction_type] nvarchar(30) NOT NULL,
    [reference_id] int NULL,
    [reference_type] nvarchar(20) NOT NULL,
    [description] nvarchar(200) NOT NULL,
    [balance_after] int NOT NULL,
    [expires_at] datetime2 NULL,
    [created_at] datetime2 NOT NULL,
    [UserId1] nvarchar(450) NULL,
    CONSTRAINT [PK_PointTransactions] PRIMARY KEY ([transaction_id]),
    CONSTRAINT [FK_PointTransactions_AspNetUsers_UserId1] FOREIGN KEY ([UserId1]) REFERENCES [AspNetUsers] ([Id])
);

CREATE TABLE [RecentVieweds] (
    [Id] int NOT NULL IDENTITY,
    [DocumentId] int NOT NULL,
    [CourseId] int NOT NULL,
    CONSTRAINT [PK_RecentVieweds] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RecentVieweds_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([CourseId]) ON DELETE CASCADE,
    CONSTRAINT [FK_RecentVieweds_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [Documents] ([DocumentId]) ON DELETE CASCADE
);

CREATE TABLE [UserFollowers] (
    [FollowerId] nvarchar(450) NOT NULL,
    [FollowingId] nvarchar(450) NOT NULL,
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [PK_UserFollowers] PRIMARY KEY ([FollowerId], [FollowingId]),
    CONSTRAINT [FK_UserFollowers_AspNetUsers_FollowerId] FOREIGN KEY ([FollowerId]) REFERENCES [AspNetUsers] ([Id]),
    CONSTRAINT [FK_UserFollowers_AspNetUsers_FollowingId] FOREIGN KEY ([FollowingId]) REFERENCES [AspNetUsers] ([Id])
);

CREATE INDEX [IX_DocumentReviews_document_id] ON [DocumentReviews] ([document_id]);

CREATE INDEX [IX_DocumentReviews_ReviewerId1] ON [DocumentReviews] ([ReviewerId1]);

CREATE INDEX [IX_PointTransactions_UserId1] ON [PointTransactions] ([UserId1]);

CREATE INDEX [IX_RecentVieweds_CourseId] ON [RecentVieweds] ([CourseId]);

CREATE INDEX [IX_RecentVieweds_DocumentId] ON [RecentVieweds] ([DocumentId]);

CREATE INDEX [IX_UserFollowers_FollowingId] ON [UserFollowers] ([FollowingId]);

ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_Majors_MajorId] FOREIGN KEY ([MajorId]) REFERENCES [Majors] ([MajorId]) ON DELETE CASCADE;

ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_SubscriptionPlans_SubscriptionPlanId] FOREIGN KEY ([SubscriptionPlanId]) REFERENCES [SubscriptionPlans] ([PlanId]) ON DELETE CASCADE;

ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_Universities_UniversityId] FOREIGN KEY ([UniversityId]) REFERENCES [Universities] ([Id]);

ALTER TABLE [Courses] ADD CONSTRAINT [FK_Courses_Majors_MajorId] FOREIGN KEY ([MajorId]) REFERENCES [Majors] ([MajorId]) ON DELETE CASCADE;

ALTER TABLE [Documents] ADD CONSTRAINT [FK_Documents_AspNetUsers_ApprovedBy] FOREIGN KEY ([ApprovedBy]) REFERENCES [AspNetUsers] ([Id]);

ALTER TABLE [Documents] ADD CONSTRAINT [FK_Documents_AspNetUsers_UploaderId] FOREIGN KEY ([UploaderId]) REFERENCES [AspNetUsers] ([Id]);

ALTER TABLE [Documents] ADD CONSTRAINT [FK_Documents_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([CourseId]);

ALTER TABLE [DocumentTags] ADD CONSTRAINT [FK_DocumentTags_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [Documents] ([DocumentId]);

ALTER TABLE [Majors] ADD CONSTRAINT [FK_Majors_Universities_UniversityId] FOREIGN KEY ([UniversityId]) REFERENCES [Universities] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Universities] ADD CONSTRAINT [FK_Universities_Countries_CountryId] FOREIGN KEY ([CountryId]) REFERENCES [Countries] ([Id]) ON DELETE CASCADE;

ALTER TABLE [UserFavorites] ADD CONSTRAINT [FK_UserFavorites_AspNetUsers_UserId1] FOREIGN KEY ([UserId1]) REFERENCES [AspNetUsers] ([Id]);

ALTER TABLE [UserFavorites] ADD CONSTRAINT [FK_UserFavorites_Documents_document_id] FOREIGN KEY ([document_id]) REFERENCES [Documents] ([DocumentId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250828131428_UserFollower', N'9.0.8');

ALTER TABLE [AspNetUsers] DROP CONSTRAINT [FK_AspNetUsers_Majors_MajorId];

ALTER TABLE [AspNetUsers] DROP CONSTRAINT [FK_AspNetUsers_SubscriptionPlans_SubscriptionPlanId];

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'UniversityId');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [AspNetUsers] ALTER COLUMN [UniversityId] int NULL;

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'SubscriptionPlanId');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [AspNetUsers] ALTER COLUMN [SubscriptionPlanId] int NULL;

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'MajorId');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [AspNetUsers] ALTER COLUMN [MajorId] int NULL;

ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_Majors_MajorId] FOREIGN KEY ([MajorId]) REFERENCES [Majors] ([MajorId]);

ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_SubscriptionPlans_SubscriptionPlanId] FOREIGN KEY ([SubscriptionPlanId]) REFERENCES [SubscriptionPlans] ([PlanId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250828131931_UserFix', N'9.0.8');

ALTER TABLE [AspNetUsers] DROP CONSTRAINT [FK_AspNetUsers_Majors_MajorId];

ALTER TABLE [AspNetUsers] DROP CONSTRAINT [FK_AspNetUsers_SubscriptionPlans_SubscriptionPlanId];

ALTER TABLE [Courses] DROP CONSTRAINT [FK_Courses_Majors_MajorId];

ALTER TABLE [DocumentReviews] DROP CONSTRAINT [FK_DocumentReviews_AspNetUsers_ReviewerId1];

ALTER TABLE [Documents] DROP CONSTRAINT [FK_Documents_AspNetUsers_ApprovedBy];

ALTER TABLE [DocumentTags] DROP CONSTRAINT [FK_DocumentTags_Documents_DocumentId];

ALTER TABLE [Majors] DROP CONSTRAINT [FK_Majors_Universities_UniversityId];

ALTER TABLE [PointTransactions] DROP CONSTRAINT [FK_PointTransactions_AspNetUsers_UserId1];

ALTER TABLE [Universities] DROP CONSTRAINT [FK_Universities_Countries_CountryId];

ALTER TABLE [UserFavorites] DROP CONSTRAINT [FK_UserFavorites_AspNetUsers_UserId1];

ALTER TABLE [UserFavorites] DROP CONSTRAINT [FK_UserFavorites_Documents_document_id];

ALTER TABLE [UserFollowers] DROP CONSTRAINT [PK_UserFollowers];

ALTER TABLE [UserFavorites] DROP CONSTRAINT [PK_UserFavorites];

DROP INDEX [IX_UserFavorites_UserId1] ON [UserFavorites];

DROP INDEX [IX_PointTransactions_UserId1] ON [PointTransactions];

DROP INDEX [IX_DocumentTags_DocumentId] ON [DocumentTags];

DROP INDEX [IX_DocumentReviews_ReviewerId1] ON [DocumentReviews];

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UserFavorites]') AND [c].[name] = N'UserId1');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [UserFavorites] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [UserFavorites] DROP COLUMN [UserId1];

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PointTransactions]') AND [c].[name] = N'UserId1');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [PointTransactions] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [PointTransactions] DROP COLUMN [UserId1];

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DocumentTags]') AND [c].[name] = N'DocumentId');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [DocumentTags] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [DocumentTags] DROP COLUMN [DocumentId];

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DocumentReviews]') AND [c].[name] = N'ReviewerId1');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [DocumentReviews] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [DocumentReviews] DROP COLUMN [ReviewerId1];

EXEC sp_rename N'[UserFavorites].[document_id]', N'DocumentId', 'COLUMN';

EXEC sp_rename N'[UserFavorites].[user_id]', N'UserId', 'COLUMN';

EXEC sp_rename N'[UserFavorites].[IX_UserFavorites_document_id]', N'IX_UserFavorites_DocumentId', 'INDEX';

ALTER TABLE [UserFollowers] ADD [Id] int NOT NULL IDENTITY;

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UserFavorites]') AND [c].[name] = N'DocumentId');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [UserFavorites] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [UserFavorites] ALTER COLUMN [DocumentId] int NOT NULL;

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UserFavorites]') AND [c].[name] = N'UserId');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [UserFavorites] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [UserFavorites] ALTER COLUMN [UserId] nvarchar(450) NOT NULL;

ALTER TABLE [UserFavorites] ADD [Id] int NOT NULL IDENTITY;

ALTER TABLE [RecentVieweds] ADD [UserId] nvarchar(450) NOT NULL DEFAULT N'';

ALTER TABLE [RecentVieweds] ADD [ViewedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PointTransactions]') AND [c].[name] = N'user_id');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [PointTransactions] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [PointTransactions] ALTER COLUMN [user_id] nvarchar(450) NOT NULL;

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DocumentReviews]') AND [c].[name] = N'reviewer_id');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [DocumentReviews] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [DocumentReviews] ALTER COLUMN [reviewer_id] nvarchar(450) NOT NULL;

ALTER TABLE [UserFollowers] ADD CONSTRAINT [PK_UserFollowers] PRIMARY KEY ([Id]);

ALTER TABLE [UserFavorites] ADD CONSTRAINT [PK_UserFavorites] PRIMARY KEY ([Id]);

CREATE TABLE [DocumentDocumentTags] (
    [DocumentId] int NOT NULL,
    [TagsTagId] int NOT NULL,
    CONSTRAINT [PK_DocumentDocumentTags] PRIMARY KEY ([DocumentId], [TagsTagId]),
    CONSTRAINT [FK_DocumentDocumentTags_DocumentTags_TagsTagId] FOREIGN KEY ([TagsTagId]) REFERENCES [DocumentTags] ([TagId]) ON DELETE CASCADE,
    CONSTRAINT [FK_DocumentDocumentTags_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [Documents] ([DocumentId]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_UserFollowers_FollowerId_FollowingId] ON [UserFollowers] ([FollowerId], [FollowingId]);

CREATE UNIQUE INDEX [IX_UserFavorites_UserId_DocumentId] ON [UserFavorites] ([UserId], [DocumentId]);

CREATE INDEX [IX_RecentVieweds_UserId] ON [RecentVieweds] ([UserId]);

CREATE INDEX [IX_PointTransactions_user_id] ON [PointTransactions] ([user_id]);

CREATE INDEX [IX_DocumentReviews_reviewer_id] ON [DocumentReviews] ([reviewer_id]);

CREATE INDEX [IX_DocumentDocumentTags_TagsTagId] ON [DocumentDocumentTags] ([TagsTagId]);

ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_Majors_MajorId] FOREIGN KEY ([MajorId]) REFERENCES [Majors] ([MajorId]) ON DELETE SET NULL;

ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_SubscriptionPlans_SubscriptionPlanId] FOREIGN KEY ([SubscriptionPlanId]) REFERENCES [SubscriptionPlans] ([PlanId]) ON DELETE SET NULL;

ALTER TABLE [Courses] ADD CONSTRAINT [FK_Courses_Majors_MajorId] FOREIGN KEY ([MajorId]) REFERENCES [Majors] ([MajorId]);

ALTER TABLE [DocumentReviews] ADD CONSTRAINT [FK_DocumentReviews_AspNetUsers_reviewer_id] FOREIGN KEY ([reviewer_id]) REFERENCES [AspNetUsers] ([Id]);

ALTER TABLE [Documents] ADD CONSTRAINT [FK_Documents_AspNetUsers_ApprovedBy] FOREIGN KEY ([ApprovedBy]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL;

ALTER TABLE [Majors] ADD CONSTRAINT [FK_Majors_Universities_UniversityId] FOREIGN KEY ([UniversityId]) REFERENCES [Universities] ([Id]);

ALTER TABLE [PointTransactions] ADD CONSTRAINT [FK_PointTransactions_AspNetUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;

ALTER TABLE [RecentVieweds] ADD CONSTRAINT [FK_RecentVieweds_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Universities] ADD CONSTRAINT [FK_Universities_Countries_CountryId] FOREIGN KEY ([CountryId]) REFERENCES [Countries] ([Id]);

ALTER TABLE [UserFavorites] ADD CONSTRAINT [FK_UserFavorites_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;

ALTER TABLE [UserFavorites] ADD CONSTRAINT [FK_UserFavorites_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [Documents] ([DocumentId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250911120210_Fix_key', N'9.0.8');

CREATE TABLE [PaymentRecords] (
    [PaymentId] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Currency] nvarchar(3) NOT NULL,
    [PaymentMethod] nvarchar(50) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ProcessedAt] datetime2 NULL,
    [Notes] nvarchar(1000) NULL,
    CONSTRAINT [PK_PaymentRecords] PRIMARY KEY ([PaymentId]),
    CONSTRAINT [FK_PaymentRecords_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_PaymentRecords_UserId] ON [PaymentRecords] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250920113556_Add Payment', N'9.0.8');

ALTER TABLE [Documents] ADD [FilePDFpath] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250926061640_Add Pdf file path Document', N'9.0.8');

CREATE TABLE [DocumentLikes] (
    [Id] int NOT NULL IDENTITY,
    [DocumentId] int NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [LikedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_DocumentLikes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DocumentLikes_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_DocumentLikes_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [Documents] ([DocumentId]) ON DELETE CASCADE
);

CREATE INDEX [IX_DocumentLikes_DocumentId] ON [DocumentLikes] ([DocumentId]);

CREATE INDEX [IX_DocumentLikes_UserId] ON [DocumentLikes] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250928082220_AddDocumentLikeTable', N'9.0.8');

ALTER TABLE [DocumentLikes] DROP CONSTRAINT [FK_DocumentLikes_AspNetUsers_UserId];

ALTER TABLE [DocumentLikes] DROP CONSTRAINT [FK_DocumentLikes_Documents_DocumentId];

DROP INDEX [IX_DocumentLikes_DocumentId] ON [DocumentLikes];

DROP INDEX [IX_DocumentLikes_UserId] ON [DocumentLikes];

CREATE UNIQUE INDEX [IX_DocumentLikes_DocumentId] ON [DocumentLikes] ([DocumentId]);

CREATE UNIQUE INDEX [IX_DocumentLikes_UserId] ON [DocumentLikes] ([UserId]);

ALTER TABLE [DocumentLikes] ADD CONSTRAINT [FK_DocumentLikes_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]);

ALTER TABLE [DocumentLikes] ADD CONSTRAINT [FK_DocumentLikes_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [Documents] ([DocumentId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250928154513_DocumentLike', N'9.0.8');

COMMIT;
GO

