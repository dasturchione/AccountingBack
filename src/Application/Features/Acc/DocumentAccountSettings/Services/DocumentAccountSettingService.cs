using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Manual;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Acc.DocumentAccountSettings;

public class DocumentAccountSettingService : IDocumentAccountSettingService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<DocumentAccountType> _typeQuery;
    private readonly IQueryRepository<DocumentAccountTypeRole> _typeRoleQuery;
    private readonly IQueryRepository<DocumentAccountSetting> _settingQuery;
    private readonly IQueryRepository<ChartAccount> _chartAccountQuery;
    private readonly ICommandRepository<DocumentAccountSetting> _settingCommand;

    public DocumentAccountSettingService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<DocumentAccountType> typeQuery,
        IQueryRepository<DocumentAccountTypeRole> typeRoleQuery,
        IQueryRepository<DocumentAccountSetting> settingQuery,
        IQueryRepository<ChartAccount> chartAccountQuery,
        ICommandRepository<DocumentAccountSetting> settingCommand)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _typeQuery = typeQuery;
        _typeRoleQuery = typeRoleQuery;
        _settingQuery = settingQuery;
        _chartAccountQuery = chartAccountQuery;
        _settingCommand = settingCommand;
    }

    public async Task<Result<PagedResponse<DocumentAccountSettingListDto>>> GetAllAsync(DocumentAccountSettingListFilter filter, CancellationToken ct = default)
    {
        var languageId = _userContext.LanguageId ?? LanguageIdConst.UZ;
        var page = Math.Max(filter.Page, 1);
        var take = Math.Max(filter.PageSize.GetValueOrDefault(50), 1);

        var query = _queryBuilder.For<DocumentAccountType>()
            .Where(x => x.StateId == StateIdConst.ACTIVE)
            .As(x => new DocumentAccountSettingListDto
            {
                DocumentTypeId = x.Id,
                DocumentTypeCode = x.Code,
                DocumentTypeName = x.DocumentAccountTypeTranslations
                    .Where(t => t.LanguageId == languageId)
                    .Select(t => t.Name)
                    .FirstOrDefault() ?? x.Name,
                DocumentTypeDescription = x.DocumentAccountTypeTranslations
                    .Where(t => t.LanguageId == languageId)
                    .Select(t => t.Description)
                    .FirstOrDefault() ?? x.Description ?? string.Empty,
                StateId = x.StateId,
                StateName = x.State.ShortName
            })
            .OrderBy(x => x.OrderBy(y => y.DocumentTypeId))
            .Take(take)
            .Skip((page - 1) * take)
            .BuildPaged();

        var pagedList = await _typeQuery.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, page, take);
    }
    
    public async Task<Result<DocumentAccountSettingDto>> GetByDocumentTypeIdAsync(short documentAccountTypeId, CancellationToken cancellationToken = default)
    {
        if (_userContext.OrganizationId is null)
        {
            return Result.Failure<DocumentAccountSettingDto>(
                CommonErrors.UserHasNoOrganization(_userContext.LanguageId));
        }

        var organizationId = _userContext.OrganizationId.Value;
        var languageId = _userContext.LanguageId ?? LanguageIdConst.UZ;

        var documentTypeRoleQuery = _queryBuilder
            .For<DocumentAccountTypeRole>()
            .Where(x =>
                x.DocumentAccountTypeId == documentAccountTypeId &&
                x.DocumentAccountType.StateId == StateIdConst.ACTIVE &&
                x.DocumentAccountRole.StateId == StateIdConst.ACTIVE)
            .As(x => new
            {
                DocumentAccountTypeRoleId = x.Id,

                DocumentAccountRoleId = x.DocumentAccountRoleId,
                DocumentAccountRoleCode = x.DocumentAccountRole.Code,
                DocumentAccountRoleName = x.DocumentAccountRole
                    .DocumentAccountRoleTranslations
                    .Where(translation => translation.LanguageId == languageId)
                    .Select(translation => translation.Name)
                    .FirstOrDefault() ?? x.DocumentAccountRole.Name,

                DocumentAccountRoleDescription = x.DocumentAccountRole
                    .DocumentAccountRoleTranslations
                    .Where(translation => translation.LanguageId == languageId)
                    .Select(translation => translation.Description)
                    .FirstOrDefault() ?? x.DocumentAccountRole.Description,

                DocumentAccountTypeId = x.DocumentAccountTypeId,
                DocumentAccountTypeCode = x.DocumentAccountType.Code,
                DocumentAccountTypeName = x.DocumentAccountType
                    .DocumentAccountTypeTranslations
                    .Where(translation => translation.LanguageId == languageId)
                    .Select(translation => translation.Name)
                    .FirstOrDefault() ?? x.DocumentAccountType.Name,

                DocumentAccountTypeDescription = x.DocumentAccountType
                    .DocumentAccountTypeTranslations
                    .Where(translation => translation.LanguageId == languageId)
                    .Select(translation => translation.Description)
                    .FirstOrDefault() ?? x.DocumentAccountType.Description,

                x.AccountSide,
                x.IsRequired,
                x.SortOrder
            })
            .OrderBy(x => x.SortOrder)
            .Build();

        var documentTypeRoles = await _typeRoleQuery.GetAllAsync(documentTypeRoleQuery, cancellationToken);

        if (!documentTypeRoles.Any())
            return Result.Failure<DocumentAccountSettingDto>(DocumentAccountSettingErrors.TypeRoleNotFound(documentAccountTypeId, _userContext.LanguageId));

        var documentTypeRoleIds = documentTypeRoles
            .Select(x => x.DocumentAccountTypeRoleId)
            .ToList();

        var documentAccountSettingQuery = _queryBuilder
            .For<DocumentAccountSetting>()
            .Where(x =>
                x.OrganizationId == organizationId &&
                x.StateId == StateIdConst.ACTIVE &&
                documentTypeRoleIds.Contains(x.DocumentAccountTypeRoleId))
            .As(x => new
            {
                DocumentAccountSettingId = x.Id,
                x.OrganizationId,
                x.DocumentAccountTypeRoleId,
                x.ChartAccountId,
                x.IsDefault,
                x.CanChange,
                x.SortOrder,
                x.StateId,

                StateName = x.State.ShortName,
                ChartAccountName = x.ChartAccount.Name,
                ChartAccountCode = x.ChartAccount.Code,
                ChartAccountNumber = x.ChartAccount.Number
            })
            .Build();

        var documentAccountSettings = await _settingQuery.GetAllAsync(documentAccountSettingQuery, cancellationToken);

        var documentAccountSetting = documentTypeRoles
            .GroupBy(x => new
            {
                x.DocumentAccountTypeId,
                x.DocumentAccountTypeCode,
                x.DocumentAccountTypeName,
                x.DocumentAccountTypeDescription
            })
            .Select(documentTypeGroup => new DocumentAccountSettingDto
            {
                DocumentTypeId = documentTypeGroup.Key.DocumentAccountTypeId,
                DocumentTypeCode = documentTypeGroup.Key.DocumentAccountTypeCode,
                DocumentTypeName = documentTypeGroup.Key.DocumentAccountTypeName,
                DocumentTypeDescription =
                    documentTypeGroup.Key.DocumentAccountTypeDescription ?? string.Empty,

                AccountSettings = documentTypeGroup
                    .Select(documentTypeRole =>
                    {
                        var accountOptions = documentAccountSettings
                            .Where(setting =>
                                setting.DocumentAccountTypeRoleId ==
                                documentTypeRole.DocumentAccountTypeRoleId)
                            .OrderBy(setting => setting.SortOrder)
                            .Select(setting => new DocumentAccountOptionDto
                            {
                                Id = setting.DocumentAccountSettingId,
                                OrganizationId = setting.OrganizationId,

                                ChartAccountId = setting.ChartAccountId,
                                ChartAccountName = setting.ChartAccountName,
                                ChartAccountCode =
                                    setting.ChartAccountCode ?? string.Empty,
                                ChartAccountNumber = setting.ChartAccountNumber,

                                IsDefault = setting.IsDefault,
                                CanChange = setting.CanChange,
                                SortOrder = setting.SortOrder,

                                StateId = setting.StateId,
                                StateName = setting.StateName
                            })
                            .ToList();

                        return new DocumentAccountRuleDto
                        {
                            DocumentAccountTypeRoleId =
                                documentTypeRole.DocumentAccountTypeRoleId,

                            DocumentAccountRoleId =
                                documentTypeRole.DocumentAccountRoleId,
                            DocumentAccountRoleCode =
                                documentTypeRole.DocumentAccountRoleCode,
                            DocumentAccountRoleName =
                                documentTypeRole.DocumentAccountRoleName,
                            DocumentAccountRoleDescription =
                                documentTypeRole.DocumentAccountRoleDescription ??
                                string.Empty,

                            AccountSide = documentTypeRole.AccountSide,
                            IsRequired = documentTypeRole.IsRequired,
                            SortOrder = documentTypeRole.SortOrder,
                            Accounts = accountOptions
                        };
                    })
                    .OrderBy(accountRule => accountRule.SortOrder)
                    .ToList()
            })
            .First();

        return documentAccountSetting;
    }

    public async Task<Result<List<ChartAccountSelectListDto>>> GetSelectListAsync(short documentTypeId, short? documentRoleId, string? documentRoleCode, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<List<ChartAccountSelectListDto>>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var organizationId = _userContext.OrganizationId.Value;
        var roleCode = string.IsNullOrWhiteSpace(documentRoleCode) ? null : documentRoleCode.Trim();
        var hasRoleId = documentRoleId.HasValue && documentRoleId.Value > 0;

        if (!hasRoleId && roleCode is null)
            return Result.Failure<List<ChartAccountSelectListDto>>(DocumentAccountSettingErrors.RoleFilterRequired(_userContext.LanguageId));

        var roleId = hasRoleId ? documentRoleId!.Value : default;
        var documentTypeRoleQuery = _queryBuilder
            .For<DocumentAccountTypeRole>()
            .Where(x =>
                x.DocumentAccountTypeId == documentTypeId &&
                x.DocumentAccountType.StateId == StateIdConst.ACTIVE &&
                x.DocumentAccountRole.StateId == StateIdConst.ACTIVE &&
                ((hasRoleId && x.DocumentAccountRoleId == roleId) ||
                 (!hasRoleId && x.DocumentAccountRole.Code == roleCode)))
            .As(x => new
            {
                x.Id
            })
            .Build();

        var documentTypeRole = await _typeRoleQuery.GetAsync(documentTypeRoleQuery, ct);
        if (documentTypeRole is null)
            return Result.Failure<List<ChartAccountSelectListDto>>(
                DocumentAccountSettingErrors.TypeRoleNotFound(documentTypeId, hasRoleId ? roleId : null, roleCode, _userContext.LanguageId));

        var documentAccountSettingQuery = _queryBuilder
            .For<DocumentAccountSetting>()
            .Where(x =>
                x.OrganizationId == organizationId &&
                x.DocumentAccountTypeRoleId == documentTypeRole.Id &&
                x.StateId == StateIdConst.ACTIVE &&
                x.ChartAccount.StateId == StateIdConst.ACTIVE)
            .As(x => new
            {
                x.ChartAccountId,
                ChartAccountName = x.ChartAccount.Name,
                ChartAccountCode = x.ChartAccount.Code,
                ChartAccountNumber = x.ChartAccount.Number,
                x.SortOrder
            })
            .Build();

        var settings = await _settingQuery.GetAllAsync(documentAccountSettingQuery, ct);

        return settings
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.ChartAccountNumber)
            .Select(x => new ChartAccountSelectListDto
            {
                Id = x.ChartAccountId,
                Name = x.ChartAccountName,
                Code = x.ChartAccountCode,
                Number = x.ChartAccountNumber
            })
            .ToList();
    }

    public async Task<Result<List<long>>> SaveAsync(DocumentAccountRuleSettingSaveDto dto, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<List<long>>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var organizationId = _userContext.OrganizationId.Value;
        var now = DateTime.Now;
        var items = dto.Items ?? [];

        if (items.Count == 0)
            return new List<long>();

        var duplicateTypeRoleExists = items
            .GroupBy(x => x.DocumentAccountTypeRoleId)
            .Any(x => x.Count() > 1);
        if (duplicateTypeRoleExists)
            return Result.Failure<List<long>>(DocumentAccountSettingErrors.DuplicateTypeRoles(_userContext.LanguageId));

        foreach (var item in items)
        {
            var itemAccounts = item.Accounts ?? [];

            var duplicateAccountExists = itemAccounts
                .GroupBy(x => x.ChartAccountId)
                .Any(x => x.Count() > 1);
            if (duplicateAccountExists)
                return Result.Failure<List<long>>(DocumentAccountSettingErrors.DuplicateAccounts(_userContext.LanguageId));

            if (itemAccounts.Count(x => x.IsDefault) > 1)
                return Result.Failure<List<long>>(DocumentAccountSettingErrors.MultipleDefaults(_userContext.LanguageId));
        }

        var documentAccountTypeRoleIds = items
            .Select(x => x.DocumentAccountTypeRoleId)
            .Distinct()
            .ToList();

        var typeRoleQuery = _queryBuilder.For<DocumentAccountTypeRole>()
            .Where(x => documentAccountTypeRoleIds.Contains(x.Id))
            .As(x => x.Id)
            .Build();
        var existingTypeRoleIds = await _typeRoleQuery.GetAllAsync(typeRoleQuery, ct);
        var missingTypeRoleIds = documentAccountTypeRoleIds.Except(existingTypeRoleIds).ToList();
        if (missingTypeRoleIds.Count > 0)
            return Result.Failure<List<long>>(DocumentAccountSettingErrors.TypeRoleNotFound(missingTypeRoleIds[0], _userContext.LanguageId));

        var chartAccountIds = items
            .SelectMany(x => x.Accounts ?? [])
            .Select(x => x.ChartAccountId)
            .Distinct()
            .ToList();

        if (chartAccountIds.Count > 0)
        {
            var chartAccountQuery = _queryBuilder.For<ChartAccount>()
                .Where(x => x.OrganizationId == organizationId &&
                            x.StateId == StateIdConst.ACTIVE &&
                            chartAccountIds.Contains(x.Id))
                .As(x => x.Id)
                .Build();
            var existingChartAccountIds = await _chartAccountQuery.GetAllAsync(chartAccountQuery, ct);
            var missingChartAccountIds = chartAccountIds.Except(existingChartAccountIds).ToList();
            if (missingChartAccountIds.Count > 0)
                return Result.Failure<List<long>>(DocumentAccountSettingErrors.ChartAccountsNotFound(missingChartAccountIds, _userContext.LanguageId));
        }

        foreach (var item in items.Where(x => (x.Accounts ?? []).Any(account => account.IsDefault)))
            await ResetExistingDefaultsAsync(organizationId, item.DocumentAccountTypeRoleId, ct);

        var existingQuery = _queryBuilder.For<DocumentAccountSetting>()
            .Where(x => x.OrganizationId == organizationId &&
                        documentAccountTypeRoleIds.Contains(x.DocumentAccountTypeRoleId))
            .Build();
        var existingSettings = await _settingQuery.GetAllAsync(existingQuery, ct);
        var existingByRoleId = existingSettings
            .GroupBy(x => x.DocumentAccountTypeRoleId)
            .ToDictionary(x => x.Key, x => x.ToList());

        var toCreate = new List<DocumentAccountSetting>();
        var toUpdate = new List<DocumentAccountSetting>();
        var ids = new List<long>();

        foreach (var item in items)
        {
            var accounts = item.Accounts ?? [];
            var existingForRole = existingByRoleId.GetValueOrDefault(item.DocumentAccountTypeRoleId) ?? [];
            var existingByChartAccountId = existingForRole.ToDictionary(x => x.ChartAccountId);
            var requestedChartAccountIds = accounts
                .Select(x => x.ChartAccountId)
                .ToHashSet();

            foreach (var existing in existingForRole.Where(x => !requestedChartAccountIds.Contains(x.ChartAccountId)))
            {
                if (existing.StateId == StateIdConst.PASSIVE && !existing.IsDefault)
                    continue;

                existing.StateId = StateIdConst.PASSIVE;
                existing.IsDefault = false;
                existing.UpdatedDate = now;
                toUpdate.Add(existing);
            }

            foreach (var account in accounts.OrderBy(x => x.SortOrder))
            {
                if (existingByChartAccountId.TryGetValue(account.ChartAccountId, out var existing))
                {
                    existing.IsDefault = account.IsDefault;
                    existing.CanChange = account.CanChange;
                    existing.SortOrder = Math.Max(1, account.SortOrder);
                    existing.StateId = StateIdConst.ACTIVE;
                    existing.UpdatedDate = now;
                    toUpdate.Add(existing);
                    ids.Add(existing.Id);
                    continue;
                }

                var entity = new DocumentAccountSetting
                {
                    OrganizationId = organizationId,
                    DocumentAccountTypeRoleId = item.DocumentAccountTypeRoleId,
                    ChartAccountId = account.ChartAccountId,
                    IsDefault = account.IsDefault,
                    CanChange = account.CanChange,
                    SortOrder = Math.Max(1, account.SortOrder),
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = now
                };
                toCreate.Add(entity);
            }
        }

        if (toUpdate.Count > 0)
            await _settingCommand.UpdateAsync(toUpdate, ct);

        if (toCreate.Count > 0)
        {
            await _settingCommand.CreateAsync(toCreate, ct);
            ids.AddRange(toCreate.Select(x => x.Id));
        }

        return ids;
    }

    private async Task ResetExistingDefaultsAsync(int organizationId, int documentAccountTypeRoleId, CancellationToken ct)
    {
        var existingDefaultsQuery = _queryBuilder.For<DocumentAccountSetting>()
            .Where(x => x.OrganizationId == organizationId &&
                        x.DocumentAccountTypeRoleId == documentAccountTypeRoleId &&
                        x.StateId == StateIdConst.ACTIVE &&
                        x.IsDefault)
            .Build();

        var existingDefaults = await _settingQuery.GetAllAsync(existingDefaultsQuery, ct);
        if (existingDefaults.Count == 0)
            return;

        var now = DateTime.Now;
        foreach (var setting in existingDefaults)
        {
            setting.IsDefault = false;
            setting.UpdatedDate = now;
        }

        await _settingCommand.UpdateAsync(existingDefaults, ct);
    }
}
