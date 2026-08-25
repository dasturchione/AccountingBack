namespace Domain.Entities;

public static class EdoImportMarkingPolicy
{
    public const string ProviderDataRequired = "MARKING_PROVIDER_DATA_REQUIRED";
    public const string CountMismatch = "MARKING_COUNT_MISMATCH";
    public const string Duplicate = "MARKING_DUPLICATE";
    public const string AlreadyUsed = "MARKING_ALREADY_USED";
    public const string AlreadyUsedSkipped = "MARKING_ALREADY_USED_SKIPPED";
    public const string QuantityInvalid = "MARKING_QUANTITY_INVALID";
    public const string ProductPieceTrackingRequired = "PRODUCT_PIECE_TRACKING_REQUIRED";

    private const string SkippedSuffix = "_SKIPPED";

    public static string? ValidateStructure(
        bool isPieceTracked,
        bool isService,
        decimal? quantity,
        IReadOnlyCollection<string> markings)
    {
        if (isService)
            return null;
        if (!isPieceTracked)
            return markings.Count == 0 ? null : ProductPieceTrackingRequired;
        if (!quantity.HasValue
            || quantity.Value <= 0
            || quantity.Value > int.MaxValue
            || decimal.Truncate(quantity.Value) != quantity.Value)
            return QuantityInvalid;
        if (markings.Distinct(StringComparer.Ordinal).Count() != markings.Count)
            return Duplicate;
        return markings.Count <= decimal.ToInt32(quantity.Value)
            ? null
            : CountMismatch;
    }

    public static bool IsStructuralFailure(string? safeErrorCode) => safeErrorCode is
        ProviderDataRequired or Duplicate or QuantityInvalid;

    public static bool CanSkipConflict(string? safeErrorCode) => safeErrorCode is
        AlreadyUsed or CountMismatch or ProviderDataRequired or Duplicate or QuantityInvalid;

    public static bool IsSkippedConflict(string? safeErrorCode) =>
        GetOriginalConflict(safeErrorCode) is not null
        && !CanSkipConflict(safeErrorCode);

    public static string? GetOriginalConflict(string? safeErrorCode)
    {
        if (CanSkipConflict(safeErrorCode))
            return safeErrorCode;
        if (safeErrorCode is null
            || !safeErrorCode.EndsWith(SkippedSuffix, StringComparison.Ordinal))
            return null;

        var original = safeErrorCode[..^SkippedSuffix.Length];
        return CanSkipConflict(original) ? original : null;
    }

    public static string ToSkippedConflict(string safeErrorCode)
    {
        if (!CanSkipConflict(safeErrorCode))
            throw new ArgumentException("Safe error code is not a skippable marking conflict.", nameof(safeErrorCode));

        return safeErrorCode + SkippedSuffix;
    }
}
