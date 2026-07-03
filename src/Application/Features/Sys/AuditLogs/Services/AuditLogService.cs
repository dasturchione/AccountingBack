using System.Text.Json;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.AuditLogs;

public class AuditLogService : IAuditLogService
{
    private string? _oldValues;
    private string? _newValues;
    private readonly IUserContext _userContext;
    private readonly ICommandRepository<AuditLog> _command;
    private readonly IQueryRepository<AuditLog> _query;

    public AuditLogService(IUserContext userContext,
                           ICommandRepository<AuditLog> command,
                           IQueryRepository<AuditLog> query)
    {
        _userContext = userContext;
        _command = command;
        _query = query;
    }

    public async Task CreateAsync(string tableName, string recordId, string operationType, string? comment = null)
    {
        if (operationType == AuditLogOperationTypeConst.Create && string.IsNullOrEmpty(_newValues))
            throw new ArgumentException("Yaratish amali uchun yangi qiymatlar kiritilishi shart.");

        if (operationType == AuditLogOperationTypeConst.Update && string.IsNullOrEmpty(_oldValues) && string.IsNullOrEmpty(_newValues))
            throw new ArgumentException("Tahrirlash amali uchun eski yoki yangi qiymatlardan kamida biri berilishi shart.");

        if (operationType == AuditLogOperationTypeConst.Delete && string.IsNullOrEmpty(_oldValues))
            throw new ArgumentException("O'chirish amali uchun eski qiymatlar berilishi shart.");

        var entity = new AuditLog
        {
            OrganizationId = ResolveOrganizationId(),
            SchemaName = "public",
            TableName = tableName,
            RecordId = recordId,
            Action = operationType,
            OldData = _oldValues,
            NewData = _newValues,
            ChangedUserId = _userContext.Id,
            ChangedDate = DateTime.Now
        };

        await _command.CreateAsync(entity);

        _oldValues = null;
        _newValues = null;
    }

    public void SetNewValues(object newValues)
    {
        _newValues = JsonSerializer.Serialize(newValues, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    public void SetOldValues(object oldValues)
    {
        _oldValues = JsonSerializer.Serialize(oldValues, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    public async Task<List<AuditLogDto>> GetByRecordAsync(AuditLogFilter filter)
    {
        if (!_userContext.HasGlobalAccess && _userContext.AllowedOrganizationIds.Count == 0)
            return [];

        var spec = new SharedKernel.Query.Specifications.QuerySpecification<AuditLog, AuditLogDto>
        {
            Criteria = a => a.RecordId == filter.RecordId &&
                            a.TableName == filter.TableName &&
                            (_userContext.HasGlobalAccess ||
                             (a.OrganizationId.HasValue &&
                              (_userContext.OrganizationId.HasValue
                                  ? a.OrganizationId.Value == _userContext.OrganizationId.Value
                                  : _userContext.AllowedOrganizationIds.Contains(a.OrganizationId.Value)))),
            OrderBy = q => q.OrderByDescending(a => a.ChangedDate),
            Selector = a => new AuditLogDto
            {
                Id = a.Id,
                OrganizationId = a.OrganizationId,
                SchemaName = a.SchemaName,
                TableName = a.TableName,
                RecordId = a.RecordId,
                Action = a.Action,
                OldData = a.OldData,
                NewData = a.NewData,
                ChangedUserId = a.ChangedUserId,
                ChangedDate = a.ChangedDate
            }
        };

        var logs = await _query.GetAllAsync(spec);

        return logs.Select(s =>
        {
            var changes = new List<ChangeResult>();
            if (s.OldData is not null && s.NewData is not null)
                changes = DeepCompareJson(s.OldData, s.NewData);

            s.ChangeResults = changes;
            return s;
        }).ToList();
    }

    private int? ResolveOrganizationId()
    {
        if (_userContext.OrganizationId.HasValue)
            return _userContext.OrganizationId.Value;

        if (_userContext.HasGlobalAccess)
            return null;

        return _userContext.AllowedOrganizationIds.Count == 1
            ? _userContext.AllowedOrganizationIds[0]
            : null;
    }

    public static List<ChangeResult> DeepCompareJson(string oldJson, string newJson)
    {
        var changes = new List<ChangeResult>();
        using var oldDoc = JsonDocument.Parse(oldJson);
        using var newDoc = JsonDocument.Parse(newJson);
        CompareElements(oldDoc.RootElement, newDoc.RootElement, "", changes);
        return changes;
    }

    private static void CompareElements(JsonElement oldElement, JsonElement newElement, string path, List<ChangeResult> changes)
    {
        if (oldElement.ValueKind != newElement.ValueKind)
        {
            changes.Add(new ChangeResult { Path = path, OldValue = oldElement.ToString(), NewValue = newElement.ToString() });
            return;
        }

        switch (oldElement.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var oldProp in oldElement.EnumerateObject())
                {
                    var newPath = string.IsNullOrEmpty(path) ? oldProp.Name : $"{path}.{oldProp.Name}";
                    if (!newElement.TryGetProperty(oldProp.Name, out var newProp))
                    {
                        changes.Add(new ChangeResult { Path = newPath, OldValue = oldProp.Value.ToString(), NewValue = null });
                        continue;
                    }
                    CompareElements(oldProp.Value, newProp, newPath, changes);
                }
                foreach (var newProp in newElement.EnumerateObject())
                {
                    if (!oldElement.TryGetProperty(newProp.Name, out _))
                    {
                        var newPath = string.IsNullOrEmpty(path) ? newProp.Name : $"{path}.{newProp.Name}";
                        changes.Add(new ChangeResult { Path = newPath, OldValue = null, NewValue = newProp.Value.ToString() });
                    }
                }
                break;

            case JsonValueKind.Array:
                var oldList = oldElement.EnumerateArray().ToList();
                var newList = newElement.EnumerateArray().ToList();
                int max = Math.Max(oldList.Count, newList.Count);
                for (int i = 0; i < max; i++)
                {
                    var oldItem = i < oldList.Count ? oldList[i] : default;
                    var newItem = i < newList.Count ? newList[i] : default;
                    var itemPath = $"{path}[{i}]";
                    if (oldItem.ValueKind == JsonValueKind.Undefined || newItem.ValueKind == JsonValueKind.Undefined)
                    {
                        changes.Add(new ChangeResult
                        {
                            Path = itemPath,
                            OldValue = oldItem.ValueKind == JsonValueKind.Undefined ? null : oldItem.ToString(),
                            NewValue = newItem.ValueKind == JsonValueKind.Undefined ? null : newItem.ToString()
                        });
                    }
                    else
                    {
                        CompareElements(oldItem, newItem, itemPath, changes);
                    }
                }
                break;

            default:
                if (oldElement.ToString() != newElement.ToString())
                    changes.Add(new ChangeResult { Path = path, OldValue = oldElement.ToString(), NewValue = newElement.ToString() });
                break;
        }
    }
}
