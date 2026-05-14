namespace HRNexus.Business.Security;

public static class AuthorizationPolicyNames
{
    public const string AuthenticatedUser = "AuthenticatedUser";
    public const string HrOrAdmin = "HrOrAdmin";
    public const string SecurityAdmin = "SecurityAdmin";
    public const string CanReviewLeave = "CanReviewLeave";
    public const string SelfOrHr = "SelfOrHr";
}
