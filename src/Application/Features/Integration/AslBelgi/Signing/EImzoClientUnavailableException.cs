namespace Application.Features.Integration.AslBelgi.Signing;

public sealed class EImzoClientUnavailableException : InvalidOperationException
{
    public EImzoClientUnavailableException()
        : base("Client-side E-IMZO integration is unavailable. CRPT support'dan aniqlanishi kerak.")
    {
    }
}
