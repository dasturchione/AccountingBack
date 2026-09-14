using SharedKernel.Money;

namespace Application.Features.RetailSaleDocs;

public static class RetailSaleVatCalculator
{
    /// <summary>
    /// Resolves a retail line's net/VAT/gross. An explicit <paramref name="vatAmount"/> from the
    /// fiscal register wins; otherwise the amount is read as VAT-inclusive when the receipt says
    /// its prices already contain VAT — which is the usual case for a shelf price.
    /// </summary>
    public static VatAmounts Resolve(
        decimal unitPrice,
        decimal quantity,
        decimal? vatAmount,
        decimal? vatRate,
        bool priceIncludesVat)
    {
        var enteredAmount = DocumentMoney.Round(unitPrice * quantity);
        if (vatAmount is not { } explicitVat)
            return VatCalculator.Resolve(enteredAmount, vatRate, priceIncludesVat);

        var vat = DocumentMoney.Round(explicitVat);
        return priceIncludesVat
            ? new VatAmounts(DocumentMoney.Round(enteredAmount - vat), vat, enteredAmount)
            : new VatAmounts(enteredAmount, vat, DocumentMoney.Round(enteredAmount + vat));
    }

    /// <summary>
    /// Splits a line's VAT over its marked items, keeping the parts adding up to the line.
    /// </summary>
    public static List<decimal> ResolvePerUnit(decimal totalVatAmount, int itemCount) =>
        DocumentMoney.Distribute(totalVatAmount, itemCount);
}
