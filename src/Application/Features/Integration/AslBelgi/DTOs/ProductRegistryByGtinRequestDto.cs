namespace Application.Features.Integration.AslBelgi.DTOs;

public sealed class ProductRegistryByGtinRequestDto
{
    public string ProductGroup { get; init; } = string.Empty;
    public string Gtin { get; init; } = string.Empty;
}
