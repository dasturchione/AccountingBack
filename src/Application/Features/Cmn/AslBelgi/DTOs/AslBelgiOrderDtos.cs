using System.Text.Json.Serialization;

namespace Application.Features.Cmn.AslBelgi.DTOs;

// ---------------------------------------------------------------------------------------------
// Asl Belgisi KM (marking code) order DTOs — Open API v1.18.2, section 4.
// JSON keys are camelCase exactly as documented (productGroup, gtin, cisType, ...).
//
// Справочник values (productGroup, releaseMethodType, cisType, serialNumberType) and gtin are NOT
// stored in our domain — they are supplied by the caller (⏳ waiting points, see A2 report).
// ---------------------------------------------------------------------------------------------

/// <summary>POST /api/orders — register a KM emission order.</summary>
public sealed class AslBelgiOrderRequest
{
    [JsonPropertyName("productGroup")]
    public string ProductGroup { get; init; } = null!;

    [JsonPropertyName("businessPlaceId")]
    public int BusinessPlaceId { get; init; }

    [JsonPropertyName("releaseMethodType")]
    public string ReleaseMethodType { get; init; } = null!;

    [JsonPropertyName("isPaid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsPaid { get; init; }

    [JsonPropertyName("poNumber")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PoNumber { get; init; }

    [JsonPropertyName("products")]
    public IReadOnlyList<AslBelgiOrderProduct> Products { get; init; } = [];

    [JsonPropertyName("contractorInfo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AslBelgiContractorInfo? ContractorInfo { get; init; }
}

public sealed class AslBelgiOrderProduct
{
    [JsonPropertyName("gtin")]
    public string Gtin { get; init; } = null!;

    [JsonPropertyName("quantity")]
    public int Quantity { get; init; }

    [JsonPropertyName("cisType")]
    public string CisType { get; init; } = null!;

    [JsonPropertyName("serialNumberType")]
    public string SerialNumberType { get; init; } = null!;

    /// <summary>Only sent when serialNumberType = SELF_MADE.</summary>
    [JsonPropertyName("serialNumbers")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? SerialNumbers { get; init; }
}

public sealed class AslBelgiContractorInfo
{
    [JsonPropertyName("contractorTin")]
    public string? ContractorTin { get; init; }

    [JsonPropertyName("contractorCountryCode")]
    public string? ContractorCountryCode { get; init; }
}

public sealed class AslBelgiOrderResponse
{
    [JsonPropertyName("orderId")]
    public string? OrderId { get; init; }
}

/// <summary>GET /api/codes — codes pulled from a sub-order.</summary>
public sealed class AslBelgiCodesResponse
{
    [JsonPropertyName("packId")]
    public string? PackId { get; init; }

    [JsonPropertyName("codes")]
    public IReadOnlyList<string> Codes { get; init; } = [];
}

/// <summary>GET /api/orders query filter.</summary>
public sealed class AslBelgiOrdersFilter
{
    public string? OrderId { get; set; }
    public string? Status { get; set; }
    public string? ProductGroup { get; set; }
    public DateTimeOffset? DateFrom { get; set; }
    public DateTimeOffset? DateTo { get; set; }
    public int? Limit { get; set; }
}

// TODO(Asl Belgisi doc): the full order-list item schema is not specified in the available facts;
// these are the documented/filterable fields. Confirm and extend against the real response.
public sealed class AslBelgiOrderInfo
{
    [JsonPropertyName("orderId")]
    public string? OrderId { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("productGroup")]
    public string? ProductGroup { get; init; }

    [JsonPropertyName("businessPlaceId")]
    public int? BusinessPlaceId { get; init; }
}
