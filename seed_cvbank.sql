-- Dữ liệu test cho Kho CV (CvBankEntries)
-- Recruiter Id=2: recruiter1@fpt.com | Recruiter Id=3: recruiter2@viettel.com
-- Mật khẩu đăng nhập mặc định: Password123@
SET NOCOUNT ON;

DELETE FROM [CvBankEntries] WHERE [RecruiterId] IN (2, 3);

INSERT INTO [CvBankEntries]
    ([RecruiterId], [FileName], [StoredFileName], [Name], [Email], [Phone], [CurrentTitle],
     [TotalYearsExperience], [AiYearsExperience], [IsFresher], [Skills], [Summary], [RawText], [CreatedAt])
VALUES
-- ===== Recruiter 2 (FPT) =====
(2, N'nguyen_trong_quy.pdf', N'seed-quy.pdf', N'Nguyễn Trọng Quý', N'quynthe173486@fpt.edu.vn', N'0901234567', N'Intern Software Engineer',
 0, 0, 1, N'HTML, CSS, Bootstrap, JavaScript, Java, SQL Server, MySQL, teamwork',
 N'Ứng viên là sinh viên ngành Kỹ thuật phần mềm tại Đại học FPT, có kinh nghiệm thực tập và tham gia các dự án phát triển web. Có khả năng làm việc độc lập và trong nhóm.',
 N'Sinh viên FPT, thực tập phát triển web, HTML CSS JS Java SQL.', SYSUTCDATETIME()),

(2, N'do_ha_chieu_thu.pdf', N'seed-thu.pdf', N'Đỗ Hà Chiều Thu', N'thu@gmail.com', N'0902345678', N'Điều dưỡng viên',
 7.5, 0, 0, N'Tổ chức tiếp đón, Hướng dẫn người bệnh, Quản lý thời gian hiệu quả, Theo dõi tình hình bệnh nhân, Báo cáo tình hình bệnh nhân',
 N'Ứng viên có 7.5 năm kinh nghiệm trong lĩnh vực điều dưỡng. Từng làm việc tại các phòng khám và có khả năng tổ chức, quản lý thời gian hiệu quả.',
 N'Điều dưỡng 7.5 năm, chăm sóc bệnh nhân, phòng khám.', SYSUTCDATETIME()),

(2, N'pham_hoang_anh.pdf', N'seed-anh.pdf', N'Phạm Hoàng Anh', N'timviec@gmail.com', N'0903456789', N'Giám Đốc Quan Hệ Khách Hàng Doanh Nghiệp Lớn',
 6, 0, 0, N'Tổ chức và sắp xếp công việc, Làm việc nhóm và độc lập, Kỹ năng vượt trội trong thương lượng, Ms Word, Ms Excel, Ms PowerPoint, Ms Outlook',
 N'Ứng viên có 6 năm kinh nghiệm trong lĩnh vực tài chính và quản lý khách hàng. Hiện đang giữ vị trí Giám Đốc Quan Hệ Khách Hàng tại Techcombank.',
 N'6 năm tài chính, quản lý khách hàng doanh nghiệp, Techcombank.', SYSUTCDATETIME()),

(2, N'nguyen_thi_minh_thao.pdf', N'seed-thao.pdf', N'Nguyễn Thị Minh Thảo', N'timviec@gmail.com', N'0904567890', N'Nhân viên quản lý hàng tồn kho',
 2.2, 0, 0, N'Quản lý hàng tồn kho, Kiểm soát kho hàng, Quản lý dữ liệu, Chuẩn bị hàng hóa, Báo cáo số liệu, Kỹ năng làm việc nhóm, Tin học văn phòng',
 N'Nguyễn Thị Minh Thảo có kinh nghiệm 2 năm trong lĩnh vực Logistics và quản lý kho hàng.',
 N'2.2 năm logistics, quản lý kho, kiểm soát tồn kho.', SYSUTCDATETIME()),

(2, N'tran_van_data.pdf', N'seed-data.pdf', N'Trần Văn Data', N'tranvandata@gmail.com', N'0905678901', N'Data Scientist',
 4, 3.5, 0, N'Python, Pandas, NumPy, Scikit-learn, TensorFlow, SQL, Machine Learning, Data Visualization',
 N'Data Scientist với 4 năm kinh nghiệm, trong đó 3.5 năm chuyên sâu về Machine Learning và phân tích dữ liệu cho ngành thương mại điện tử.',
 N'4 năm data, 3.5 năm ML, Python TensorFlow.', SYSUTCDATETIME()),

(2, N'le_thi_ai.pdf', N'seed-mlai.pdf', N'Lê Thị AI', N'lethiai@gmail.com', N'0906789012', N'Machine Learning Engineer',
 5, 4, 0, N'Python, PyTorch, TensorFlow, Computer Vision, NLP, Docker, Kubernetes, MLOps',
 N'ML Engineer 5 năm kinh nghiệm, 4 năm làm việc trực tiếp với các mô hình Deep Learning về thị giác máy tính và xử lý ngôn ngữ tự nhiên.',
 N'5 năm, 4 năm AI/ML, deep learning, CV, NLP.', SYSUTCDATETIME()),

(2, N'hoang_minh_backend.pdf', N'seed-backend.pdf', N'Hoàng Minh Backend', N'hoangminh@gmail.com', N'0907890123', N'.NET Backend Developer',
 3, 0, 0, N'C#, ASP.NET Core, Entity Framework Core, SQL Server, Docker, Redis, RabbitMQ',
 N'Lập trình viên Backend .NET 3 năm kinh nghiệm, xây dựng Web API quy mô lớn, tối ưu hiệu năng hệ thống.',
 N'3 năm .NET backend, Web API, EF Core, SQL Server.', SYSUTCDATETIME()),

(2, N'vu_fresher_frontend.pdf', N'seed-fefresher.pdf', N'Vũ Thị Fresher', N'vufresher@gmail.com', N'0908901234', N'Fresher Frontend Developer',
 0, 0, 1, N'HTML, CSS, JavaScript, React, Git, Figma',
 N'Sinh viên mới tốt nghiệp CNTT, có kiến thức cơ bản về React và làm việc nhóm qua các dự án học tập.',
 N'Fresher frontend, React cơ bản, mới tốt nghiệp.', SYSUTCDATETIME()),

(2, N'dang_van_devops.pdf', N'seed-devops.pdf', N'Đặng Văn DevOps', N'dangvandevops@gmail.com', N'0909012345', N'DevOps Engineer',
 4.5, 1, 0, N'Docker, Kubernetes, Jenkins, Terraform, AWS, Azure, CI/CD, Linux',
 N'DevOps Engineer 4.5 năm kinh nghiệm triển khai hạ tầng cloud, tự động hoá CI/CD, có tiếp xúc MLOps.',
 N'4.5 năm devops, cloud, CI/CD, có 1 năm MLOps.', SYSUTCDATETIME()),

(2, N'bui_thi_ba_intern.pdf', N'seed-baintern.pdf', N'Bùi Thị Ba', N'buithiba@gmail.com', N'0900123456', N'Intern Data Analyst',
 0.5, 0.5, 1, N'Excel, SQL, Power BI, Python cơ bản',
 N'Thực tập sinh phân tích dữ liệu, có nửa năm kinh nghiệm intern với báo cáo Power BI và truy vấn SQL.',
 N'Intern data analyst, 0.5 năm, Power BI, SQL.', SYSUTCDATETIME()),

-- ===== Recruiter 3 (Viettel) - để kiểm tra cách ly theo owner =====
(3, N'ung_vien_viettel_1.pdf', N'seed-vt1.pdf', N'Phan Văn Viettel', N'phanvan@viettel.com', N'0911111111', N'Network Engineer',
 3, 0, 0, N'Networking, Cisco, Linux, Python',
 N'Kỹ sư mạng 3 năm kinh nghiệm tại nhà mạng lớn.',
 N'3 năm network engineer.', SYSUTCDATETIME()),

(3, N'ung_vien_viettel_2.pdf', N'seed-vt2.pdf', N'Ngô Thị Viettel', N'ngothi@viettel.com', N'0922222222', N'Fresher Tester',
 0, 0, 1, N'Manual Testing, SQL, Jira',
 N'Fresher kiểm thử phần mềm, mới tốt nghiệp.',
 N'Fresher tester.', SYSUTCDATETIME());

SELECT [RecruiterId], COUNT(*) AS SoLuongCV
FROM [CvBankEntries]
GROUP BY [RecruiterId];
