namespace SharedKernel.Exceptions;

public sealed class EdoProviderNotFoundException : Exception
{
    public EdoProviderNotFoundException(string providerCode)
        : base($"EDO provider '{providerCode}' is not registered.")
    {
        ProviderCode = providerCode;
    }

    public string ProviderCode { get; }
}
