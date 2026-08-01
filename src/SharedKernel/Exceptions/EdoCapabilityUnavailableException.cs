namespace SharedKernel.Exceptions;

public sealed class EdoCapabilityUnavailableException : Exception
{
    public EdoCapabilityUnavailableException(
        string providerCode,
        string capability,
        string status)
        : base($"EDO capability '{capability}' for provider '{providerCode}' is '{status}'.")
    {
        ProviderCode = providerCode;
        Capability = capability;
        Status = status;
    }

    public string ProviderCode { get; }
    public string Capability { get; }
    public string Status { get; }
}
