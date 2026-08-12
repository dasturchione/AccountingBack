using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Fa;

public sealed record FaDocumentAccountRequirement(
    int? AccountId,
    string RoleCode,
    bool IsRequired = true);

public interface IFaDocumentAccountValidator
{
    Task<Result> ValidateAsync(
        int organizationId,
        short documentTypeId,
        IReadOnlyCollection<FaDocumentAccountRequirement> requirements,
        CancellationToken ct = default);
}

public sealed class FaDocumentAccountValidator : IFaDocumentAccountValidator
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<ChartAccount> _chartAccountQuery;
    private readonly IQueryRepository<DocumentAccountTypeRole> _typeRoleQuery;
    private readonly IQueryRepository<DocumentAccountSetting> _settingQuery;

    public FaDocumentAccountValidator(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<ChartAccount> chartAccountQuery,
        IQueryRepository<DocumentAccountTypeRole> typeRoleQuery,
        IQueryRepository<DocumentAccountSetting> settingQuery)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _chartAccountQuery = chartAccountQuery;
        _typeRoleQuery = typeRoleQuery;
        _settingQuery = settingQuery;
    }

    public async Task<Result> ValidateAsync(
        int organizationId,
        short documentTypeId,
        IReadOnlyCollection<FaDocumentAccountRequirement> requirements,
        CancellationToken ct = default)
    {
        var missing = requirements.FirstOrDefault(item => item.IsRequired && !item.AccountId.HasValue);
        if (missing is not null)
            return Result.Failure(AccountRequired(missing.RoleCode));

        var accountIds = requirements
            .Where(item => item.AccountId.HasValue)
            .Select(item => item.AccountId!.Value)
            .Distinct()
            .ToList();

        if (accountIds.Count == 0)
            return Result.Success();

        var accountQuery = _queryBuilder.For<ChartAccount>()
            .Where(account =>
                accountIds.Contains(account.Id) &&
                account.OrganizationId == organizationId &&
                account.StateId == StateIdConst.ACTIVE)
            .As(account => account.Id)
            .Build();
        var activeAccountIds = (await _chartAccountQuery.GetAllAsync(accountQuery, ct)).ToHashSet();

        var invalidAccountId = accountIds.FirstOrDefault(id => !activeAccountIds.Contains(id));
        if (invalidAccountId != 0)
            return Result.Failure(AccountUnavailable(invalidAccountId));

        var roleCodes = requirements
            .Select(item => item.RoleCode)
            .Distinct()
            .ToList();

        var accountTypeCode =
            FaDocumentAccountTypeCodeConst.FromDocumentTypeId(documentTypeId);
        if (accountTypeCode is null)
            return Result.Success();

        var typeRoleQuery = _queryBuilder.For<DocumentAccountTypeRole>()
            .Where(role =>
                role.DocumentAccountType.Code == accountTypeCode &&
                role.DocumentAccountType.StateId == StateIdConst.ACTIVE &&
                role.DocumentAccountRole.StateId == StateIdConst.ACTIVE &&
                roleCodes.Contains(role.DocumentAccountRole.Code))
            .As(role => new
            {
                role.Id,
                RoleCode = role.DocumentAccountRole.Code
            })
            .Build();
        var configuredRoles = await _typeRoleQuery.GetAllAsync(typeRoleQuery, ct);
        if (configuredRoles.Count == 0)
            return Result.Success();

        var typeRoleIds = configuredRoles.Select(role => role.Id).ToList();
        var settingQuery = _queryBuilder.For<DocumentAccountSetting>()
            .Where(setting =>
                setting.OrganizationId == organizationId &&
                typeRoleIds.Contains(setting.DocumentAccountTypeRoleId) &&
                setting.StateId == StateIdConst.ACTIVE &&
                setting.ChartAccount.StateId == StateIdConst.ACTIVE)
            .As(setting => new
            {
                setting.ChartAccountId,
                RoleCode = setting.DocumentAccountTypeRole.DocumentAccountRole.Code
            })
            .Build();
        var settings = await _settingQuery.GetAllAsync(settingQuery, ct);

        foreach (var requirement in requirements.Where(item => item.AccountId.HasValue))
        {
            if (!configuredRoles.Any(role => role.RoleCode == requirement.RoleCode))
                continue;

            var accountId = requirement.AccountId.GetValueOrDefault();
            if (!settings.Any(setting =>
                    setting.RoleCode == requirement.RoleCode &&
                    setting.ChartAccountId == accountId))
            {
                return Result.Failure(AccountNotAllowed(accountId, requirement.RoleCode));
            }
        }

        return Result.Success();
    }

    private Error AccountRequired(string roleCode) =>
        Error.Business(
            "Fa.AccountRequired",
            _userContext.LanguageId switch
            {
                LanguageIdConst.UZ => $"'{roleCode}' roli uchun hisob ko'rsatilishi shart.",
                LanguageIdConst.RU => $"Для роли '{roleCode}' необходимо указать счёт.",
                _ => $"An account is required for role '{roleCode}'."
            });

    private Error AccountUnavailable(int accountId) =>
        Error.Business(
            "Fa.AccountUnavailable",
            _userContext.LanguageId switch
            {
                LanguageIdConst.UZ => $"Id-si {accountId} bo'lgan hisob faol emas yoki joriy tashkilotga tegishli emas.",
                LanguageIdConst.RU => $"Счёт с id {accountId} неактивен или не принадлежит текущей организации.",
                _ => $"Account with id {accountId} is inactive or does not belong to the current organization."
            });

    private Error AccountNotAllowed(int accountId, string roleCode) =>
        Error.Business(
            "Fa.AccountNotAllowed",
            _userContext.LanguageId switch
            {
                LanguageIdConst.UZ => $"Id-si {accountId} bo'lgan hisob '{roleCode}' roli uchun ruxsat etilmagan.",
                LanguageIdConst.RU => $"Для роли '{roleCode}' необходимо указать счёт.",
                _ => $"Account with id {accountId} is not allowed for role '{roleCode}'."
            });
}
