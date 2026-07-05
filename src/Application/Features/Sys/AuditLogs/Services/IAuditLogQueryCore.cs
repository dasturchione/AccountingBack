using SharedKernel.QueryResults;
using SharedKernel.Results;

namespace Application.Features.AuditLogs;

public interface IAuditLogQueryCore
{
    Task<Result<List<AuditLogQueryItem>>> QueryAsync(
        AuditLogQueryFilter filter,
        AuditLogQueryOptions options,
        CancellationToken ct = default);

    Task<Result<PagedList<AuditLogQueryItem>>> QueryPagedAsync(
        AuditLogQueryFilter filter,
        AuditLogQueryOptions options,
        AuditLogQueryPagination pagination,
        CancellationToken ct = default);
}
