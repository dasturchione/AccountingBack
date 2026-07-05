namespace Application.Features.AuditLogs;

public enum AuditLogQueryScope
{
    Record = 0,
    Organization = 1,
    Global = 2
}

public sealed class AuditLogQueryOptions
{
    public required AuditLogQueryScope Scope { get; init; }
    public bool IncludeDisplayNames { get; init; }

    public static AuditLogQueryOptions ForRecord() =>
        new()
        {
            Scope = AuditLogQueryScope.Record,
            IncludeDisplayNames = false
        };

    public static AuditLogQueryOptions ForOrganization(bool includeDisplayNames = false) =>
        new()
        {
            Scope = AuditLogQueryScope.Organization,
            IncludeDisplayNames = includeDisplayNames
        };

    public static AuditLogQueryOptions ForGlobal() =>
        new()
        {
            Scope = AuditLogQueryScope.Global,
            IncludeDisplayNames = true
        };
}

public sealed class AuditLogQueryFilter
{
    public int? OrganizationId { get; init; }
    public int? UserId { get; init; }
    public int? ChangedUserId { get; init; }
    public string? EntityType { get; init; }
    public string? TableName { get; init; }
    public string? EntityId { get; init; }
    public string? RecordId { get; init; }
    public string? Action { get; init; }
    public string? SearchText { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public sealed class AuditLogQueryPagination
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

public sealed class AuditLogQueryItem : AuditLogCoreDto
{
    public string? OrganizationName { get; init; }
    public string? RequestId { get; init; }
    public string? ClientAddr { get; init; }
    public string? ApplicationName { get; init; }
}
