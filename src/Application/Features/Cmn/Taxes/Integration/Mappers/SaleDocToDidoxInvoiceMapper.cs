using System.Globalization;
using Application.Features.Cmn.Taxes.Integration.DTOs;
using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.Cmn.Taxes.Integration.Mappers;

public static class SaleDocToDidoxInvoiceMapper
{
    private const string DidoxDateFormat = "yyyy-MM-dd";

    public sealed record MappingContext(
        OrganizationTaxSetting? SellerTaxSetting,
        BankAccount? SellerBankAccount,
        CounterpartyBankAccount? BuyerBankAccount,
        DidoxBuyerProfile? BuyerProfile = null,
        IReadOnlyDictionary<long, IReadOnlyList<DidoxResolvedProductLine>>? ProductLines = null);

    /// <summary>Resolved outside the mapper; keeps mapper free of persistence and I/O concerns.</summary>
    public sealed record DidoxBuyerProfile(string VatRegCode, short VatRegStatus, string LegalAddress);

    /// <summary>One source SaleDocProduct can yield multiple Didox lines when stock origins differ.</summary>
    public sealed record DidoxResolvedProductLine(
        string CatalogName,
        string PackageCode,
        string PackageName,
        short OriginCode,
        string OriginName,
        decimal Quantity,
        decimal UnitPrice,
        decimal Amount,
        decimal VatAmount,
        decimal TotalAmount);

    public static Result<DidoxInvoiceRequest> Map(SaleDoc doc, MappingContext context,
        DidoxFacturaType facturaType = DidoxFacturaType.Standard)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(context);

        var error = Validate(doc, context);
        if (error is not null)
            return Result.Failure<DidoxInvoiceRequest>(error);

        var sellerTin = doc.Organization!.Inn;
        var products = doc.SaleDocProducts.OrderBy(x => x.Id)
            .SelectMany(line => GetLines(line, context))
            .Select((line, index) => MapProduct(line.Line, line.Resolved, index + 1)).ToList();

        return Result.Success(new DidoxInvoiceRequest
        {
            Version = 1,
            FacturaType = (int)facturaType,
            HasMarking = null,
            ProductList = new DidoxProductList { Tin = sellerTin, HasVat = products.Any(x => !x.WithoutVat), Products = products },
            FacturaDoc = new DidoxFacturaDoc
            {
                FacturaNo = doc.DocNumber,
                FacturaDate = doc.DocDate.ToString(DidoxDateFormat, CultureInfo.InvariantCulture)
            },
            ContractDoc = doc.Contract is null ? new DidoxContractDoc() : new DidoxContractDoc
            {
                ContractNo = doc.Contract.ContractNumber,
                ContractDate = doc.Contract.ContractDate.ToString(DidoxDateFormat, CultureInfo.InvariantCulture)
            },
            SellerTin = sellerTin!,
            Seller = MapParty(doc.Organization.FullName, doc.Organization.Address, doc.Organization.Director,
                context.SellerTaxSetting!.IsVatPayer, context.SellerTaxSetting.VatRegistrationNumber,
                context.SellerBankAccount!.AccountNumber, context.SellerBankAccount.Bank.Mfo),
            BuyerTin = doc.Counterparty!.Inn!,
            Buyer = MapParty(doc.Counterparty.FullName, context.BuyerProfile?.LegalAddress ?? doc.Counterparty.Address, null,
                context.BuyerProfile is null ? null : true, context.BuyerProfile?.VatRegCode,
                context.BuyerBankAccount!.AccountNumber, context.BuyerBankAccount.Bank.Mfo, context.BuyerProfile?.VatRegStatus)
        });
    }

    private static Error? Validate(SaleDoc doc, MappingContext context)
    {
        if (string.IsNullOrWhiteSpace(doc.Organization?.Inn))
            return Error.Problem("Didox.SellerTinRequired", "SellerTin is required.");
        if (string.IsNullOrWhiteSpace(doc.Counterparty?.Inn))
            return Error.Problem("Didox.BuyerTinRequired", "BuyerTin is required.");
        if (context.SellerTaxSetting is null || string.IsNullOrWhiteSpace(context.SellerTaxSetting.VatRegistrationNumber))
            return Error.Problem("Didox.SellerVatRequired", "Seller VAT registration number is required.");
        if (context.SellerBankAccount?.Bank is null || string.IsNullOrWhiteSpace(context.SellerBankAccount.AccountNumber) || string.IsNullOrWhiteSpace(context.SellerBankAccount.Bank.Mfo))
            return Error.Problem("Didox.SellerBankRequired", "Seller main bank account and MFO are required.");
        if (context.BuyerBankAccount?.Bank is null || string.IsNullOrWhiteSpace(context.BuyerBankAccount.AccountNumber) || string.IsNullOrWhiteSpace(context.BuyerBankAccount.Bank.Mfo))
            return Error.Problem("Didox.BuyerBankRequired", "Buyer main bank account and MFO are required.");
        return null;
    }

    private static IEnumerable<(SaleDocProduct Line, DidoxResolvedProductLine? Resolved)> GetLines(SaleDocProduct line, MappingContext context)
    {
        if (context.ProductLines is not null && context.ProductLines.TryGetValue(line.Id, out var resolved))
            return resolved.Select(x => (line, (DidoxResolvedProductLine?)x));

        return [(line, null)];
    }

    private static DidoxInvoiceProduct MapProduct(SaleDocProduct line, DidoxResolvedProductLine? resolved, int ordNo)
    {
        var rate = line.VatRate?.Rate ?? 0m;
        return new DidoxInvoiceProduct
        {
            OrdNo = ordNo, Name = line.Product?.Name ?? string.Empty,
            CatalogCode = line.Product?.Mxik ?? string.Empty, CatalogName = resolved?.CatalogName,
            PackageName = resolved?.PackageName ?? line.Unit?.Name ?? string.Empty, PackageCode = resolved?.PackageCode,
            Count = Number(resolved?.Quantity ?? line.Quantity), Summa = Number(resolved?.UnitPrice ?? line.UnitPrice), DeliverySum = Money(resolved?.Amount ?? line.Amount),
            VatRate = Number(rate), VatSum = Money(resolved?.VatAmount ?? line.VatAmount), DeliverySumWithVat = Money(resolved?.TotalAmount ?? line.TotalAmount),
            WithoutVat = rate <= 0m, Origin = resolved?.OriginCode
        };
    }

    private static DidoxParty MapParty(string? name, string? address, string? director,
        bool? isVatPayer, string? vatRegCode, string account, string? mfo, int? vatRegStatus = null) => new()
    {
        Name = name ?? string.Empty, Address = address ?? string.Empty, Director = director ?? string.Empty,
        VatRegCode = vatRegCode ?? string.Empty, Account = account, BankId = mfo ?? string.Empty,
        VatRegStatus = vatRegStatus ?? (isVatPayer.HasValue ? (isVatPayer.Value ? 20 : 10) : null)
    };

    private static string Number(decimal value) => value.ToString(CultureInfo.InvariantCulture);
    private static string Money(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);
}
