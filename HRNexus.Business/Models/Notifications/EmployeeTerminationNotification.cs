namespace HRNexus.Business.Models.Notifications;

public sealed record EmployeeTerminationNotification(
    int EmployeeId,
    string EmployeeCode,
    string EmployeeFullName,
    string TerminationReasonName,
    DateOnly TerminationDate,
    bool IsEligibleForRehire,
    int? TerminatedByUserId,
    string? TerminatedByUsername,
    DateTime OccurredAt);
