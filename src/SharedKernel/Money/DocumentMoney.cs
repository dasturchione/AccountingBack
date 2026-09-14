namespace SharedKernel.Money;

/// <summary>
/// Rounds document money to the scale the accounting register actually stores.
/// <para>
/// Document amount columns are <c>numeric(24,8)</c> but <c>acc_reg_entry.amount</c> is
/// <c>numeric(18,2)</c>. VAT computed to eight decimals therefore reached the ledger rounded,
/// and every line drifted a fraction away from the document it came from — enough, once summed
/// over a document, to disagree with the ЭСФ. Money is decided here, once, at the scale it is
/// stored and reported in, so the document and its postings are the same number.
/// </para>
/// </summary>
public static class DocumentMoney
{
    /// <summary>Decimal places money is kept at, matching <c>acc_reg_entry.amount</c>.</summary>
    public const int Scale = 2;

    private const decimal Cent = 0.01m;

    public static decimal Round(decimal value) =>
        Math.Round(value, Scale, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Splits <paramref name="total"/> into <paramref name="count"/> equal money shares whose
    /// sum is exactly <paramref name="total"/>. Dividing and rounding each share on its own
    /// loses or invents cents (100.00 over three items gives 33.33 × 3 = 99.99), so the cents
    /// left over are handed out one per share instead.
    /// </summary>
    public static List<decimal> Distribute(decimal total, int count)
    {
        if (count <= 0)
            return [];

        var rounded = Round(total);
        var share = decimal.Truncate(rounded / count * 100m) / 100m;
        var shares = Enumerable.Repeat(share, count).ToList();

        var remainder = rounded - share * count;
        var cent = rounded < 0m ? -Cent : Cent;
        for (var i = 0; i < count && Math.Abs(remainder) >= Cent / 2m; i++)
        {
            shares[i] += cent;
            remainder -= cent;
        }

        return shares;
    }
}
