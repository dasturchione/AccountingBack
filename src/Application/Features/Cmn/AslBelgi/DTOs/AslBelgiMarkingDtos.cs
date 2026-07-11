namespace Application.Features.Cmn.AslBelgi.DTOs;

/// <summary>
/// Request a KM emission order for a domain product. GTIN is resolved from Product.Gtin unless an
/// explicit override is supplied. Справочник values (productGroup, cisType, ...) are caller-supplied.
/// </summary>
public sealed class AslBelgiMarkingRequestDto
{
    public int ProductId { get; init; }
    public int Quantity { get; init; }
    public string ProductGroup { get; init; } = null!;
    public string ReleaseMethodType { get; init; } = null!;
    public string CisType { get; init; } = null!;
    public string SerialNumberType { get; init; } = null!;
    public int BusinessPlaceId { get; init; }
    public string? Gtin { get; init; }
    public string? PoNumber { get; init; }
}

/// <summary>Pull KM codes from a sub-order and bind them to ProductTable rows.</summary>
public sealed class AslBelgiBindCodesRequestDto
{
    public string OrderId { get; init; } = null!;
    public int ProductId { get; init; }
    public string? Gtin { get; init; }
    public int? Quantity { get; init; }
    public string? LastPackId { get; init; }
}

public sealed class AslBelgiBindResultDto
{
    public string OrderId { get; init; } = null!;
    public string? PackId { get; init; }
    public int TotalCodes { get; init; }
    public int BoundCount { get; init; }
    public int SkippedExisting { get; init; }
}
