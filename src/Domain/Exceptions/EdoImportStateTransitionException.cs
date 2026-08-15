namespace Domain.Exceptions;

public sealed class EdoImportStateTransitionException : InvalidOperationException
{
    public const string ErrorCode = "EDO_IMPORT_INVALID_STATE_TRANSITION";

    public EdoImportStateTransitionException(string aggregate, string currentStatus, string requestedStatus)
        : base($"{aggregate} cannot transition from '{currentStatus}' to '{requestedStatus}'.")
    {
        Aggregate = aggregate;
        CurrentStatus = currentStatus;
        RequestedStatus = requestedStatus;
    }

    public string Aggregate { get; }

    public string CurrentStatus { get; }

    public string RequestedStatus { get; }

    public string Code => ErrorCode;
}
