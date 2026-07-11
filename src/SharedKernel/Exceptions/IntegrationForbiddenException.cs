namespace SharedKernel.Exceptions;

public class IntegrationForbiddenException : IntegrationHttpException
{
    public IntegrationForbiddenException(string message)
        : base(message, 403)
    {
    }
}
