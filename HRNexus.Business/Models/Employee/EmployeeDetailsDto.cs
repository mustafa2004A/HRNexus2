namespace HRNexus.Business.Models.Employee;

public sealed record EmployeeDetailsDto(
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
