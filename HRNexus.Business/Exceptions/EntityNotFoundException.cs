namespace HRNexus.Business.Exceptions;

public sealed class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message)
        : base(message)
    {
    }
}
