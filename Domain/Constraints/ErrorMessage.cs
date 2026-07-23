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

    // Error messages for company profile (Recruiter)
    public const string CompanyNotFound = "Không tìm thấy hồ sơ công ty.";
    public const string CompanySaveError = "Có lỗi xảy ra khi lưu hồ sơ công ty. Vui lòng thử lại.";
    public const string DuplicateTaxCode = "Mã số thuế này đã được đăng ký bởi một công ty khác.";

    // Error messages for applications (Recruiter)
    public const string ApplicationNotFound = "Không tìm thấy hồ sơ ứng tuyển hoặc bạn không có quyền truy cập.";
    public const string ApplicationSaveError = "Có lỗi xảy ra khi cập nhật hồ sơ ứng tuyển. Vui lòng thử lại.";

    // Error messages for AI CV evaluation (Gemini)
    public const string CvContentMissing = "Hồ sơ ứng viên chưa có nội dung CV để chấm điểm.";
    public const string AiNotConfigured = "Chưa cấu hình Gemini API Key. Vui lòng đặt 'Gemini:ApiKey' trong User Secrets.";
    public const string AiEvaluationError = "Có lỗi xảy ra khi chấm điểm CV bằng AI. Vui lòng thử lại.";
    public const string AiQuotaExceeded = "Gemini đã hết hạn mức (429 Too Many Requests). Vui lòng đợi ít phút rồi thử lại, hoặc dùng API key khác.";
    public const string AiTimeout = "AI phản hồi quá lâu (timeout). Vui lòng thử lại sau.";



    public const string InvalidPassword = "Invalid password.";
    public const string UserNotFound = "User not found.";
    public const string UserAlreadyExists = "User already exists.";
    public const string InvalidCredentials = "Invalid credentials.";
    public const string UnauthorizedAccess = "Unauthorized access.";
    public const string InternalServerError = "Internal server error.";


    public const string ExceptionError = "Có lỗi xảy ra trong quá trình tạo tài khoản. Vui lòng thử lại.";

}
