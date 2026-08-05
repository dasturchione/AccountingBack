using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Platform;
using Domain.Entities;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using SharedKernel.Results;

namespace Application.Features.AuditLogs;

public sealed class AuditLogQueryCore : IAuditLogQueryCore
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<AuditLog> _auditLogQuery;
    private readonly IQueryRepository<User> _userQuery;
    private readonly IQueryRepository<Organization> _organizationQuery;

    public AuditLogQueryCore(
        IUserContext userContext,
        IQueryRepository<AuditLog> auditLogQuery,
        IQueryRepository<User> userQuery,
        IQueryRepository<Organization> organizationQuery)
    {
        _userContext = userContext;
        _auditLogQuery = auditLogQuery;
        _userQuery = userQuery;
        _organizationQuery = organizationQuery;
    }

    public async Task<Result<List<AuditLogQueryItem>>> QueryAsync(
        AuditLogQueryFilter filter,
        AuditLogQueryOptions options,
        CancellationToken ct = default)
    {
        var accessResult = EnsureAccess(options.Scope);
        if (!accessResult.IsSuccess)
            return Result.Failure<List<AuditLogQueryItem>>(accessResult.Error);

        if (!HasScopedVisibility(options.Scope))
            return Result.Success(new List<AuditLogQueryItem>());

        var logs = await _auditLogQuery.GetAllAsync(BuildListSpecification(filter, options.Scope), ct);
        var items = await EnrichAsync(logs, options.IncludeDisplayNames, ct);
        return Result.Success(items);
    }

    public async Task<Result<PagedList<AuditLogQueryItem>>> QueryPagedAsync(
        AuditLogQueryFilter filter,
        AuditLogQueryOptions options,
        AuditLogQueryPagination pagination,
        CancellationToken ct = default)
    {
        var accessResult = EnsureAccess(options.Scope);
        if (!accessResult.IsSuccess)
            return Result.Failure<PagedList<AuditLogQueryItem>>(accessResult.Error);

        if (!HasScopedVisibility(options.Scope))
            return Result.Success(new PagedList<AuditLogQueryItem>([], 0));

        var preparedPagination = PreparePagination(pagination);
        var paged = await _auditLogQuery.GetPagedAsync(
            BuildPagedSpecification(filter, options.Scope, preparedPagination.Page, preparedPagination.PageSize),
            ct);

        var items = await EnrichAsync(paged.Items, options.IncludeDisplayNames, ct);
        return Result.Success(new PagedList<AuditLogQueryItem>(items, paged.TotalCount));
    }

    private Result EnsureAccess(AuditLogQueryScope scope) =>
        scope == AuditLogQueryScope.Global && _userContext.UserKind != CurrentUserKind.SuperAdmin
            ? Result.Failure(PlatformErrors.GlobalAccessRequired())
            : Result.Success();

    private bool HasScopedVisibility(AuditLogQueryScope scope) =>
        scope == AuditLogQueryScope.Global || _userContext.UserKind == CurrentUserKind.SuperAdmin || _userContext.AllowedOrganizationIds.Count > 0;

    private QuerySpecification<AuditLog> BuildListSpecification(AuditLogQueryFilter filter, AuditLogQueryScope scope)
    {
        var orderBy = new Func<IQueryable<AuditLog>, IOrderedQueryable<AuditLog>>(query =>
            query.OrderByDescending(log => log.ChangedDate));

        return scope == AuditLogQueryScope.Record
            ? new QuerySpecification<AuditLog>
            {
                Criteria = BuildRecordCriteria(filter),
                OrderBy = orderBy
            }
            : new QuerySpecification<AuditLog>
            {
                Criteria = BuildScopedCriteria(filter, scope),
                OrderBy = orderBy
            };
    }

    private PagedQuerySpecification<AuditLog> BuildPagedSpecification(
        AuditLogQueryFilter filter,
        AuditLogQueryScope scope,
        int page,
        int pageSize) =>
        new()
        {
            Criteria = scope == AuditLogQueryScope.Record
                ? BuildRecordCriteria(filter)
                : BuildScopedCriteria(filter, scope),
            OrderBy = query => query.OrderByDescending(log => log.ChangedDate),
            Skip = (page - 1) * pageSize,
            Take = pageSize
        };

    private System.Linq.Expressions.Expression<Func<AuditLog, bool>> BuildRecordCriteria(AuditLogQueryFilter filter)
    {
        var tableName = ResolveEntityType(filter) ?? string.Empty;
        var recordId = ResolveEntityId(filter) ?? string.Empty;
        var action = filter.Action?.Trim().ToLowerInvariant();
        var searchText = filter.SearchText?.Trim().ToLowerInvariant();
        var changedUserId = ResolveChangedUserId(filter);
        var organizationId = filter.OrganizationId;
        var fromDate = filter.FromDate;
        var toDate = filter.ToDate;
        var isSuperAdmin = _userContext.UserKind == CurrentUserKind.SuperAdmin;
        var scopedOrganizationId = _userContext.OrganizationId;
        var allowedOrganizationIds = _userContext.AllowedOrganizationIds.ToArray();
        var hasActionFilter = !string.IsNullOrWhiteSpace(action);
        var hasSearchFilter = !string.IsNullOrWhiteSpace(searchText);

        return log =>
            log.RecordId == recordId
            && log.TableName == tableName
            && (!organizationId.HasValue || log.OrganizationId == organizationId.Value)
            && (!changedUserId.HasValue || log.ChangedUserId == changedUserId.Value)
            && (!hasActionFilter || log.Action.ToLower() == action!)
            && (!fromDate.HasValue || log.ChangedDate >= fromDate.Value)
            && (!toDate.HasValue || log.ChangedDate <= toDate.Value)
            && (!hasSearchFilter || MatchesSearch(log, searchText!))
            && (isSuperAdmin
                || (log.OrganizationId.HasValue
                    && (scopedOrganizationId.HasValue
                        ? log.OrganizationId.Value == scopedOrganizationId.Value
                        : allowedOrganizationIds.Contains(log.OrganizationId.Value))));
    }

    private System.Linq.Expressions.Expression<Func<AuditLog, bool>> BuildScopedCriteria(
        AuditLogQueryFilter filter,
        AuditLogQueryScope scope)
    {
        var tableName = ResolveEntityType(filter)?.Trim().ToLowerInvariant();
        var recordId = ResolveEntityId(filter)?.Trim().ToLowerInvariant();
        var action = filter.Action?.Trim().ToLowerInvariant();
        var searchText = filter.SearchText?.Trim().ToLowerInvariant();
        var hasTableNameFilter = !string.IsNullOrWhiteSpace(tableName);
        var hasRecordIdFilter = !string.IsNullOrWhiteSpace(recordId);
        var hasActionFilter = !string.IsNullOrWhiteSpace(action);
        var organizationId = filter.OrganizationId;
        var changedUserId = ResolveChangedUserId(filter);
        var fromDate = filter.FromDate;
        var toDate = filter.ToDate;
        var scopedOrganizationId = _userContext.OrganizationId;
        var allowedOrganizationIds = _userContext.AllowedOrganizationIds.ToArray();
        var isGlobalScope = scope == AuditLogQueryScope.Global;
        var hasSearchFilter = !string.IsNullOrWhiteSpace(searchText);

        return log =>
            (!organizationId.HasValue || log.OrganizationId == organizationId.Value)
            && (!changedUserId.HasValue || log.ChangedUserId == changedUserId.Value)
            && (!hasTableNameFilter || log.TableName.ToLower().Contains(tableName!))
            && (!hasRecordIdFilter || (log.RecordId != null && log.RecordId.ToLower().Contains(recordId!)))
            && (!hasActionFilter || log.Action.ToLower() == action!)
            && (!hasSearchFilter || MatchesSearch(log, searchText!))
            && (!fromDate.HasValue || log.ChangedDate >= fromDate.Value)
            && (!toDate.HasValue || log.ChangedDate <= toDate.Value)
            && (isGlobalScope
                || (log.OrganizationId.HasValue
                    && (scopedOrganizationId.HasValue
                        ? log.OrganizationId.Value == scopedOrganizationId.Value
                        : allowedOrganizationIds.Contains(log.OrganizationId.Value))));
    }

    private async Task<List<AuditLogQueryItem>> EnrichAsync(
        List<AuditLog> logs,
        bool includeDisplayNames,
        CancellationToken ct)
    {
        if (logs.Count == 0)
            return [];

        if (!includeDisplayNames)
            return logs.Select(log => CreateItem(log, null, null)).ToList();

        var userIds = logs
            .Where(log => log.ChangedUserId.HasValue)
            .Select(log => log.ChangedUserId!.Value)
            .Distinct()
            .ToArray();

        var organizationIds = logs
            .Where(log => log.OrganizationId.HasValue)
            .Select(log => log.OrganizationId!.Value)
            .Distinct()
            .ToArray();

        var users = userIds.Length == 0
            ? new List<User>()
            : await _userQuery.GetAllAsync(new QuerySpecification<User>
            {
                Criteria = user => userIds.Contains(user.Id)
            }, ct);

        var organizations = organizationIds.Length == 0
            ? new List<Organization>()
            : await _organizationQuery.GetAllAsync(new QuerySpecification<Organization>
            {
                Criteria = organization => organizationIds.Contains(organization.Id)
            }, ct);

        var userNames = users.ToDictionary(user => user.Id, user => user.UserName);
        var organizationNames = organizations.ToDictionary(organization => organization.Id, organization => organization.ShortName);

        return logs.Select(log =>
                CreateItem(
                    log,
                    log.OrganizationId.HasValue && organizationNames.TryGetValue(log.OrganizationId.Value, out var organizationName)
                        ? organizationName
                        : null,
                    log.ChangedUserId.HasValue && userNames.TryGetValue(log.ChangedUserId.Value, out var userName)
                        ? userName
                        : null))
            .ToList();
    }

    private static AuditLogQueryPagination PreparePagination(AuditLogQueryPagination pagination) =>
        new()
        {
            Page = Math.Max(pagination.Page, 1),
            PageSize = pagination.PageSize > 0 ? pagination.PageSize : 50
        };

    private static int? ResolveChangedUserId(AuditLogQueryFilter filter) =>
        filter.UserId ?? filter.ChangedUserId;

    private static string? ResolveEntityType(AuditLogQueryFilter filter) =>
        string.IsNullOrWhiteSpace(filter.EntityType) ? filter.TableName : filter.EntityType;

    private static string? ResolveEntityId(AuditLogQueryFilter filter) =>
        string.IsNullOrWhiteSpace(filter.EntityId) ? filter.RecordId : filter.EntityId;

    private static bool MatchesSearch(AuditLog log, string searchText) =>
        log.TableName.ToLower().Contains(searchText)
        || log.Action.ToLower().Contains(searchText)
        || (log.RecordId != null && log.RecordId.ToLower().Contains(searchText))
        || (log.RequestId != null && log.RequestId.ToLower().Contains(searchText))
        || (log.ApplicationName != null && log.ApplicationName.ToLower().Contains(searchText));

    private static AuditLogQueryItem CreateItem(
        AuditLog log,
        string? organizationName,
        string? changedUserName) =>
        new()
        {
            Id = log.Id,
            OrganizationId = log.OrganizationId,
            OrganizationName = organizationName,
            SchemaName = log.SchemaName,
            TableName = log.TableName,
            RecordId = log.RecordId,
            Action = log.Action,
            OldData = log.OldData,
            NewData = log.NewData,
            ChangedUserId = log.ChangedUserId,
            ChangedUserName = changedUserName,
            RequestId = log.RequestId,
            ClientAddr = log.ClientAddr?.ToString(),
            ApplicationName = log.ApplicationName,
            ChangedDate = log.ChangedDate
        };
}
