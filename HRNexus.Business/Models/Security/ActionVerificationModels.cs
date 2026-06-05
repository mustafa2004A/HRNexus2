namespace HRNexus.Business.Models.Security;

public sealed record ActionVerificationCodeIssueRequest(
    string ActionType,
    string TargetEntityType,
    int TargetEntityId,
    int RequestedByUserId,
    string? CreatedByIp);

public sealed record ActionVerificationCodeIssueResult(
    Guid VerificationRequestId,
    string DeliveryMethod,
    string? DestinationMasked,
    DateTime ExpiresAt,
    string Message);

public sealed record ActionVerificationCodeVerifyRequest(
    Guid VerificationRequestId,
    string VerificationCode,
    string ActionType,
    string TargetEntityType,
    int TargetEntityId,
    int RequestedByUserId,
    string? ClientIpAddress);

public sealed record VerificationCodeDeliveryRequest(
    string ActionType,
    string DeliveryMethod,
    string Destination,
    string DestinationMasked,
    string Code,
    DateTime ExpiresAt);
