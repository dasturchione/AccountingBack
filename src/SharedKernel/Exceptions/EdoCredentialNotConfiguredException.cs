namespace SharedKernel.Exceptions;

public sealed class EdoCredentialNotConfiguredException : InvalidOperationException
{
    public EdoCredentialNotConfiguredException(string providerCode)
        : base($"Organization-scoped credentials are not configured for EDO provider '{providerCode}'.")
    {
        ProviderCode = providerCode;
    }

    public string ProviderCode { get; }
}
