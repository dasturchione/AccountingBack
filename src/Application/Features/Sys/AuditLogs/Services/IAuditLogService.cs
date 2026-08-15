namespace Application.Features.AuditLogs;

public interface IAuditLogService
{
    void SetOldValues(object oldValues);
    void SetNewValues(object newValues);
    Task CreateAsync(string tableName, string recordId, string operationType, string? comment = null, int? organizationId = null);
    Task<List<AuditLogDto>> GetByRecordAsync(AuditLogFilter filter);
}
