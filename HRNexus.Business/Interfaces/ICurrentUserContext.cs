namespace HRNexus.Business.Interfaces;

public interface ICurrentUserContext
{
    int? UserId { get; }
    int? EmployeeId { get; }
    string? Username { get; }
    IReadOnlyList<string> Roles { get; }
}
