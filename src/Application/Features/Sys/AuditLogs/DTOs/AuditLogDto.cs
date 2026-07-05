namespace Application.Features.AuditLogs;

public class AuditLogCoreDto
{
    public long Id { get; set; }
    public int? OrganizationId { get; set; }
    public string SchemaName { get; set; } = null!;
    public string TableName { get; set; } = null!;
    public string? RecordId { get; set; }
    public string Action { get; set; } = null!;
    public string? OldData { get; set; }
    public string? NewData { get; set; }
    public int? ChangedUserId { get; set; }
    public string? ChangedUserName { get; set; }
    public DateTime ChangedDate { get; set; }
}

public class AuditLogDto : AuditLogCoreDto
{
    public string? Comment { get; set; }
    public List<ChangeResult> ChangeResults { get; set; } = [];
}

public class ChangeResult
{
    public string? Path { get; set; }
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
}
