namespace SharedKernel.Money;

/// <summary>
/// A document line's money, split into the net amount that reaches the income or expense
/// account, the VAT that reaches the tax account, and the gross the counterparty settles.
/// </summary>
public readonly record struct VatAmounts(decimal NetAmount, decimal VatAmount, decimal GrossAmount);

/// <summary>
/// Resolves a line's net/VAT/gross from the price it was entered at.
/// <para>
/// Prices are quoted both ways. A wholesale invoice usually quotes the price before VAT and
/// adds it on top; a retail price already contains it — a 12 000 soum shelf price is what the
/// customer hands over, of which 1 285.71 is VAT. Treating a VAT-inclusive price as exclusive
/// posted 12 000 to income plus 1 440 of VAT and left 1 440 hanging on the receivable, because
/// only 12 000 ever arrived in the till.
/// </para>
/// </summary>
public static class VatCalculator
{
    /// <summary>The entered price is net: VAT is added on top of it.</summary>
    public static VatAmounts FromNet(decimal netAmount, decimal rate)
    {
        var net = DocumentMoney.Round(netAmount);
        var vat = DocumentMoney.Round(net * NormalizeRate(rate) / 100m);
        return new VatAmounts(net, vat, DocumentMoney.Round(net + vat));
    }

    /// <summary>The entered price already contains VAT: it is extracted from it.</summary>
    public static VatAmounts FromGross(decimal grossAmount, decimal rate)
    {
        var gross = DocumentMoney.Round(grossAmount);
        var normalizedRate = NormalizeRate(rate);
        var vat = normalizedRate == 0m
            ? 0m
            : DocumentMoney.Round(gross * normalizedRate / (100m + normalizedRate));
        return new VatAmounts(DocumentMoney.Round(gross - vat), vat, gross);
    }

    /// <summary>
    /// Resolves the line from the entered amount, reading it as gross when
    /// <paramref name="priceIncludesVat"/> is set and as net otherwise.
    /// </summary>
    public static VatAmounts Resolve(decimal enteredAmount, decimal? rate, bool priceIncludesVat)
    {
        var effectiveRate = rate ?? 0m;
        return priceIncludesVat
            ? FromGross(enteredAmount, effectiveRate)
            : FromNet(enteredAmount, effectiveRate);
    }

    private static decimal NormalizeRate(decimal rate) => Math.Max(rate, 0m);
}
