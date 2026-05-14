namespace HRNexus.Business.Models.Leave;

public sealed record RequestStatusDto(
    int RequestStatusId,
    string StatusName,
    string StatusCode,
    string? Description,
    bool IsFinalState,
    bool IsActive);
