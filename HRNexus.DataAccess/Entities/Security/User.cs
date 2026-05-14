using HRNexus.DataAccess.Entities.Employee;
using HRNexus.DataAccess.Entities.Leave;
using HRNexus.DataAccess.Entities.Core;
using EmployeeEntity = HRNexus.DataAccess.Entities.Employee.Employee;

namespace HRNexus.DataAccess.Entities.Security;

public sealed class User
{
    public int UserId { get; set; }
    public int? EmployeeId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public int AccountStatusId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public EmployeeEntity? Employee { get; set; }
    public AccountStatus AccountStatus { get; set; } = null!;
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserRole> AssignedUserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<UserActivityLog> ActivityLogs { get; set; } = new List<UserActivityLog>();
    public ICollection<LeaveRequest> ReviewedLeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<LeaveAttachment> UploadedLeaveAttachments { get; set; } = new List<LeaveAttachment>();
    public ICollection<EmployeeDocument> VerifiedEmployeeDocuments { get; set; } = new List<EmployeeDocument>();
    public ICollection<EmployeeDocument> UploadedEmployeeDocuments { get; set; } = new List<EmployeeDocument>();
    public ICollection<FileStorageItem> UploadedFileStorageItems { get; set; } = new List<FileStorageItem>();
}
