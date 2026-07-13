namespace Application.Features.Edocs;

public sealed class EdocsChallengeRequest
{
    public string SerialNumber { get; init; } = string.Empty;
}

public sealed class EdocsChallengeResponse
{
    public string ChallengeId { get; init; } = string.Empty;
    public string AuthId { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; init; }
}

public sealed class EdocsLoginRequest
{
    public string ChallengeId { get; init; } = string.Empty;
    public string SerialNumber { get; init; } = string.Empty;
    public string Pkcs7 { get; init; } = string.Empty;
}

public sealed class EdocsLoginResponse
{
    public bool Connected { get; init; }
}

public sealed class EdocsProfileResponse
{
    public string Tin { get; init; } = string.Empty;
    public string? Name { get; init; }
}

public sealed class EdocsDocumentListQuery
{
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 20;
    public string? Sort { get; init; }
    public int? Order { get; init; }
    public string? Filter { get; init; }
    public string? Fields { get; init; }
    public string? Io { get; init; }
    public string? Status { get; init; }
    public string? Type { get; init; }
}

public sealed class EdocsDocumentListResponse
{
    public IReadOnlyCollection<EdocsDocumentListItem> Items { get; init; } = [];
    public int? Total { get; init; }
}

public sealed class EdocsDocumentListItem
{
    public string? Id { get; init; }
    public string? Number { get; init; }
    public string? Type { get; init; }
    public string? Status { get; init; }
    public DateTimeOffset? Date { get; init; }
}
