using Services.DTOs.Application;

namespace Services.Interfaces;

public interface IApplicationService
{
    Task<bool> ApplyJobAsync(int candidateId, int jobId, string cvFilePath, string cvFileName, string? coverLetter);
    Task<List<ApplicationDto>> GetMyApplicationsAsync(int candidateId);
    Task<bool> WithdrawApplicationAsync(int candidateId, int applicationId, string reason);
}
