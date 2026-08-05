namespace Application.Abstractions.Integration.Faktura;

public sealed class FakturaAuthCompleteRequestDto
{
    public string? PreparedPkcs7 { get; init; }

    public bool RememberMe { get; init; }
}
