namespace Application.Features.Rnt.RentalAccruals;

public static class RentalAccrualCalculator
{
    public static RentalAccrualAmounts Calculate(
        decimal contractAmount,
        decimal taxBaseAmount,
        decimal taxRate)
    {
        if (contractAmount < 0m)
            throw new ArgumentOutOfRangeException(nameof(contractAmount));
        if (taxBaseAmount < contractAmount)
            throw new ArgumentOutOfRangeException(nameof(taxBaseAmount));
        if (taxRate is < 0m or > 100m)
            throw new ArgumentOutOfRangeException(nameof(taxRate));

        var taxAmount = Round(taxBaseAmount * taxRate / 100m);
        var withheldFromContract = Round(contractAmount * taxRate / 100m);
        var payableAmount = Round(contractAmount - withheldFromContract);

        return new RentalAccrualAmounts(
            taxAmount,
            payableAmount,
            Round(taxAmount + payableAmount));
    }

    private static decimal Round(decimal value) =>
        Math.Round(value, 8, MidpointRounding.AwayFromZero);
}

public readonly record struct RentalAccrualAmounts(
    decimal TaxAmount,
    decimal PayableAmount,
    decimal Amount);
