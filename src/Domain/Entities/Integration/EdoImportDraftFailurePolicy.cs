namespace Domain.Entities;

public static class EdoImportDraftFailurePolicy
{
    public const string LineValuesInvalid =
        "DRAFT_IMPORT_PURCHASEFROMEDO_HISTORICALVALIDATION_LINE_VALUES_INVALID";
    public const string MarkingAlreadyUsed = "DRAFT_IMPORT_MARKING_ALREADY_USED";
    public const string MarkingAlreadyUsedSkipped = "DRAFT_IMPORT_MARKING_ALREADY_USED_SKIPPED";

    private const string SkippedSuffix = "_SKIPPED";

    public static bool CanSkip(string? safeErrorCode) => safeErrorCode is LineValuesInvalid;

    public static bool IsSkipped(string? safeErrorCode) =>
        safeErrorCode == MarkingAlreadyUsedSkipped
        || safeErrorCode is not null
            && safeErrorCode.EndsWith(SkippedSuffix, StringComparison.Ordinal)
            && CanSkip(safeErrorCode[..^SkippedSuffix.Length]);

    public static bool IsMarkingAlreadyUsed(string? safeErrorCode) =>
        safeErrorCode == MarkingAlreadyUsed;

    public static string ToSkipped(string safeErrorCode)
    {
        if (!CanSkip(safeErrorCode) && !IsMarkingAlreadyUsed(safeErrorCode))
            throw new ArgumentException(
                "Safe error code is not a non-retryable historical Draft failure.",
                nameof(safeErrorCode));

        return safeErrorCode + SkippedSuffix;
    }
}
