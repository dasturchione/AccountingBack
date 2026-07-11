namespace SharedKernel.Exceptions;

public class IntegrationUnauthorizedException : IntegrationHttpException
{
    public IntegrationUnauthorizedException(string message)
        : base(message, 401)
    {
    }
}
