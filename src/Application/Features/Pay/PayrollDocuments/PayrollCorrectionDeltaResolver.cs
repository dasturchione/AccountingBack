namespace Application.Features.Pay.PayrollDocuments;

/// <summary>
/// Resolves the correction amount that is actually stored on a correction document.
/// 1C-style: the accountant enters the desired NEW value (<paramref name="targetAmount"/>)
/// and the server records only the difference against the currently posted amount, so
/// the correction never re-adds the original salary. When no target is given the raw
/// delta (<paramref name="rawAmount"/>) is used as-is (ad-hoc доначисление/удержание).
/// </summary>
public static class PayrollCorrectionDeltaResolver
{
    public static decimal Resolve(decimal? targetAmount, decimal? rawAmount, decimal currentPosted)
    {
        var delta = targetAmount.HasValue
            ? targetAmount.Value - currentPosted
            : rawAmount ?? 0m;
        return decimal.Round(delta, 2, MidpointRounding.AwayFromZero);
    }
}
