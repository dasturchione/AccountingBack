namespace SharedKernel.Exceptions;

public class UniqueConstraintViolationException : Exception
{
    public UniqueConstraintViolationException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
