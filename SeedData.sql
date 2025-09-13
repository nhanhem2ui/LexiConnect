USE [LexiConnect]
GO

SET IDENTITY_INSERT [dbo].[Countries] ON;
INSERT INTO [dbo].[Countries] ([Id],[CountryName])
VALUES 
    (1,N'Vietnam'),
    (2, N'United States'),
    (3, N'Japan'),
    (4, N'France'),
    (5, N'Germany');
SET IDENTITY_INSERT [dbo].[Countries] OFF;

GO
SET IDENTITY_INSERT [dbo].[Universities] ON;
INSERT INTO [dbo].[Universities]
           ([Id], [Name], [ShortName], [CountryId], [City], [IsVerified], [LogoUrl], [CreatedAt])
VALUES
(0, N'Not provided', N'', 1, N'', 1, N'', SYSDATETIME()),

(1, N'Đại học Quốc gia Hà Nội', N'VNU', 1, N'Hà Nội', 1, N'/image/logo_VNU.jpg', SYSDATETIME()),

(2, N'Đại học Quốc gia Thành phố Hồ Chí Minh', N'VNUHCM', 1, N'TP. Hồ Chí Minh', 1, N'/image/logo_VNUHCM.png', SYSDATETIME()),

(3, N'Đại học FPT Đà Nẵng', N'FUDA', 1, N'TP. Đà Nẵng', 1, N'/image/logo_FPT.png', SYSDATETIME())
GO
SET IDENTITY_INSERT [dbo].[Universities] OFF;

GO
SET IDENTITY_INSERT [dbo].[Majors] ON;
INSERT INTO [dbo].[Majors]
           ([MajorId], [Name], [Code], [UniversityId], [Description], [IsActive], [CreatedAt])
VALUES
    (0, N'Not provided', '', 0, N'', 1, GETDATE()),
    (1, N'Software Engineering', 'SE', 3, N'Study of software design, development, and testing.', 1, GETDATE()),
    (2, N'Information Security', 'IS', 3, N'Study of cybersecurity, cryptography, and secure systems.', 1, GETDATE()),
    (3, N'Artificial Intelligence', 'AI', 3, N'Focus on machine learning, data science, and AI applications.', 1, GETDATE()),
    (4, N'Business Administration', 'BA', 3, N'Management, finance, marketing, and organizational studies.', 1, GETDATE()),
    (5, N'International Business', 'IB', 3, N'Business strategies in global markets and international trade.', 1, GETDATE()),
    (6, N'Graphic Design', 'GD', 3, N'Visual communication, multimedia, and creative design.', 1, GETDATE()),
    (7, N'Digital Marketing', 'DM', 3, N'Online marketing strategies, SEO, and social media.', 1, GETDATE()),
    (8, N'Tourism and Hospitality Management', 'THM', 3, N'Management of travel, tourism, and hospitality industries.', 1, GETDATE());
GO

-- Majors for VNU (UniversityId = 1)
INSERT INTO [dbo].[Majors]
           ([MajorId], [Name], [Code], [UniversityId], [Description], [IsActive], [CreatedAt])
VALUES
    (9, N'Computer Science', 'CS', 1, N'Focus on algorithms, programming languages, and theoretical foundations of computing.', 1, GETDATE()),
    (10, N'Mathematics', 'MATH', 1, N'Study of pure and applied mathematics, logic, and statistics.', 1, GETDATE()),
    (11, N'Physics', 'PHY', 1, N'Focus on classical mechanics, electromagnetism, and quantum physics.', 1, GETDATE());

-- Majors for VNUHCM (UniversityId = 2)
INSERT INTO [dbo].[Majors]
           ([MajorId], [Name], [Code], [UniversityId], [Description], [IsActive], [CreatedAt])
VALUES
    (12, N'Biotechnology', 'BIO', 2, N'Study of genetics, molecular biology, and biotech applications.', 1, GETDATE()),
    (13, N'Civil Engineering', 'CE', 2, N'Design and construction of infrastructure projects like bridges, roads, and buildings.', 1, GETDATE()),
    (14, N'Chemical Engineering', 'CHE', 2, N'Processes involving chemical production, materials science, and industrial systems.', 1, GETDATE());

SET IDENTITY_INSERT [dbo].[Majors] OFF;
GO

SET IDENTITY_INSERT [dbo].[Courses] ON;
-- Example courses for Software Engineering Major (MajorId = 1, adjust to your actual MajorId)
INSERT INTO [dbo].[Courses]
           ([CourseId], [CourseCode], [CourseName], [MajorId], [Semester], [AcademicYear], [Description], [IsActive], [CreatedAt])
VALUES
    (1,'PRF192', N'Programming Fundamentals', 1, 1, 2025, N'Introduction to programming using C/C++. Covers basic programming concepts and problem-solving.', 1, GETDATE()),
    (2, 'PRO201', N'Object-Oriented Programming with Java', 1, 2, 2025, N'OOP concepts using Java, including inheritance, polymorphism, and interfaces.', 1, GETDATE()),
    (3, 'DBI202', N'Database Systems', 1, 2, 2025, N'Fundamentals of relational databases, SQL, normalization, and database design.', 1, GETDATE()),
    (4, 'WED201', N'Web Application Development', 1, 3, 2025, N'Building dynamic websites using ASP.NET/Java technologies.', 1, GETDATE()),
    (5, 'SWE301', N'Software Engineering Practices', 1, 5, 2025, N'Principles of software development life cycle, Agile, and project management.', 1, GETDATE());

-- Example courses for Artificial Intelligence Major (MajorId = 2)
INSERT INTO [dbo].[Courses]
           ([CourseId], [CourseCode], [CourseName], [MajorId], [Semester], [AcademicYear], [Description], [IsActive], [CreatedAt])
VALUES
    (6, 'AI101', N'Introduction to Artificial Intelligence', 3, 1, 2025, N'Overview of AI history, concepts, and applications.', 1, GETDATE()),
    (7, 'ML201', N'Machine Learning Basics', 3, 3, 2025, N'Introduction to supervised and unsupervised learning algorithms.', 1, GETDATE()),
    (8, 'DL301', N'Deep Learning', 3, 5, 2025, N'Neural networks, CNNs, RNNs, and applications in image and speech recognition.', 1, GETDATE());

-- Example courses for Business Administration Major (MajorId = 3)
INSERT INTO [dbo].[Courses]
           ([CourseId], [CourseCode], [CourseName], [MajorId], [Semester], [AcademicYear], [Description], [IsActive], [CreatedAt])
VALUES
    (9, 'MKT101', N'Marketing Principles', 4, 1, 2025, N'Fundamentals of marketing, consumer behavior, and branding.', 1, GETDATE()),
    (10, 'ACC201', N'Financial Accounting', 4, 2, 2025, N'Introduction to accounting concepts, balance sheets, and financial analysis.', 1, GETDATE()),
    (11, 'HRM301', N'Human Resource Management', 4, 4, 2025, N'Study of HR policies, recruitment, and employee relations.', 1, GETDATE());
GO

-- Courses for Computer Science (MajorId = 9, UniversityId = 1)
INSERT INTO [dbo].[Courses]
           ([CourseId], [CourseCode], [CourseName], [MajorId], [Semester], [AcademicYear], [Description], [IsActive], [CreatedAt])
VALUES
    (12, 'CS101', N'Introduction to Computer Science', 9, 1, 2025, N'Foundations of computing, data structures, and programming basics.', 1, GETDATE()),
    (13, 'CS201', N'Data Structures & Algorithms', 9, 2, 2025, N'Analysis and implementation of common data structures and algorithms.', 1, GETDATE()),
    (14, 'CS301', N'Operating Systems', 9, 3, 2025, N'Concepts of processes, memory management, and file systems.', 1, GETDATE());

-- Courses for Mathematics (MajorId = 10)
INSERT INTO [dbo].[Courses]
           ([CourseId], [CourseCode], [CourseName], [MajorId], [Semester], [AcademicYear], [Description], [IsActive], [CreatedAt])
VALUES
    (15, 'MATH101', N'Calculus I', 10, 1, 2025, N'Differentiation, integration, and applications.', 1, GETDATE()),
    (16, 'MATH201', N'Linear Algebra', 10, 2, 2025, N'Matrices, vector spaces, and linear transformations.', 1, GETDATE());

-- Courses for Biotechnology (MajorId = 12)
INSERT INTO [dbo].[Courses]
           ([CourseId], [CourseCode], [CourseName], [MajorId], [Semester], [AcademicYear], [Description], [IsActive], [CreatedAt])
VALUES
    (17, 'BIO101', N'General Biology', 12, 1, 2025, N'Cell structure, genetics, and evolution.', 1, GETDATE()),
    (18, 'BIO201', N'Molecular Biology', 12, 3, 2025, N'Gene expression, DNA replication, and protein synthesis.', 1, GETDATE());

-- Courses for Civil Engineering (MajorId = 13)
INSERT INTO [dbo].[Courses]
           ([CourseId], [CourseCode], [CourseName], [MajorId], [Semester], [AcademicYear], [Description], [IsActive], [CreatedAt])
VALUES
    (19, 'CE101', N'Engineering Mechanics', 13, 1, 2025, N'Statics, dynamics, and structural analysis.', 1, GETDATE()),
    (20, 'CE201', N'Structural Engineering', 13, 3, 2025, N'Design and analysis of structures and materials.', 1, GETDATE());

SET IDENTITY_INSERT [dbo].[Courses] OFF;
GO

SET IDENTITY_INSERT [dbo].[Documents] ON;

INSERT INTO [dbo].[Documents]
    ([DocumentId], [Title], [Description], [DocumentType], [FilePath], [FileSize], [FileType],
     [ThumbnailUrl], [UploaderId], [CourseId], [ApprovedBy], [PointsAwarded], [PointsToDownload],
     [DownloadCount], [ViewCount], [LikeCount], [IsPremiumOnly], [IsAiGenerated], [AiConfidenceScore],
     [Status], [RejectionReason], [ApprovedAt], [LanguageCode], [PageCount], [CreatedAt], [UpdatedAt])
VALUES
-- Example Document 1
(1, N'PRF192 Lecture Notes', N'Full lecture notes for Programming Fundamentals.',
 'notes', N'/uploads/docs/PRF192_notes.pdf', 524288, 'pdf',
 N'/uploads/thumbnails/PRF192_notes.png', 'cb1accbd-4502-4222-ad98-cdc11892d287', 1, NULL, 
 10, 5, 0, 0, 0, 0, 0, NULL, 
 'approved', NULL, GETDATE(), 'en', 120, GETDATE(), GETDATE()),

-- Example Document 2
(2, N'OOP with Java Assignment', N'Assignment 1 for Java OOP course.',
 'assignment', N'/uploads/docs/PRO201_assignment1.docx', 1048576, 'docx',
 N'/uploads/thumbnails/PRO201_assignment1.png', 'cb1accbd-4502-4222-ad98-cdc11892d287', 2, NULL,
 15, 10, 0, 0, 0, 0, 1, 0.95,
 'pending', NULL, NULL, 'en', 15, GETDATE(), GETDATE()),

-- Example Document 3
(3, N'Database Systems Quiz', N'Quiz with answers for DBI202.',
 'quiz', N'/uploads/docs/DBI202_quiz.pdf', 256000, 'pdf',
 N'/uploads/thumbnails/DBI202_quiz.png', 'cb1accbd-4502-4222-ad98-cdc11892d287', 3, NULL,
 5, 3, 0, 0, 0, 0, 0, NULL,
 'approved', NULL, GETDATE(), 'en', 10, GETDATE(), GETDATE());

SET IDENTITY_INSERT [dbo].[Documents] OFF;
GO

GO

