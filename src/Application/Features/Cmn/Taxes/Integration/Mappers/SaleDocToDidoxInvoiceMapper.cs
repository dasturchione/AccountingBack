using System.Globalization;
using Application.Features.Cmn.Taxes.Integration.DTOs;
using Domain.Entities;

namespace Application.Features.Cmn.Taxes.Integration.Mappers;

/// <summary>
/// Maps a posted sales document (счёт-фактура source) into the official Didox ЭСФ payload.
/// Pure transformation — reuses amounts already computed on the domain document (no recalculation,
/// no hardcoded rates). The caller must load the SaleDoc graph with its Organization, Counterparty,
/// Contract, and SaleDocProducts (+ Product, Unit, VatRate) navigations.
///
/// Fields with no domain source are left at safe defaults and flagged TODO(domain); the D3 report
/// lists them for the accountant. They are NOT guessed here.
/// </summary>
public static class SaleDocToDidoxInvoiceMapper
{
    private const string DidoxDateFormat = "yyyy-MM-dd"; // confirmed ISO date

    // TODO(domain): Origin (справочник происхождения товара) has no domain source. 1 is a placeholder
    // and MUST be replaced with a per-product classification before a real submit.
    private const int DefaultOrigin = 1;

    // TODO(domain): VAT registration status code has no domain source.
    private const int DefaultVatRegStatus = 0;

    public static DidoxInvoiceRequest Map(
        SaleDoc doc,
        DidoxFacturaType facturaType = DidoxFacturaType.Standard)
    {
        ArgumentNullException.ThrowIfNull(doc);

        var sellerTin = doc.Organization?.Inn ?? string.Empty;

        var products = doc.SaleDocProducts
            .OrderBy(line => line.Id)
            .Select((line, index) => MapProduct(line, index + 1))
            .ToList();

        var hasVat = products.Any(p => !p.WithoutVat);

        return new DidoxInvoiceRequest
        {
            Version = 1,
            FacturaType = (int)facturaType,
            HasMarking = false, // TODO(domain): set true for marked (КИЗ) goods — no domain flag yet.
            ProductList = new DidoxProductList
            {
                Tin = sellerTin,
                HasVat = hasVat,
                Products = products
            },
            FacturaDoc = new DidoxFacturaDoc
            {
                FacturaNo = doc.DocNumber,
                FacturaDate = doc.DocDate.ToString(DidoxDateFormat, CultureInfo.InvariantCulture)
            },
            ContractDoc = doc.Contract is null
                ? new DidoxContractDoc()
                : new DidoxContractDoc
                {
                    ContractNo = doc.Contract.ContractNumber,
                    ContractDate = doc.Contract.ContractDate.ToString(DidoxDateFormat, CultureInfo.InvariantCulture)
                },
            SellerTin = sellerTin,
            Seller = MapParty(doc.Organization?.FullName, doc.Organization?.Address, doc.Organization?.Director),
            BuyerTin = doc.Counterparty?.Inn ?? string.Empty,
            Buyer = MapParty(doc.Counterparty?.FullName, doc.Counterparty?.Address, director: null)
        };
    }

    private static DidoxInvoiceProduct MapProduct(SaleDocProduct line, int ordNo)
    {
        var rate = line.VatRate?.Rate ?? 0m;

        return new DidoxInvoiceProduct
        {
            OrdNo = ordNo,
            Name = line.Product?.Name ?? string.Empty,
            CatalogCode = line.Product?.Mxik ?? string.Empty,
            CatalogName = string.Empty,   // TODO(domain): MXIK catalog name not stored on Product.
            PackageName = line.Unit?.Name ?? string.Empty,
            PackageCode = string.Empty,   // TODO(domain): Didox measure code (/v1/measures/all) not mapped.
            Count = Number(line.Quantity),
            Summa = Number(line.UnitPrice),
            DeliverySum = Money(line.Amount),
            VatRate = Number(rate),
            VatSum = Money(line.VatAmount),
            DeliverySumWithVat = Money(line.TotalAmount),
            WithoutVat = rate <= 0m,
            Origin = DefaultOrigin
        };
    }

    private static DidoxParty MapParty(string? name, string? address, string? director) => new()
    {
        Name = name ?? string.Empty,
        Address = address ?? string.Empty,
        Director = director ?? string.Empty,
        VatRegStatus = DefaultVatRegStatus
        // VatRegCode / Account / BankId / Accountant left empty — TODO(domain), see report.
    };

    // Plain invariant string (Count / Summa / VatRate).
    private static string Number(decimal value) => value.ToString(CultureInfo.InvariantCulture);

    // Two-decimal invariant string (sums).
    private static string Money(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);
}
