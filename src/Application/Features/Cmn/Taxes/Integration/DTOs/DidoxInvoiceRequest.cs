using System.Text.Json.Serialization;

namespace Application.Features.Cmn.Taxes.Integration.DTOs;

// ---------------------------------------------------------------------------------------------
// Didox ЭСФ (счёт-фактура) payload — official structure (07 JSON, Счёт-фактура, Version 1).
//
// Key points of the real schema (differ from the earlier flat draft):
//   * ProductList is an OBJECT (HasVat/Tin/Products), not a bare array.
//   * Money/quantity fields are STRINGS (Count, Summa, DeliverySum, VatRate, VatSum, ...).
//   * MeasureId is НЕ ИСПОЛЬЗУЕТСЯ — always null.
//   * SellerTin/BuyerTin are duplicated at top level alongside the Seller/Buyer objects.
//
// Every property carries an explicit [JsonPropertyName] so the exact doc casing survives the
// provider's Web (camelCase) serializer — a wrong key is rejected by Didox.
//
// TODO(domain) markers flag fields with no source in our domain model; see the mapper and the
// D3 report for the list an accountant must fill in. These are NOT guessed here.
// ---------------------------------------------------------------------------------------------

/// <summary>FacturaType справочник (0=Стандартный .. 4=Исправленный).</summary>
public enum DidoxFacturaType
{
    Standard = 0,
    Additional = 1,
    Refund = 2,
    WithoutPayment = 3,
    Corrected = 4
}

public sealed class DidoxInvoiceRequest
{
    [JsonPropertyName("Version")]
    public int Version { get; init; } = 1;

    [JsonPropertyName("WaybillLocalIds")]
    public IReadOnlyList<long> WaybillLocalIds { get; init; } = [];

    [JsonPropertyName("HasMarking")]
    public bool HasMarking { get; init; }

    [JsonPropertyName("HasRent")]
    public bool HasRent { get; init; }

    [JsonPropertyName("FacturaRentDoc")]
    public object? FacturaRentDoc { get; init; }

    [JsonPropertyName("FacturaType")]
    public int FacturaType { get; init; }

    [JsonPropertyName("ProductList")]
    public DidoxProductList ProductList { get; init; } = new();

    [JsonPropertyName("FacturaDoc")]
    public DidoxFacturaDoc FacturaDoc { get; init; } = new();

    [JsonPropertyName("ContractDoc")]
    public DidoxContractDoc ContractDoc { get; init; } = new();

    [JsonPropertyName("ContractId")]
    public string ContractId { get; init; } = string.Empty;

    [JsonPropertyName("LotId")]
    public string LotId { get; init; } = string.Empty;

    [JsonPropertyName("OldFacturaDoc")]
    public DidoxOldFacturaDoc OldFacturaDoc { get; init; } = new();

    [JsonPropertyName("SellerTin")]
    public string SellerTin { get; init; } = string.Empty;

    [JsonPropertyName("Seller")]
    public DidoxParty Seller { get; init; } = new();

    [JsonPropertyName("ItemReleasedDoc")]
    public DidoxItemReleasedDoc ItemReleasedDoc { get; init; } = new();

    [JsonPropertyName("BuyerTin")]
    public string BuyerTin { get; init; } = string.Empty;

    [JsonPropertyName("Buyer")]
    public DidoxParty Buyer { get; init; } = new();
}

public sealed class DidoxProductList
{
    [JsonPropertyName("HasCommittent")]
    public bool HasCommittent { get; init; }

    [JsonPropertyName("HasLgota")]
    public bool HasLgota { get; init; }

    [JsonPropertyName("Tin")]
    public string Tin { get; init; } = string.Empty;

    [JsonPropertyName("HasExcise")]
    public bool HasExcise { get; init; }

    [JsonPropertyName("HasVat")]
    public bool HasVat { get; init; }

    [JsonPropertyName("Products")]
    public IReadOnlyList<DidoxInvoiceProduct> Products { get; init; } = [];
}

public sealed class DidoxFacturaDoc
{
    [JsonPropertyName("FacturaNo")]
    public string FacturaNo { get; init; } = string.Empty;

    [JsonPropertyName("FacturaDate")]
    public string FacturaDate { get; init; } = string.Empty;
}

public sealed class DidoxContractDoc
{
    [JsonPropertyName("ContractNo")]
    public string ContractNo { get; init; } = string.Empty;

    [JsonPropertyName("ContractDate")]
    public string ContractDate { get; init; } = string.Empty;
}

public sealed class DidoxOldFacturaDoc
{
    [JsonPropertyName("OldFacturaDate")]
    public string OldFacturaDate { get; init; } = string.Empty;

    [JsonPropertyName("OldFacturaNo")]
    public string OldFacturaNo { get; init; } = string.Empty;

    [JsonPropertyName("OldFacturaId")]
    public string OldFacturaId { get; init; } = string.Empty;
}

public sealed class DidoxItemReleasedDoc
{
    [JsonPropertyName("ItemReleasedPinfl")]
    public string ItemReleasedPinfl { get; init; } = string.Empty;

    [JsonPropertyName("ItemReleasedFio")]
    public string ItemReleasedFio { get; init; } = string.Empty;
}

public sealed class DidoxParty
{
    [JsonPropertyName("Name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("BranchCode")]
    public string BranchCode { get; init; } = string.Empty;

    [JsonPropertyName("BranchName")]
    public string BranchName { get; init; } = string.Empty;

    // TODO(domain): VAT registration code — reachable via OrganizationTaxSetting.VatRegistrationNumber
    // (effective-date resolution) for the seller; the counterparty has no such field.
    [JsonPropertyName("VatRegCode")]
    public string VatRegCode { get; init; } = string.Empty;

    // TODO(domain): bank account number — reachable via (Org|Counterparty)BankAccount.AccountNumber (IsMain).
    [JsonPropertyName("Account")]
    public string Account { get; init; } = string.Empty;

    // TODO(domain): МФО — reachable via Bank.Mfo of the main bank account.
    [JsonPropertyName("BankId")]
    public string BankId { get; init; } = string.Empty;

    [JsonPropertyName("Address")]
    public string Address { get; init; } = string.Empty;

    [JsonPropertyName("Director")]
    public string Director { get; init; } = string.Empty;

    // TODO(domain): accountant name — not present in the domain model.
    [JsonPropertyName("Accountant")]
    public string Accountant { get; init; } = string.Empty;

    // TODO(domain): VAT registration status code — not present in the domain model.
    [JsonPropertyName("VatRegStatus")]
    public int VatRegStatus { get; init; }
}

public sealed class DidoxInvoiceProduct
{
    [JsonPropertyName("OrdNo")]
    public int OrdNo { get; init; }

    [JsonPropertyName("LgotaId")]
    public int? LgotaId { get; init; }

    [JsonPropertyName("CommittentName")]
    public string CommittentName { get; init; } = string.Empty;

    [JsonPropertyName("CommittentTin")]
    public string CommittentTin { get; init; } = string.Empty;

    [JsonPropertyName("CommittentVatRegCode")]
    public string CommittentVatRegCode { get; init; } = string.Empty;

    [JsonPropertyName("CommittentVatRegStatus")]
    public string CommittentVatRegStatus { get; init; } = string.Empty;

    [JsonPropertyName("Name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>MXIK (17 chars) — from Product.Mxik.</summary>
    [JsonPropertyName("CatalogCode")]
    public string CatalogCode { get; init; } = string.Empty;

    // TODO(domain): MXIK catalog name — not stored on Product (only the code). Resolve via MXIK lookup.
    [JsonPropertyName("CatalogName")]
    public string CatalogName { get; init; } = string.Empty;

    [JsonPropertyName("Marks")]
    public string Marks { get; init; } = string.Empty;

    [JsonPropertyName("Barcode")]
    public string Barcode { get; init; } = string.Empty;

    /// <summary>НЕ ИСПОЛЬЗУЕТСЯ — always null per the Didox doc.</summary>
    [JsonPropertyName("MeasureId")]
    public object? MeasureId => null;

    // TODO(domain): Didox package code from /v1/measures/all — our Unit has no Didox measure code.
    [JsonPropertyName("PackageCode")]
    public string PackageCode { get; init; } = string.Empty;

    [JsonPropertyName("PackageName")]
    public string PackageName { get; init; } = string.Empty;

    [JsonPropertyName("Count")]
    public string Count { get; init; } = string.Empty;

    [JsonPropertyName("Summa")]
    public string Summa { get; init; } = string.Empty;

    [JsonPropertyName("DeliverySum")]
    public string DeliverySum { get; init; } = string.Empty;

    [JsonPropertyName("VatRate")]
    public string VatRate { get; init; } = string.Empty;

    [JsonPropertyName("VatSum")]
    public string VatSum { get; init; } = string.Empty;

    [JsonPropertyName("ExciseRate")]
    public int ExciseRate { get; init; }

    [JsonPropertyName("ExciseSum")]
    public int ExciseSum { get; init; }

    [JsonPropertyName("DeliverySumWithVat")]
    public string DeliverySumWithVat { get; init; } = string.Empty;

    [JsonPropertyName("WithoutVat")]
    public bool WithoutVat { get; init; }

    [JsonPropertyName("WithoutExcise")]
    public bool WithoutExcise { get; init; } = true;

    [JsonPropertyName("LgotaType")]
    public string? LgotaType { get; init; }

    [JsonPropertyName("LgotaName")]
    public string? LgotaName { get; init; }

    [JsonPropertyName("LgotaVatSum")]
    public int LgotaVatSum { get; init; }

    [JsonPropertyName("WarehouseId")]
    public int? WarehouseId { get; init; }

    // TODO(domain): product origin code (справочник) — not stored on Product.
    [JsonPropertyName("Origin")]
    public int Origin { get; init; }
}

/// <summary>Didox create-document envelope: { "document_json": &lt;payload&gt; }.</summary>
public sealed class DidoxDocumentEnvelope<T>
{
    [JsonPropertyName("document_json")]
    public T DocumentJson { get; init; } = default!;
}
