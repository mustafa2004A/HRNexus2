namespace HRNexus.Business.Security;

public static class SecurityActivityCodes
{
    public const string LoginSuccess = "LOGIN_SUCCESS";
    public const string LoginFailed = "LOGIN_FAILED";
    public const string Logout = "LOGOUT";
    public const string TokenRefresh = "TOKEN_REFRESH";
    public const string AccessDenied = "ACCESS_DENIED";
    public const string AccountLocked = "ACCOUNT_LOCKED";
    public const string TerminationVerificationCodeRequested = "TERM_VERIF_REQ";
    public const string TerminationVerificationCodeSendFailed = "TERM_VERIF_SEND_FAIL";
    public const string TerminationVerificationFailed = "TERM_VERIF_FAILED";
    public const string TerminationVerificationSucceeded = "TERM_VERIF_OK";
    public const string EmployeeTerminated = "EMPLOYEE_TERMINATED";
}
