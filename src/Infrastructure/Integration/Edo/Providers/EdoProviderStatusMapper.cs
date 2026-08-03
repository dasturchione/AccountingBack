using Application.Abstractions.Integration.Edo;

namespace Integration.Edo.Providers;

public static class EdoProviderStatusMapper
{
    public static EdoDocumentStatusDto Map(string? providerStatus) =>
        (providerStatus ?? string.Empty).Trim().ToUpperInvariant() switch
        {
            "UNKNOWN" => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.UNKNOWN, ProviderStatusCode = providerStatus },
            "DRAFT" => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.DRAFT, ProviderStatusCode = providerStatus },
            "PENDING" => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.PENDING, ProviderStatusCode = providerStatus },
            "SUBMITTED" => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.SENT, ProviderStatusCode = providerStatus },
            "SENT" => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.SENT, ProviderStatusCode = providerStatus },
            "SIGN_SENT" => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.SENT, ProviderStatusCode = providerStatus },
            "SIGNED" => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.SIGNED, IsSuccessful = true, ProviderStatusCode = providerStatus },
            "RECEIVED" => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.RECEIVED, ProviderStatusCode = providerStatus },
            "REJECTED" => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.REJECTED, IsTerminal = true, ProviderStatusCode = providerStatus },
            "COMPLETED" => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.COMPLETED, IsTerminal = true, IsSuccessful = true, ProviderStatusCode = providerStatus },
            "CANCELLED" => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.CANCELLED, IsTerminal = true, ProviderStatusCode = providerStatus },
            "FAILED" => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.FAILED, IsTerminal = true },
            "RECONCILIATION_REQUIRED" => new EdoDocumentStatusDto
            {
                Code = EdoDocumentStatusCode.RECONCILIATION_REQUIRED,
                IsTerminal = false
            },
            _ => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.UNKNOWN }
        };

    public static EdoDocumentStatusDto MapDidoxStatus(int status) => status switch
    {
        0 => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.DRAFT, ProviderStatusCode = status.ToString() },
        1 or 2 or 6 or 60 => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.SENT, ProviderStatusCode = status.ToString() },
        3 => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.SIGNED, ProviderStatusCode = status.ToString(), IsSuccessful = true },
        4 => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.REJECTED, ProviderStatusCode = status.ToString(), IsTerminal = true },
        5 or 50 or 55 => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.CANCELLED, ProviderStatusCode = status.ToString(), IsTerminal = true },
        40 => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.FAILED, ProviderStatusCode = status.ToString(), IsTerminal = true },
        _ => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.UNKNOWN, ProviderStatusCode = status.ToString() }
    };

    public static EdoDocumentStatusDto MapEdocsStatus(string? status)
    {
        var normalized = (status ?? string.Empty).Trim().ToLowerInvariant();
        return normalized switch
        {
            "draft" or "drafts" => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.DRAFT, ProviderStatusCode = status },
            "sended" or "sent" => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.SENT, ProviderStatusCode = status },
            "signed" => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.SIGNED, ProviderStatusCode = status, IsSuccessful = true },
            "rejected" or "reject" => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.REJECTED, ProviderStatusCode = status, IsTerminal = true },
            _ => new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.UNKNOWN, ProviderStatusCode = status }
        };
    }

    public static string ToStorageStatus(EdoDocumentStatusDto status) =>
        status.Code.ToString();
}
