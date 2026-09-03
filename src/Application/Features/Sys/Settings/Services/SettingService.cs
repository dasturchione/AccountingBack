using System.Text.Json;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features;
using Application.Features.Platform;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Settings;

public sealed class SettingService : BaseService, ISettingService
{
    private const short StringValueType = 0;
    private const short IntValueType = 1;
    private const short BoolValueType = 2;
    private const short JsonValueType = 3;

    private readonly IUserContext _userContext;
    private readonly IQueryRepository<SystemSetting> _query;
    private readonly ICommandRepository<SystemSetting> _command;
    private readonly IQueryBuilder _queryBuilder;

    public SettingService(
        IUserContext userContext,
        IQueryRepository<SystemSetting> query,
        ICommandRepository<SystemSetting> command,
        IQueryBuilder queryBuilder,
        ILogger<SettingService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _query = query;
        _command = command;
        _queryBuilder = queryBuilder;
    }

    public Task<Result<string?>> GetValueAsync(string code, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetValueAsync), async () =>
        {
            var settingResult = await GetSettingAsync(code, ct);
            return !settingResult.IsSuccess
                ? Result.Failure<string?>(settingResult.Error)
                : Result.Success(settingResult.Value.Value);
        });

    public Task<Result<T>> GetValueAsync<T>(string code, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetValueAsync), async () =>
        {
            var settingResult = await GetSettingAsync(code, ct);
            if (!settingResult.IsSuccess)
                return Result.Failure<T>(settingResult.Error);

            return ParseValue<T>(settingResult.Value);
        });

    public Task<Result<SettingDto>> GetAsync(string code, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAsync), async () =>
        {
            var settingResult = await GetSettingAsync(code, ct);
            return !settingResult.IsSuccess
                ? Result.Failure<SettingDto>(settingResult.Error)
                : Result.Success(Map(settingResult.Value));
        });

    public Task<Result<List<SettingDto>>> GetAllAsync(string? category, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
                return Result.Failure<List<SettingDto>>(PlatformErrors.GlobalAccessRequired(_userContext.LanguageId));

            var normalizedCategory = NormalizeCategory(category);
            var items = await _query.GetAllAsync(_queryBuilder.For<SystemSetting>()
                .Where(x =>
                    x.StateId == StateIdConst.ACTIVE &&
                    x.OrganizationId == null &&
                    (normalizedCategory == null || (x.Category != null && x.Category.ToLower() == normalizedCategory)))
                .OrderBy(q => q.OrderBy(x => x.Category).ThenBy(x => x.Code))
                .Build(), ct);

            return Result.Success(items.Select(Map).ToList());
        });

    public Task<Result> UpdateAsync(string code, string? value, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            var settingResult = await GetSettingAsync(code, ct);
            if (!settingResult.IsSuccess)
                return Result.Failure(settingResult.Error);

            var setting = settingResult.Value;
            if (!setting.IsEditable)
                return Result.Failure(SettingErrors.ReadOnly(setting.Code, _userContext.LanguageId));

            var validationError = ValidateValue(setting.Code, setting.ValueType, value);
            if (validationError is not null)
                return Result.Failure(validationError);

            setting.Value = value;
            setting.UpdatedDate = DateTime.Now;

            await _command.UpdateAsync(setting, ct);
            return Result.Success();
        }, ct);

    private async Task<Result<SystemSetting>> GetSettingAsync(string code, CancellationToken ct)
    {
        if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
            return Result.Failure<SystemSetting>(PlatformErrors.GlobalAccessRequired(_userContext.LanguageId));

        var normalizedCode = NormalizeCode(code);
        var setting = await _query.GetAsync(_queryBuilder.For<SystemSetting>()
            .Where(x => x.StateId == StateIdConst.ACTIVE &&
                        x.OrganizationId == null &&
                        x.Code == normalizedCode)
            .Build(), ct);

        return setting is null
            ? Result.Failure<SystemSetting>(SettingErrors.NotFound(normalizedCode, _userContext.LanguageId))
            : Result.Success(setting);
    }

    private static SettingDto Map(SystemSetting setting) =>
        new()
        {
            Code = setting.Code,
            Value = setting.Value,
            ValueType = setting.ValueType,
            Category = setting.Category,
            Description = setting.Description,
            IsEditable = setting.IsEditable
        };

    private Result<T> ParseValue<T>(SystemSetting setting)
    {
        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        switch (setting.ValueType)
        {
            case StringValueType:
                if (targetType != typeof(string))
                    return Result.Failure<T>(SettingErrors.TypeMismatch(setting.Code, typeof(T).Name, _userContext.LanguageId));

                return Result.Success((T)(object?)setting.Value!);

            case IntValueType:
                if (targetType != typeof(int))
                    return Result.Failure<T>(SettingErrors.TypeMismatch(setting.Code, typeof(T).Name, _userContext.LanguageId));

                if (!int.TryParse(setting.Value, out var intValue))
                    return Result.Failure<T>(SettingErrors.InvalidValue(setting.Code, _userContext.LanguageId));

                return Result.Success(CastValue<T, int>(intValue));

            case BoolValueType:
                if (targetType != typeof(bool))
                    return Result.Failure<T>(SettingErrors.TypeMismatch(setting.Code, typeof(T).Name, _userContext.LanguageId));

                if (!bool.TryParse(setting.Value, out var boolValue))
                    return Result.Failure<T>(SettingErrors.InvalidValue(setting.Code, _userContext.LanguageId));

                return Result.Success(CastValue<T, bool>(boolValue));

            case JsonValueType:
                if (string.IsNullOrWhiteSpace(setting.Value))
                    return Result.Failure<T>(SettingErrors.InvalidValue(setting.Code, _userContext.LanguageId));

                try
                {
                    if (targetType == typeof(JsonDocument))
                        return Result.Success((T)(object)JsonDocument.Parse(setting.Value));

                    var deserialized = JsonSerializer.Deserialize<T>(setting.Value);
                    return deserialized is null
                        ? Result.Failure<T>(SettingErrors.InvalidValue(setting.Code, _userContext.LanguageId))
                        : Result.Success(deserialized);
                }
                catch (JsonException)
                {
                    return Result.Failure<T>(SettingErrors.InvalidValue(setting.Code, _userContext.LanguageId));
                }

            default:
                return Result.Failure<T>(SettingErrors.UnsupportedValueType(setting.Code, setting.ValueType, _userContext.LanguageId));
        }
    }

    private static T CastValue<T, TValue>(TValue value)
    {
        if (typeof(T) == typeof(TValue))
            return (T)(object)value!;

        var nullableType = typeof(Nullable<>).MakeGenericType(typeof(TValue));
        if (typeof(T) == nullableType)
            return (T)Activator.CreateInstance(nullableType, value)!;

        throw new InvalidCastException($"Cannot cast setting value to {typeof(T).Name}.");
    }

    private Error? ValidateValue(string code, short valueType, string? value)
    {
        if (value is null)
            return SettingErrors.InvalidValue(code, _userContext.LanguageId);

        return valueType switch
        {
            StringValueType => null,
            IntValueType => int.TryParse(value, out _) ? null : SettingErrors.InvalidValue(code, _userContext.LanguageId),
            BoolValueType => bool.TryParse(value, out _) ? null : SettingErrors.InvalidValue(code, _userContext.LanguageId),
            JsonValueType => IsValidJson(value) ? null : SettingErrors.InvalidValue(code, _userContext.LanguageId),
            _ => SettingErrors.UnsupportedValueType(code, valueType, _userContext.LanguageId)
        };
    }

    private static bool IsValidJson(string value)
    {
        try
        {
            JsonDocument.Parse(value);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();

    private static string? NormalizeCategory(string? category) =>
        string.IsNullOrWhiteSpace(category) ? null : category.Trim().ToLowerInvariant();
}
