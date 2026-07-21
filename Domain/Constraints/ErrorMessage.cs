namespace Domain.Constraints;

public static class ErrorMessage
{
    // Error messages when registering a new user
    public const string DuplicateEmail = "Email này đã được sử dụng bởi tài khoản khác.";
    public const string CandidateRoleNotAvailable = "Hệ thống chưa cấu hình tài khoản ứng viên, vui lòng liên hệ Admin.";

    // Error messages when logging in
    public const string InvalidLogin = "Email hoặc mật khẩu không đúng.";
    public const string AccountLocked = "Tài khoản của bạn hiện đang bị khóa, vui lòng liên hệ Admin.";

    // Error messages for job postings (Recruiter)
    public const string CompanyProfileRequired = "Bạn cần tạo hồ sơ công ty trước khi đăng tin tuyển dụng.";
    public const string JobNotFound = "Không tìm thấy tin tuyển dụng hoặc bạn không có quyền truy cập.";
    public const string JobCategoryNotFound = "Ngành nghề được chọn không hợp lệ.";
    public const string JobSaveError = "Có lỗi xảy ra khi lưu tin tuyển dụng. Vui lòng thử lại.";



    public const string InvalidPassword = "Invalid password.";
    public const string UserNotFound = "User not found.";
    public const string UserAlreadyExists = "User already exists.";
    public const string InvalidCredentials = "Invalid credentials.";
    public const string UnauthorizedAccess = "Unauthorized access.";
    public const string InternalServerError = "Internal server error.";


    public const string ExceptionError = "Có lỗi xảy ra trong quá trình tạo tài khoản. Vui lòng thử lại.";

}
