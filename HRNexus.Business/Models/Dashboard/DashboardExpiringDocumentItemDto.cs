namespace HRNexus.Business.Models.Dashboard;

public sealed record DashboardExpiringDocumentItemDto(
    int DocumentId,
    int EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    string DocumentName,
    string DocumentTypeName,
    DateOnly ExpiryDate);
