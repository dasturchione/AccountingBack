namespace SharedKernel.Exceptions;

public class OptimisticConcurrencyException : Exception
{
    public OptimisticConcurrencyException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
