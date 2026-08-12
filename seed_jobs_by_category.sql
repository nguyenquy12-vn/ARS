SET NOCOUNT ON;

DECLARE @CompanyId int = (SELECT TOP 1 Id FROM Companies ORDER BY Id);
IF @CompanyId IS NULL
    THROW 50001, N'Không có công ty để gán tin tuyển dụng.', 1;

;WITH JobTemplates AS
(
    SELECT c.Id AS CategoryId, v.SortOrder,
        CASE c.Id
            WHEN 1 THEN CASE v.SortOrder WHEN 1 THEN N'Lập trình viên Full-stack' ELSE N'Kỹ sư DevOps' END
            WHEN 2 THEN CASE v.SortOrder WHEN 1 THEN N'Chuyên viên Kinh doanh B2B' ELSE N'Nhân viên Chăm sóc khách hàng' END
            WHEN 3 THEN CASE v.SortOrder WHEN 1 THEN N'Content Marketing Executive' ELSE N'Chuyên viên SEO' END
            WHEN 4 THEN CASE v.SortOrder WHEN 1 THEN N'Chuyên viên Phân tích tài chính' ELSE N'Kế toán viên' END
            WHEN 5 THEN CASE v.SortOrder WHEN 1 THEN N'Chuyên viên Tuyển dụng' ELSE N'Nhân viên Hành chính Nhân sự' END
            WHEN 6 THEN CASE v.SortOrder WHEN 1 THEN N'UI/UX Designer' ELSE N'Graphic Designer' END
            WHEN 7 THEN CASE v.SortOrder WHEN 1 THEN N'Biên dịch viên Tiếng Anh' ELSE N'Phiên dịch viên Tiếng Nhật' END
            ELSE CASE v.SortOrder WHEN 1 THEN N'Chuyên viên ' + c.Name ELSE N'Nhân viên ' + c.Name END
        END AS Title,
        CASE v.SortOrder WHEN 1 THEN 12000000 ELSE 15000000 END AS MinSalary,
        CASE v.SortOrder WHEN 1 THEN 22000000 ELSE 28000000 END AS MaxSalary
    FROM JobCategories c
    CROSS JOIN (VALUES (1), (2)) v(SortOrder)
)
INSERT INTO JobPostings
    (CompanyId, Title, Description, Requirements, Benefits, Location, JobType, WorkMode,
     JobCategoryId, MinSalary, MaxSalary, Status, Vacancies, CreatedAt, ExpiredAt)
SELECT @CompanyId, t.Title,
       N'Tham gia triển khai công việc chuyên môn, phối hợp cùng đội ngũ và đảm bảo chất lượng đầu ra theo mục tiêu của doanh nghiệp.',
       N'Có kiến thức chuyên môn phù hợp, tinh thần chủ động, kỹ năng làm việc nhóm và trách nhiệm với công việc.',
       N'Lương cạnh tranh, thưởng hiệu suất, bảo hiểm đầy đủ, đào tạo chuyên môn và lộ trình phát triển rõ ràng.',
       CASE t.SortOrder WHEN 1 THEN N'Hà Nội' ELSE N'TP. Hồ Chí Minh' END,
       1, CASE t.SortOrder WHEN 1 THEN 3 ELSE 1 END,
       t.CategoryId, t.MinSalary, t.MaxSalary, 2,
       CASE t.SortOrder WHEN 1 THEN 2 ELSE 3 END,
       DATEADD(minute, -(t.CategoryId * 10 + t.SortOrder), SYSUTCDATETIME()),
       DATEADD(month, 3, SYSUTCDATETIME())
FROM JobTemplates t
WHERE NOT EXISTS
(
    SELECT 1 FROM JobPostings j
    WHERE j.JobCategoryId = t.CategoryId AND j.Title = t.Title
);

SELECT @@ROWCOUNT AS InsertedJobs;
