namespace HRNexus.DataAccess.Repositories.Employee;

public sealed record EmployeeDetailsQueryResult(
    int EmployeeId,
    string EmployeeCode,
    int PersonId,
    string FullName,
    DateOnly HireDate,
    string EmploymentStatusName,
    DateOnly? TerminationDate,
    bool IsEligibleForRehire,
    string? PhotoUrl,
    int? PhotoFileStorageItemId);

public sealed record EmployeeCurrentContextQueryResult(
    int EmployeeId,
    string EmployeeCode,
    string FullName,
    string? DepartmentName,
    string? PositionName,
    string? EmploymentTypeName,
    string EmploymentStatusName,
    int? ManagerId,
    string? ManagerName,
    DateOnly? CurrentAssignmentStartDate);

public sealed record EmployeeJobHistoryItemQueryResult(
    int JobHistoryId,
    string DepartmentName,
    string PositionName,
    string EmploymentTypeName,
    string EmploymentStatusName,
    string? ManagerName,
    bool IsCurrent,
    DateOnly StartDate,
    DateOnly? EndDate);

public sealed record EmployeeDocumentItemQueryResult(
    int DocumentId,
    string DocumentName,
    string DocumentTypeName,
    string? ReferenceNumber,
    string FileExtension,
    int? FileStorageItemId,
    DateOnly? IssueDate,
    DateOnly? ExpiryDate,
    bool IsVerified,
    string? VerifiedByUsername,
    DateTime? VerifiedDate,
    bool IsActive,
    DateTime UploadedDate);

public sealed record EmployeeSummaryQueryResult(
    int EmployeeId,
    int PersonId,
    string EmployeeCode,
    string FullName,
    DateOnly HireDate,
    int CurrentEmploymentStatusId,
    string EmploymentStatusName,
    bool IsDeleted);

public sealed record EmployeeJobHistoryQueryResult(
    int JobHistoryId,
    int EmployeeId,
    int DepartmentId,
    string DepartmentName,
    int PositionId,
    string PositionName,
    int EmploymentTypeId,
    string EmploymentTypeName,
    int JobGradeId,
    string JobGradeName,
    int EmploymentStatusId,
    string EmploymentStatusName,
    int? ManagerId,
    string? ManagerName,
    bool IsCurrent,
    DateOnly StartDate,
    DateOnly? EndDate);

public sealed record EmployeeFamilyMemberQueryResult(
    int FamilyMemberId,
    int EmployeeId,
    int PersonId,
    string FirstName,
    string? SecondName,
    string? ThirdName,
    string LastName,
    string FullName,
    string? PreferredName,
    DateOnly? DateOfBirth,
    int? GenderId,
    int? MaritalStatusId,
    int? NationalityCountryId,
    string? PhotoUrl,
    bool IsDeleted,
    int RelationshipTypeId,
    string RelationshipTypeName);

public sealed record EmployeeDocumentQueryResult(
    int DocumentId,
    int EmployeeId,
    int DocumentTypeId,
    string DocumentTypeName,
    string DocumentName,
    string? ReferenceNumber,
    string FilePath,
    string FileExtension,
    int? FileStorageItemId,
    DateOnly? IssueDate,
    DateOnly? ExpiryDate,
    bool IsVerified,
    int? VerifiedBy,
    string? VerifiedByUsername,
    DateTime? VerifiedDate,
    bool IsActive,
    int? UploadedBy,
    string? UploadedByUsername,
    DateTime UploadedDate,
    string? Remarks);
