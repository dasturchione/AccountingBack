namespace Domain.Entities;

public static class EdoImportJobStatus
{
    public const string Queued = "QUEUED";
    public const string Scanning = "SCANNING";
    public const string WaitingAuth = "WAITING_AUTH";
    public const string PreflightReady = "PREFLIGHT_READY";
    public const string Importing = "IMPORTING";
    public const string Partial = "PARTIAL";
    public const string Completed = "COMPLETED";
    public const string Failed = "FAILED";
    public const string CancelRequested = "CANCEL_REQUESTED";
    public const string Cancelled = "CANCELLED";

    public static readonly string[] ActiveValues =
    [
        Queued,
        Scanning,
        WaitingAuth,
        PreflightReady,
        Importing,
        Partial,
        CancelRequested
    ];

    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> AllowedTransitions =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
        {
            [Queued] = Set(Scanning, CancelRequested, Failed),
            [Scanning] = Set(WaitingAuth, PreflightReady, Partial, Failed, CancelRequested),
            [WaitingAuth] = Set(Scanning, Importing, Failed, CancelRequested),
            [PreflightReady] = Set(Importing, Completed, Failed, CancelRequested),
            [Importing] = Set(WaitingAuth, Partial, Completed, Failed, CancelRequested),
            [Partial] = Set(Scanning, PreflightReady, Importing, Completed, Failed, CancelRequested),
            [CancelRequested] = Set(Cancelled, Failed),
            [Completed] = Set(),
            [Failed] = Set(),
            [Cancelled] = Set()
        };

    public static bool IsDefined(string status) => AllowedTransitions.ContainsKey(status);

    public static bool IsActive(string status) => ActiveValues.Contains(status, StringComparer.Ordinal);

    public static bool CanTransition(string currentStatus, string requestedStatus) =>
        AllowedTransitions.TryGetValue(currentStatus, out var allowed)
        && allowed.Contains(requestedStatus);

    private static IReadOnlySet<string> Set(params string[] values) =>
        new HashSet<string>(values, StringComparer.Ordinal);
}

public static class EdoImportCandidateStatus
{
    public const string Discovered = "DISCOVERED";
    public const string MappingRequired = "MAPPING_REQUIRED";
    public const string Ready = "READY";
    public const string PossibleDuplicate = "POSSIBLE_DUPLICATE";
    public const string Duplicate = "DUPLICATE";
    public const string Importing = "IMPORTING";
    public const string Imported = "IMPORTED";
    public const string Failed = "FAILED";
    public const string Skipped = "SKIPPED";

    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> AllowedTransitions =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
        {
            [Discovered] = Set(MappingRequired, Ready, PossibleDuplicate, Duplicate, Failed, Skipped),
            [MappingRequired] = Set(Ready, PossibleDuplicate, Duplicate, Failed, Skipped),
            [Ready] = Set(MappingRequired, Importing, PossibleDuplicate, Duplicate, Failed, Skipped),
            [PossibleDuplicate] = Set(Ready, Duplicate, Failed, Skipped),
            [Importing] = Set(Imported, Failed),
            [Failed] = Set(MappingRequired, Ready, Duplicate, Skipped),
            [Duplicate] = Set(),
            [Imported] = Set(),
            [Skipped] = Set()
        };

    public static bool IsDefined(string status) => AllowedTransitions.ContainsKey(status);

    public static bool CanTransition(string currentStatus, string requestedStatus) =>
        AllowedTransitions.TryGetValue(currentStatus, out var allowed)
        && allowed.Contains(requestedStatus);

    private static IReadOnlySet<string> Set(params string[] values) =>
        new HashSet<string>(values, StringComparer.Ordinal);
}

public static class EdoImportBulkImportStatus
{
    public const string Queued = "QUEUED";
    public const string Running = "RUNNING";
    public const string Paused = "PAUSED";
    public const string CancelRequested = "CANCEL_REQUESTED";
    public const string Cancelled = "CANCELLED";
    public const string Completed = "COMPLETED";

    public static bool IsActive(string? status) => status is Queued or Running or Paused or CancelRequested;
    public static bool IsRunnable(string? status) => status is Queued or Running;
}

public static class EdoImportProviderCheckpointStatus
{
    public const string Queued = "QUEUED";
    public const string Scanning = "SCANNING";
    public const string WaitingAuth = "WAITING_AUTH";
    public const string Partial = "PARTIAL";
    public const string Completed = "COMPLETED";
    public const string Failed = "FAILED";
    public const string Cancelled = "CANCELLED";
}

public static class EdoImportMappingStatus
{
    public const string Unresolved = "UNRESOLVED";
    public const string Partial = "PARTIAL";
    public const string Resolved = "RESOLVED";
}

public static class EdoImportDuplicateState
{
    public const string None = "NONE";
    public const string Possible = "POSSIBLE";
    public const string Confirmed = "CONFIRMED";
}

public static class EdoImportMarkingVerificationState
{
    public const string Unverified = "UNVERIFIED";
    public const string Verified = "VERIFIED";
    public const string Mismatch = "MISMATCH";
}
