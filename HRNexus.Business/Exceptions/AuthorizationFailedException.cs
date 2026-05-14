namespace HRNexus.Business.Exceptions;

public sealed class AuthorizationFailedException : Exception
{
    public AuthorizationFailedException(string message)
        : base(message)
    {
    }
}
