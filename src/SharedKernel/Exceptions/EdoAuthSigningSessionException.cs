namespace SharedKernel.Exceptions;

public sealed class EdoAuthSigningSessionException : InvalidOperationException
{
    public EdoAuthSigningSessionException()
        : base("The EDO authentication signing session is missing, expired, scoped differently, or already used.")
    {
    }

    public EdoAuthSigningSessionException(Exception innerException)
        : base("The EDO authentication signing session is missing, expired, scoped differently, or already used.", innerException)
    {
    }
}
