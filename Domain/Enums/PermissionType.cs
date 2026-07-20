namespace Domain.Enums;

public enum PermissionType
{
    // Recruiter
    ViewJob = 1,
    CreateJob = 2,
    EditJob = 3,
    DeleteJob = 4,

    // Candidate
    ApplyJob = 5,
    ReviewCV = 6,
    EvaluateAI = 7, 

    // Admin
    ManageRoles = 8,
    ManageUsers = 9
}
