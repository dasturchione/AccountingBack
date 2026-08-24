namespace Domain.Entities;

public static class EdoImportBatchProviderCode
{
    public const string Edocs = "EDOCS";
}

public static class EdoImportBatchStatus
{
    public const string Planned = "PLANNED";
    public const string Applying = "APPLYING";
    public const string Partial = "PARTIAL";
    public const string Completed = "COMPLETED";
    public const string Failed = "FAILED";
    public const string Cancelled = "CANCELLED";

    public static bool IsDefined(string value) => value is
        Planned or Applying or Partial or Completed or Failed or Cancelled;
}

public static class EdoImportBatchDocumentStatus
{
    public const string Factura = "FACTURA";
    public const string WaybillLocal = "WAYBILL_LOCAL";
    public const string Signed = "SIGNED";
    public const string WaitingForSignature = "WAITING_FOR_SIGNATURE";
    public const string Blocked = "BLOCKED";
    public const string Imported = "IMPORTED";
    public const string Failed = "FAILED";
    public const string AlreadyImported = "ALREADY_IMPORTED";
    public const string NotEligible = "NOT_ELIGIBLE";

    public static bool IsDefined(string value) => value is
        Signed or WaitingForSignature or Blocked or Imported or Failed or AlreadyImported or NotEligible;

    public static string NormalizeDocumentType(string? value) =>
        string.Equals(value?.Trim(), "waybillLocal", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value?.Trim(), WaybillLocal, StringComparison.OrdinalIgnoreCase)
            ? WaybillLocal
            : string.Equals(value?.Trim(), Factura, StringComparison.OrdinalIgnoreCase)
                ? Factura
                : value?.Trim().ToUpperInvariant() ?? string.Empty;

    public static bool IsSupportedDocumentType(string? value) =>
        NormalizeDocumentType(value) is Factura or WaybillLocal;
}
