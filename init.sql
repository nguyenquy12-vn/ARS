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
GO

CREATE TABLE [JobCategories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(250) NULL,
    CONSTRAINT [PK_JobCategories] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Permissions] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(max) NULL,
    CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Roles] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    [Description] nvarchar(max) NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [RolePermissions] (
    [RoleId] int NOT NULL,
    [PermissionId] int NOT NULL,
    CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([RoleId], [PermissionId]),
    CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RolePermissions_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [FullName] nvarchar(100) NOT NULL,
    [Email] nvarchar(150) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [PhoneNumber] nvarchar(20) NULL,
    [RoleId] int NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Users_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Companies] (
    [Id] int NOT NULL IDENTITY,
    [RecruiterId] int NOT NULL,
    [CompanyName] nvarchar(200) NOT NULL,
    [TaxCode] nvarchar(50) NOT NULL,
    [Address] nvarchar(255) NULL,
    [LogoPath] nvarchar(500) NULL,
    [CompanySize] nvarchar(50) NULL,
    [Overview] nvarchar(max) NULL,
    [Website] nvarchar(255) NULL,
    CONSTRAINT [PK_Companies] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Companies_Users_RecruiterId] FOREIGN KEY ([RecruiterId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [RecruiterRequests] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [CompanyName] nvarchar(200) NOT NULL,
    [TaxCode] nvarchar(50) NOT NULL,
    [DocumentPath] nvarchar(max) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [AdminNotes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ProcessedAt] datetime2 NULL,
    CONSTRAINT [PK_RecruiterRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RecruiterRequests_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Resumes] (
    [Id] int NOT NULL IDENTITY,
    [CandidateId] int NOT NULL,
    [Title] nvarchar(150) NOT NULL,
    [FilePath] nvarchar(500) NOT NULL,
    [IsDefault] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [RawTextContent] nvarchar(max) NULL,
    CONSTRAINT [PK_Resumes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Resumes_Users_CandidateId] FOREIGN KEY ([CandidateId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [JobPostings] (
    [Id] int NOT NULL IDENTITY,
    [CompanyId] int NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Requirements] nvarchar(max) NOT NULL,
    [Benefits] nvarchar(max) NULL,
    [Location] nvarchar(100) NOT NULL,
    [JobType] int NOT NULL,
    [WorkMode] int NOT NULL,
    [JobCategoryId] int NOT NULL,
    [MinSalary] int NULL,
    [MaxSalary] int NULL,
    [Status] int NOT NULL,
    [Vacancies] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ExpiredAt] datetime2 NOT NULL,
    CONSTRAINT [PK_JobPostings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_JobPostings_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_JobPostings_JobCategories_JobCategoryId] FOREIGN KEY ([JobCategoryId]) REFERENCES [JobCategories] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Application] (
    [Id] int NOT NULL IDENTITY,
    [JobPostingId] int NOT NULL,
    [CandidateId] int NOT NULL,
    [ResumeId] int NOT NULL,
    [CoverLetter] nvarchar(1000) NULL,
    [AppliedAt] datetime2 NOT NULL,
    [Status] int NOT NULL,
    [AiMatchScore] int NULL,
    [AiFeedback] nvarchar(max) NULL,
    CONSTRAINT [PK_Application] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Application_JobPostings_JobPostingId] FOREIGN KEY ([JobPostingId]) REFERENCES [JobPostings] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Application_Resumes_ResumeId] FOREIGN KEY ([ResumeId]) REFERENCES [Resumes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Application_Users_CandidateId] FOREIGN KEY ([CandidateId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[JobCategories]'))
    SET IDENTITY_INSERT [JobCategories] ON;
INSERT INTO [JobCategories] ([Id], [Description], [Name])
VALUES (1, N'Lập trình viên, Tester, AI Engineer, DevOps...', N'Công nghệ thông tin / Phần mềm'),
(2, N'Sales Executive, Account Manager, Chăm sóc khách hàng...', N'Kinh doanh / Bán hàng'),
(3, N'Digital Marketing, Content Creator, SEO, Event Organizer...', N'Marketing / Truyền thông'),
(4, N'Kế toán tổng hợp, Kiểm toán viên, Phân tích tài chính...', N'Tài chính / Kế toán'),
(5, N'Tuyển dụng, C&B, Trợ lý, Quản lý văn phòng...', N'Hành chính / Nhân sự'),
(6, N'UI/UX Designer, Graphic Designer, Video Editor...', N'Thiết kế / Đồ họa'),
(7, N'Phiên dịch viên Tiếng Anh, Tiếng Trung, Tiếng Nhật...', N'Biên phiên dịch / Ngôn ngữ');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[JobCategories]'))
    SET IDENTITY_INSERT [JobCategories] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[Permissions]'))
    SET IDENTITY_INSERT [Permissions] ON;
INSERT INTO [Permissions] ([Id], [Description], [Name])
VALUES (1, N'Xem danh sách tin tuyển dụng', N'ViewJob'),
(2, N'Tạo mới tin tuyển dụng', N'CreateJob'),
(3, N'Chỉnh sửa tin tuyển dụng', N'EditJob'),
(4, N'Xóa tin tuyển dụng', N'DeleteJob'),
(5, N'Nộp hồ sơ ứng tuyển (CV)', N'ApplyJob'),
(6, N'Xem và đánh giá hồ sơ ứng viên', N'ReviewCV'),
(7, N'Sử dụng trí tuệ nhân tạo (AI) để chấm điểm CV', N'EvaluateAI'),
(8, N'Quản lý vai trò và phân quyền hệ thống', N'ManageRoles'),
(9, N'Quản lý danh sách người dùng', N'ManageUsers');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[Permissions]'))
    SET IDENTITY_INSERT [Permissions] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[Roles]'))
    SET IDENTITY_INSERT [Roles] ON;
INSERT INTO [Roles] ([Id], [Description], [Name])
VALUES (1, N'Quản trị viên toàn quyền hệ thống', N'Admin'),
(2, N'Nhà tuyển dụng (Đăng tin, duyệt CV, dùng AI)', N'Recruiter'),
(3, N'Ứng viên (Tìm việc, nộp CV)', N'Candidate');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[Roles]'))
    SET IDENTITY_INSERT [Roles] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Email', N'FullName', N'PasswordHash', N'PhoneNumber', N'RoleId', N'Status') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([Id], [CreatedAt], [Email], [FullName], [PasswordHash], [PhoneNumber], [RoleId], [Status])
VALUES (1, '2026-07-20T14:01:29.7599995Z', N'admin@ars.com', N'Hệ Thống Admin', N'$2a$11$M96I7clW6g7Y9bIvxX6gAexW7R4K1N.8h7Z62Lg82Mv7C5K1lK31.', N'0123456789', 1, N'Active'),
(2, '2026-07-20T14:01:29.7599998Z', N'recruiter1@fpt.com', N'Nguyễn Văn Tuyển FPT', N'$2a$11$M96I7clW6g7Y9bIvxX6gAexW7R4K1N.8h7Z62Lg82Mv7C5K1lK31.', N'0987654321', 2, N'Active'),
(3, '2026-07-20T14:01:29.7600000Z', N'recruiter2@viettel.com', N'Trần Thị Duyệt Viettel', N'$2a$11$M96I7clW6g7Y9bIvxX6gAexW7R4K1N.8h7Z62Lg82Mv7C5K1lK31.', N'0912345678', 2, N'Active'),
(4, '2026-07-20T14:01:29.7600002Z', N'candidate1@gmail.com', N'Lê Văn Pro .NET', N'$2a$11$M96I7clW6g7Y9bIvxX6gAexW7R4K1N.8h7Z62Lg82Mv7C5K1lK31.', N'0333444555', 3, N'Active'),
(5, '2026-07-20T14:01:29.7600004Z', N'candidate2@gmail.com', N'Nguyễn Thị Fresher', N'$2a$11$M96I7clW6g7Y9bIvxX6gAexW7R4K1N.8h7Z62Lg82Mv7C5K1lK31.', N'0333444666', 3, N'Active'),
(6, '2026-07-20T14:01:29.7600006Z', N'candidate3@gmail.com', N'Trần Văn Intern', N'$2a$11$M96I7clW6g7Y9bIvxX6gAexW7R4K1N.8h7Z62Lg82Mv7C5K1lK31.', N'0333444777', 3, N'Active'),
(7, '2026-07-20T14:01:29.7600008Z', N'candidate4@gmail.com', N'Hoàng Lệ Trái Ngành', N'$2a$11$M96I7clW6g7Y9bIvxX6gAexW7R4K1N.8h7Z62Lg82Mv7C5K1lK31.', N'0333444888', 3, N'Active');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Email', N'FullName', N'PasswordHash', N'PhoneNumber', N'RoleId', N'Status') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Address', N'CompanyName', N'CompanySize', N'LogoPath', N'Overview', N'RecruiterId', N'TaxCode', N'Website') AND [object_id] = OBJECT_ID(N'[Companies]'))
    SET IDENTITY_INSERT [Companies] ON;
INSERT INTO [Companies] ([Id], [Address], [CompanyName], [CompanySize], [LogoPath], [Overview], [RecruiterId], [TaxCode], [Website])
VALUES (1, N'Khu Công nghệ cao Hòa Lạc, Thạch Thất, Hà Nội', N'FPT Software', N'10000+ nhân viên', N'/uploads/logos/fpt-software.png', N'Tập đoàn công nghệ hàng đầu Việt Nam.', 2, N'0101248141', N'https://fpt-software.com');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Address', N'CompanyName', N'CompanySize', N'LogoPath', N'Overview', N'RecruiterId', N'TaxCode', N'Website') AND [object_id] = OBJECT_ID(N'[Companies]'))
    SET IDENTITY_INSERT [Companies] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CandidateId', N'CreatedAt', N'FilePath', N'IsDefault', N'RawTextContent', N'Title', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Resumes]'))
    SET IDENTITY_INSERT [Resumes] ON;
INSERT INTO [Resumes] ([Id], [CandidateId], [CreatedAt], [FilePath], [IsDefault], [RawTextContent], [Title], [UpdatedAt])
VALUES (1, 4, '2026-07-20T14:01:29.7600108Z', N'/uploads/cv1.pdf', CAST(1 AS bit), N'Kinh nghiệm 3 năm làm việc với C#, chuyên sâu Web API, Entity Framework Core, SQL Server và Docker.', N'CV Lê Văn Pro - .NET Developer', NULL),
(2, 5, '2026-07-20T14:01:29.7600110Z', N'/uploads/cv2.pdf', CAST(1 AS bit), N'Sinh viên mới tốt nghiệp, biết cơ bản về C# và OOP, chưa có kinh nghiệm thực tế hệ thống lớn.', N'CV Nguyễn Thị Fresher', NULL),
(3, 6, '2026-07-20T14:01:29.7600112Z', N'/uploads/cv3.pdf', CAST(1 AS bit), N'Sinh viên năm 4 tìm kiếm vị trí thực tập, biết viết câu lệnh SQL cơ bản, đang học C#.', N'CV Trần Văn Intern Backend', NULL),
(4, 7, '2026-07-20T14:01:29.7600114Z', N'/uploads/cv4.pdf', CAST(1 AS bit), N'Kinh nghiệm 2 năm chạy quảng cáo Facebook, Google Ads, tư vấn chốt đơn hàng.', N'CV Hoàng Lệ - Sales Marketing', NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CandidateId', N'CreatedAt', N'FilePath', N'IsDefault', N'RawTextContent', N'Title', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Resumes]'))
    SET IDENTITY_INSERT [Resumes] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Benefits', N'CompanyId', N'CreatedAt', N'Description', N'ExpiredAt', N'JobCategoryId', N'JobType', N'Location', N'MaxSalary', N'MinSalary', N'Requirements', N'Status', N'Title', N'Vacancies', N'WorkMode') AND [object_id] = OBJECT_ID(N'[JobPostings]'))
    SET IDENTITY_INSERT [JobPostings] ON;
INSERT INTO [JobPostings] ([Id], [Benefits], [CompanyId], [CreatedAt], [Description], [ExpiredAt], [JobCategoryId], [JobType], [Location], [MaxSalary], [MinSalary], [Requirements], [Status], [Title], [Vacancies], [WorkMode])
VALUES (1, N'Lương thưởng tháng 13, bảo hiểm FPT Care, làm việc hybrid.', 1, '2026-07-20T14:01:29.7600073Z', N'Phát triển các hệ thống Web API quy mô lớn sử dụng .NET 8 và SQL Server.', '2026-08-19T14:01:29.7600073Z', 1, 1, N'Cầu Giấy, Hà Nội', 25000000, 15000000, N'Có kinh nghiệm lập trình C#, hiểu biết về Entity Framework Core, SQL Server. Biết Docker là một lợi thế.', 2, N'Kỹ Sư Lập Trình Backend .NET', 3, 3),
(2, N'Thưởng theo KPIs, môi trường trẻ trung năng động.', 1, '2026-07-20T14:01:29.7600073Z', N'Lên kế hoạch và triển khai các chiến dịch quảng cáo trên nền tảng Digital (Facebook, Google).', '2026-08-19T14:01:29.7600073Z', 3, 1, N'Thanh Xuân, Hà Nội', 20000000, 10000000, N'Tối thiểu 1 năm kinh nghiệm chạy Ads. Có khả năng sáng tạo nội dung.', 2, N'Chuyên viên Marketing Digital', 2, 1),
(3, N'Hoa hồng cao, phụ cấp ăn trưa và đi lại.', 1, '2026-07-20T14:01:29.7600073Z', N'Tư vấn sản phẩm dịch vụ của công ty đến với khách hàng qua điện thoại.', '2026-08-19T14:01:29.7600073Z', 2, 1, N'Đống Đa, Hà Nội', 15000000, 7000000, N'Giọng nói chuẩn, giao tiếp tốt, không yêu cầu kinh nghiệm (được đào tạo).', 2, N'Nhân viên Telesales', 5, 1),
(4, N'Hỗ trợ dấu mộc thực tập, trợ cấp 3 triệu/tháng, cơ hội lên nhân viên chính thức.', 1, '2026-07-20T14:01:29.7600073Z', N'Tham gia phát triển các tính năng Front-end cho dự án công ty bằng ReactJS.', '2026-08-19T14:01:29.7600073Z', 1, 3, N'Quận 1, TP. HCM', 5000000, 3000000, N'Sinh viên năm cuối hoặc mới ra trường, nắm chắc HTML/CSS/JS cơ bản.', 2, N'Thực tập sinh ReactJS', 4, 2),
(5, N'Chế độ BHYT, BHXH đầy đủ, thưởng lễ tết hấp dẫn.', 1, '2026-07-20T14:01:29.7600073Z', N'Chịu trách nhiệm kiểm tra đối chiếu số liệu, lập báo cáo tài chính hàng tháng/quý.', '2026-08-19T14:01:29.7600073Z', 4, 1, N'Hải Châu, Đà Nẵng', 18000000, 12000000, N'Tốt nghiệp đại học chuyên ngành Kế toán, trên 3 năm kinh nghiệm làm tổng hợp.', 2, N'Kế toán tổng hợp', 1, 1);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Benefits', N'CompanyId', N'CreatedAt', N'Description', N'ExpiredAt', N'JobCategoryId', N'JobType', N'Location', N'MaxSalary', N'MinSalary', N'Requirements', N'Status', N'Title', N'Vacancies', N'WorkMode') AND [object_id] = OBJECT_ID(N'[JobPostings]'))
    SET IDENTITY_INSERT [JobPostings] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AiFeedback', N'AiMatchScore', N'AppliedAt', N'CandidateId', N'CoverLetter', N'JobPostingId', N'ResumeId', N'Status') AND [object_id] = OBJECT_ID(N'[Application]'))
    SET IDENTITY_INSERT [Application] ON;
INSERT INTO [Application] ([Id], [AiFeedback], [AiMatchScore], [AppliedAt], [CandidateId], [CoverLetter], [JobPostingId], [ResumeId], [Status])
VALUES (1, N'Hồ sơ hoàn hảo. Ứng viên có đầy đủ kỹ năng cứng về C#, EF Core, SQL và Docker trùng khớp hoàn toàn với JD.', 95, '2026-07-20T14:01:29.7600347Z', 4, N'Tôi rất mong muốn được làm việc tại FPT.', 1, 1, 2),
(2, N'Ứng viên có kiến thức nền tảng C# nhưng thiếu kinh nghiệm thực tế với SQL và hệ thống lớn theo yêu cầu.', 65, '2026-07-20T14:01:29.7600350Z', 5, N'Mong công ty cho cơ hội phỏng vấn.', 1, 2, 2),
(3, N'Hồ sơ còn khá yếu, chưa đáp ứng được các tiêu chí kỹ thuật tối thiểu của vị trí hiện tại.', 40, '2026-07-20T14:01:29.7600352Z', 6, N'Xin thực tập ạ.', 1, 3, 2),
(4, N'Hồ sơ không phù hợp. Ứng viên làm mảng Marketing, hoàn toàn không có kỹ năng lập trình phần mềm.', 10, '2026-07-20T14:01:29.7600354Z', 7, N'Tìm kiếm cơ hội mới.', 1, 4, 2);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AiFeedback', N'AiMatchScore', N'AppliedAt', N'CandidateId', N'CoverLetter', N'JobPostingId', N'ResumeId', N'Status') AND [object_id] = OBJECT_ID(N'[Application]'))
    SET IDENTITY_INSERT [Application] OFF;
GO

CREATE INDEX [IX_Application_CandidateId] ON [Application] ([CandidateId]);
GO

CREATE INDEX [IX_Application_JobPostingId] ON [Application] ([JobPostingId]);
GO

CREATE INDEX [IX_Application_ResumeId] ON [Application] ([ResumeId]);
GO

CREATE UNIQUE INDEX [IX_Companies_RecruiterId] ON [Companies] ([RecruiterId]);
GO

CREATE UNIQUE INDEX [IX_Companies_TaxCode] ON [Companies] ([TaxCode]);
GO

CREATE INDEX [IX_JobPostings_CompanyId] ON [JobPostings] ([CompanyId]);
GO

CREATE INDEX [IX_JobPostings_JobCategoryId] ON [JobPostings] ([JobCategoryId]);
GO

CREATE INDEX [IX_RecruiterRequests_UserId] ON [RecruiterRequests] ([UserId]);
GO

CREATE INDEX [IX_Resumes_CandidateId] ON [Resumes] ([CandidateId]);
GO

CREATE INDEX [IX_RolePermissions_PermissionId] ON [RolePermissions] ([PermissionId]);
GO

CREATE INDEX [IX_Users_RoleId] ON [Users] ([RoleId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260720140130_InitialCreate', N'8.0.29');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

UPDATE [Application] SET [AppliedAt] = '2026-01-05T00:00:00.0000000Z'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;

GO

UPDATE [Application] SET [AppliedAt] = '2026-01-10T00:00:00.0000000Z'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;

GO

UPDATE [Application] SET [AppliedAt] = '2026-01-15T00:00:00.0000000Z'
WHERE [Id] = 3;
SELECT @@ROWCOUNT;

GO

UPDATE [Application] SET [AppliedAt] = '2026-01-20T00:00:00.0000000Z'
WHERE [Id] = 4;
SELECT @@ROWCOUNT;

GO

UPDATE [JobPostings] SET [CreatedAt] = '2026-01-01T00:00:00.0000000Z', [ExpiredAt] = '2026-02-01T00:00:00.0000000Z'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;

GO

UPDATE [Resumes] SET [CreatedAt] = '2026-01-01T00:00:00.0000000Z'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;

GO

UPDATE [Resumes] SET [CreatedAt] = '2026-01-01T00:00:00.0000000Z'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;

GO

UPDATE [Resumes] SET [CreatedAt] = '2026-01-01T00:00:00.0000000Z'
WHERE [Id] = 3;
SELECT @@ROWCOUNT;

GO

UPDATE [Resumes] SET [CreatedAt] = '2026-01-01T00:00:00.0000000Z'
WHERE [Id] = 4;
SELECT @@ROWCOUNT;

GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermissionId', N'RoleId') AND [object_id] = OBJECT_ID(N'[RolePermissions]'))
    SET IDENTITY_INSERT [RolePermissions] ON;
INSERT INTO [RolePermissions] ([PermissionId], [RoleId])
VALUES (8, 1),
(9, 1),
(1, 2),
(2, 2),
(3, 2),
(4, 2),
(6, 2),
(7, 2),
(1, 3),
(5, 3);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermissionId', N'RoleId') AND [object_id] = OBJECT_ID(N'[RolePermissions]'))
    SET IDENTITY_INSERT [RolePermissions] OFF;
GO

UPDATE [Users] SET [CreatedAt] = '2026-01-01T00:00:00.0000000Z'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;

GO

UPDATE [Users] SET [CreatedAt] = '2026-01-01T00:00:00.0000000Z'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;

GO

UPDATE [Users] SET [CreatedAt] = '2026-01-01T00:00:00.0000000Z'
WHERE [Id] = 3;
SELECT @@ROWCOUNT;

GO

UPDATE [Users] SET [CreatedAt] = '2026-01-01T00:00:00.0000000Z'
WHERE [Id] = 4;
SELECT @@ROWCOUNT;

GO

UPDATE [Users] SET [CreatedAt] = '2026-01-01T00:00:00.0000000Z'
WHERE [Id] = 5;
SELECT @@ROWCOUNT;

GO

UPDATE [Users] SET [CreatedAt] = '2026-01-01T00:00:00.0000000Z'
WHERE [Id] = 6;
SELECT @@ROWCOUNT;

GO

UPDATE [Users] SET [CreatedAt] = '2026-01-01T00:00:00.0000000Z'
WHERE [Id] = 7;
SELECT @@ROWCOUNT;

GO

CREATE UNIQUE INDEX [IX_Roles_Name] ON [Roles] ([Name]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260721100755_AddRolePermissionSeeds', N'8.0.29');
GO

COMMIT;
GO

