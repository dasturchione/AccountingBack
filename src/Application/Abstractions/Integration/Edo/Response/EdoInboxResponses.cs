namespace Application.Abstractions.Integration.Edo;

public sealed class EdoInboxListDto
{
    public IReadOnlyCollection<EdoDocumentDto> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int? TotalCount { get; init; }
    public int? TotalDocs { get; init; }
    public int? TotalPages { get; init; }
    public bool? HasNextPage { get; init; }
    public bool? HasPrevPage { get; init; }
    public bool? HasPreviousPage { get; init; }
    public int? Limit { get; init; }
    public int? NextPage { get; init; }
    public int? PrevPage { get; init; }
    public int? PreviousPage { get; init; }
    public int? PagingCounter { get; init; }
}

public sealed class EdoPagedDocumentResponse
{
    public IReadOnlyCollection<EdoDocumentDto> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int? TotalCount { get; init; }
    public int? TotalPages { get; init; }
    public bool? HasNextPage { get; init; }
    public bool? HasPreviousPage { get; init; }

    public static EdoPagedDocumentResponse From(EdoInboxListDto response) => new()
    {
        Items = response.Items,
        Page = response.Page,
        PageSize = response.PageSize,
        TotalCount = response.TotalCount,
        TotalPages = response.TotalPages,
        HasNextPage = response.HasNextPage,
        HasPreviousPage = response.HasPreviousPage ?? response.HasPrevPage
    };
}

public sealed class EdoInboxSummaryDto
{
    public EdoProviderCode Provider { get; init; }

    public IReadOnlyDictionary<string, int> Inbox { get; init; } =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, int> Outbox { get; init; } =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
}

public sealed class EdoPublicInboxSummaryDto
{
    public EdoProviderCode Provider { get; init; }
    public EdoPublicStatusCountsDto Inbox { get; init; } = new();
    public EdoPublicStatusCountsDto Outbox { get; init; } = new();

    public static EdoPublicInboxSummaryDto From(EdoInboxSummaryDto summary) => new()
    {
        Provider = summary.Provider,
        Inbox = EdoPublicStatusCountsDto.From(summary.Inbox),
        Outbox = EdoPublicStatusCountsDto.From(summary.Outbox)
    };
}

public sealed class EdoPublicStatusCountsDto
{
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public int? Received { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public int? Signed { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public int? Rejected { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public int? Draft { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public int? Sent { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public int? Deleted { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public int? Archived { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public int? PendingSignature { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public int? PartnerSignaturePending { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public int? AgentSignaturePending { get; init; }

    public static EdoPublicStatusCountsDto From(IReadOnlyDictionary<string, int> source)
    {
        int? Read(params string[] keys)
        {
            foreach (var key in keys)
            {
                if (source.TryGetValue(key, out var value))
                    return value;
            }

            return null;
        }

        return new EdoPublicStatusCountsDto
        {
            Received = Read("received"),
            Signed = Read("signed"),
            Rejected = Read("rejected"),
            Draft = Read("draft", "drafts"),
            Sent = Read("sent", "sended"),
            Deleted = Read("deleted"),
            Archived = Read("archived"),
            PendingSignature = Read("pending", "pending_signature", "pending-signature"),
            PartnerSignaturePending = Read("partner_signature_pending", "partner-signature-pending"),
            AgentSignaturePending = Read("agent_signature_pending", "agent-signature-pending")
        };
    }
}

public sealed class EdoInboxRejectDto
{
    public EdoDocumentDto Document { get; init; } = new();
    public EdoSigningSessionDto? SigningSession { get; init; }
}
