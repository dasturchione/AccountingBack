using System.Text.Json.Serialization;

namespace Integration.Faktura.Dtos;

public sealed class FakturaImportDocumentRequestDto
{
    [JsonPropertyName("invoices")]
    public IReadOnlyCollection<FakturaInvoiceDto> Invoices { get; init; } = [];
}

public sealed class FakturaInvoiceDto
{
    [JsonPropertyName("head")]
    public FakturaHeadDto Head { get; init; } = new();

    [JsonPropertyName("document")]
    public FakturaDocumentDto Document { get; init; } = new();
}

public sealed class FakturaHeadDto
{
    [JsonPropertyName("sender")]
    public FakturaPartyEnvelopeDto Sender { get; init; } = new();

    [JsonPropertyName("receiver")]
    public FakturaPartyEnvelopeDto Receiver { get; init; } = new();
}

public sealed class FakturaPartyEnvelopeDto
{
    [JsonPropertyName("sender_info")]
    public FakturaPartyInfoDto? SenderInfo { get; init; }

    [JsonPropertyName("receiver_info")]
    public FakturaPartyInfoDto? ReceiverInfo { get; init; }

    [JsonPropertyName("approver_and_signers")]
    public FakturaApproverAndSignersDto? ApproverAndSigners { get; init; }
}

public sealed class FakturaPartyInfoDto
{
    [JsonPropertyName("INN")]
    public string INN { get; init; } = string.Empty;

    [JsonPropertyName("company_name")]
    public string CompanyName { get; init; } = string.Empty;

    [JsonPropertyName("address")]
    public FakturaAddressDto? Address { get; init; }

    [JsonPropertyName("bank_details")]
    public FakturaBankDetailsDto? BankDetails { get; init; }
}

public sealed class FakturaApproverAndSignersDto
{
    [JsonPropertyName("approver")]
    public FakturaEmployeeDto? Approver { get; init; }

    [JsonPropertyName("signer_accountant")]
    public FakturaEmployeeDto? SignerAccountant { get; init; }
}

public sealed class FakturaEmployeeDto
{
    [JsonPropertyName("last_name")]
    public string? LastName { get; init; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; init; }

    [JsonPropertyName("middle_name")]
    public string? MiddleName { get; init; }
}

public sealed class FakturaAddressDto
{
    [JsonPropertyName("street")]
    public string? Street { get; init; }
}

public sealed class FakturaBankDetailsDto
{
    [JsonPropertyName("account_number")]
    public string? AccountNumber { get; init; }

    [JsonPropertyName("bank_name")]
    public string? BankName { get; init; }

    [JsonPropertyName("bank_code")]
    public string? BankCode { get; init; }
}

public sealed class FakturaDocumentDto
{
    [JsonPropertyName("document_number")]
    public string DocumentNumber { get; init; } = string.Empty;

    [JsonPropertyName("document_date")]
    public string DocumentDate { get; init; } = string.Empty;

    [JsonPropertyName("contract_number")]
    public string? ContractNumber { get; init; }

    [JsonPropertyName("contract_date")]
    public string? ContractDate { get; init; }

    [JsonPropertyName("items")]
    public IReadOnlyCollection<FakturaItemDto> Items { get; init; } = [];

    [JsonPropertyName("column_summary_values")]
    public FakturaColumnSummaryValuesDto ColumnSummaryValues { get; init; } = new();
}

public sealed class FakturaItemDto
{
    [JsonPropertyName("item_number")]
    public string ItemNumber { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("volume")]
    public int Volume { get; init; }

    [JsonPropertyName("unit_price")]
    public decimal UnitPrice { get; init; }

    [JsonPropertyName("subtotal")]
    public decimal Subtotal { get; init; }

    [JsonPropertyName("vat")]
    public FakturaVatDto? Vat { get; init; }

    [JsonPropertyName("subtotal_with_taxes")]
    public decimal SubtotalWithTaxes { get; init; }

    [JsonPropertyName("measurement_unit")]
    public string MeasurementUnit { get; init; } = string.Empty;
}

public sealed class FakturaVatDto
{
    [JsonPropertyName("vat_rate")]
    public string VatRate { get; init; } = string.Empty;

    [JsonPropertyName("vat_value")]
    public decimal VatValue { get; init; }
}

public sealed class FakturaColumnSummaryValuesDto
{
    [JsonPropertyName("column_subtotal")]
    public decimal ColumnSubtotal { get; init; }

    [JsonPropertyName("column_vat_value")]
    public decimal ColumnVatValue { get; init; }

    [JsonPropertyName("column_subtotal_with_taxes")]
    public decimal ColumnSubtotalWithTaxes { get; init; }
}

public sealed class FakturaImportDocumentResponseDto
{
    [JsonPropertyName("SuccessCount")]
    public int SuccessCount { get; init; }

    [JsonPropertyName("ErrorCount")]
    public int ErrorCount { get; init; }

    [JsonPropertyName("SuccessItems")]
    public IReadOnlyCollection<FakturaImportItemResultDto>? SuccessItems { get; init; }

    [JsonPropertyName("ErrorItems")]
    public IReadOnlyCollection<FakturaImportItemResultDto>? ErrorItems { get; init; }
}

public sealed class FakturaImportItemResultDto
{
    [JsonPropertyName("Name")]
    public string? Name { get; init; }

    [JsonPropertyName("UniqueId")]
    public string? UniqueId { get; init; }

    [JsonPropertyName("Message")]
    public string? Message { get; init; }

    [JsonPropertyName("Id")]
    public string? Id { get; init; }
}

public sealed class FakturaSignDocumentRequestDto
{
    [JsonPropertyName("UniqueId")]
    public string UniqueId { get; init; } = string.Empty;

    [JsonPropertyName("SignedContent")]
    public string SignedContent { get; init; } = string.Empty;

    [JsonPropertyName("CertificateSerial")]
    public string CertificateSerial { get; init; } = string.Empty;
}

public sealed class FakturaRejectDocumentRequestDto
{
    [JsonPropertyName("DocumentUniqueId")]
    public string DocumentUniqueId { get; init; } = string.Empty;

    [JsonPropertyName("CompanyInn")]
    public string CompanyInn { get; init; } = string.Empty;
}

public sealed class FakturaDocumentStatusRequestDto
{
    [JsonPropertyName("DocumentUniqueIds")]
    public IReadOnlyCollection<string> DocumentUniqueIds { get; init; } = [];
}
