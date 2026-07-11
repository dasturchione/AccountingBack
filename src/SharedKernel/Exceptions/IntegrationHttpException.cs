namespace SharedKernel.Exceptions;

public class IntegrationHttpException : Exception
{
    public int StatusCode { get; }

    public IntegrationHttpException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
