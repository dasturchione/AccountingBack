using Application.Abstractions.Integration.Edo;

namespace Integration.Edo.Providers;

public static class EdoProviderStatusMapper
{
    public static EdoDocumentStatusDto Map(string? providerStatus) =>
        (providerStatus ?? string.Empty).Trim().ToUpperInvariant() switch
        {
            "UNKNOWN" => Create(EdoDocumentStatusCode.UNKNOWN, providerStatus),
            "PENDING_SIGNATURE" => Create(EdoDocumentStatusCode.PENDING_SIGNATURE, providerStatus),
            "PARTNER_SIGNATURE_PENDING" => Create(EdoDocumentStatusCode.PARTNER_SIGNATURE_PENDING, providerStatus),
            "AGENT_SIGNATURE_PENDING" => Create(EdoDocumentStatusCode.AGENT_SIGNATURE_PENDING, providerStatus),
            "DRAFT" => Create(EdoDocumentStatusCode.DRAFT, providerStatus),
            "PENDING" => Create(EdoDocumentStatusCode.PENDING, providerStatus),
            "SUBMITTED" => Create(EdoDocumentStatusCode.SENT, providerStatus),
            "SENT" => Create(EdoDocumentStatusCode.SENT, providerStatus),
            "SIGN_SENT" => Create(EdoDocumentStatusCode.SENT, providerStatus),
            "SIGNED" => Create(EdoDocumentStatusCode.SIGNED, providerStatus, isSuccessful: true),
            "RECEIVED" => Create(EdoDocumentStatusCode.RECEIVED, providerStatus),
            "REJECTED" => Create(EdoDocumentStatusCode.REJECTED, providerStatus, isTerminal: true),
            "DELETED" => Create(EdoDocumentStatusCode.DELETED, providerStatus, isTerminal: true),
            "ARCHIVED" => Create(EdoDocumentStatusCode.ARCHIVED, providerStatus, isTerminal: true),
            "COMPLETED" => Create(EdoDocumentStatusCode.COMPLETED, providerStatus, isTerminal: true, isSuccessful: true),
            "CANCELLED" => Create(EdoDocumentStatusCode.CANCELLED, providerStatus, isTerminal: true),
            "FAILED" => Create(EdoDocumentStatusCode.FAILED, providerStatus, isTerminal: true),
            "RECONCILIATION_REQUIRED" => Create(EdoDocumentStatusCode.RECONCILIATION_REQUIRED, providerStatus),
            _ => Create(EdoDocumentStatusCode.UNKNOWN, providerStatus)
        };

    public static EdoDocumentStatusDto MapDidoxStatus(int status) => status switch
    {
        0 => Create(EdoDocumentStatusCode.DRAFT, status.ToString()),
        1 => Create(EdoDocumentStatusCode.PARTNER_SIGNATURE_PENDING, status.ToString()),
        2 => Create(EdoDocumentStatusCode.PENDING_SIGNATURE, status.ToString()),
        6 => Create(EdoDocumentStatusCode.SENT, status.ToString()),
        60 => Create(EdoDocumentStatusCode.AGENT_SIGNATURE_PENDING, status.ToString()),
        3 => Create(EdoDocumentStatusCode.SIGNED, status.ToString(), isSuccessful: true),
        4 => Create(EdoDocumentStatusCode.REJECTED, status.ToString(), isTerminal: true),
        5 or 55 => Create(EdoDocumentStatusCode.DELETED, status.ToString(), isTerminal: true),
        50 => Create(EdoDocumentStatusCode.ARCHIVED, status.ToString(), isTerminal: true),
        40 => Create(EdoDocumentStatusCode.FAILED, status.ToString(), isTerminal: true),
        _ => Create(EdoDocumentStatusCode.UNKNOWN, status.ToString())
    };

    public static EdoDocumentStatusDto MapEdocsStatus(string? status)
    {
        var normalized = (status ?? string.Empty).Trim().ToLowerInvariant();
        return normalized switch
        {
            "draft" or "drafts" => Create(EdoDocumentStatusCode.DRAFT, status),
            "sended" or "sent" => Create(EdoDocumentStatusCode.SENT, status),
            "signed" => Create(EdoDocumentStatusCode.SIGNED, status, isSuccessful: true),
            "received" => Create(EdoDocumentStatusCode.RECEIVED, status),
            "rejected" or "reject" => Create(EdoDocumentStatusCode.REJECTED, status, isTerminal: true),
            "deleted" => Create(EdoDocumentStatusCode.DELETED, status, isTerminal: true),
            "cancelled" => Create(EdoDocumentStatusCode.CANCELLED, status, isTerminal: true),
            _ => Create(EdoDocumentStatusCode.UNKNOWN, status)
        };
    }

    public static EdoDocumentCategory MapCategory(
        EdoDirection direction,
        EdoDocumentStatusCode status,
        EdoDocumentCategory? requestedCategory = null) =>
        requestedCategory is EdoDocumentCategory.DRAFTS
            or EdoDocumentCategory.REJECTED
            or EdoDocumentCategory.DELETED_ARCHIVED
            or EdoDocumentCategory.ALL
            ? requestedCategory.Value
            : status switch
            {
                EdoDocumentStatusCode.DRAFT => EdoDocumentCategory.DRAFTS,
                EdoDocumentStatusCode.REJECTED => EdoDocumentCategory.REJECTED,
                EdoDocumentStatusCode.DELETED
                    or EdoDocumentStatusCode.ARCHIVED
                    or EdoDocumentStatusCode.CANCELLED => EdoDocumentCategory.DELETED_ARCHIVED,
                _ => direction == EdoDirection.INBOX
                    ? EdoDocumentCategory.INBOX
                    : EdoDocumentCategory.OUTBOX
            };

    public static string ToStorageStatus(EdoDocumentStatusDto status) =>
        status.Code.ToString();

    private static EdoDocumentStatusDto Create(
        EdoDocumentStatusCode code,
        string? providerStatus,
        bool isTerminal = false,
        bool isSuccessful = false) => new()
        {
            Code = code,
            LocalCode = code,
            ProviderStatusCode = providerStatus,
            ProviderRawStatus = providerStatus,
            IsTerminal = isTerminal,
            IsSuccessful = isSuccessful
        };
}
