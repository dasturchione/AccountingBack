using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Pay;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Pay.PayrollDocuments;

public sealed class PayrollAccountResolver : IPayrollAccountResolver
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<DocumentAccountSetting> _settingQuery;

    public PayrollAccountResolver(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<DocumentAccountSetting> settingQuery)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _settingQuery = settingQuery;
    }

    public async Task<Result<IReadOnlyDictionary<string, int>>> ResolveAsync(
        int organizationId,
        IEnumerable<string> roleCodes,
        CancellationToken ct = default)
    {
        var requestedCodes = roleCodes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (requestedCodes.Count == 0)
            return Result.Success<IReadOnlyDictionary<string, int>>(new Dictionary<string, int>());

        var query = _queryBuilder.For<DocumentAccountSetting>()
            .Where(x =>
                x.OrganizationId == organizationId &&
                x.StateId == StateIdConst.ACTIVE &&
                x.IsDefault &&
                x.DocumentAccountTypeRole.DocumentAccountType.Code == PayrollDocumentAccountTypeCodeConst.PayrollAccrual &&
                requestedCodes.Contains(x.DocumentAccountTypeRole.DocumentAccountRole.Code))
            .As(x => new
            {
                RoleCode = x.DocumentAccountTypeRole.DocumentAccountRole.Code,
                x.ChartAccountId
            })
            .Build();

        var settings = await _settingQuery.GetAllAsync(query, ct);
        var result = settings
            .GroupBy(x => x.RoleCode)
            .ToDictionary(x => x.Key, x => x.First().ChartAccountId, StringComparer.Ordinal);

        var missingRole = requestedCodes.FirstOrDefault(code => !result.ContainsKey(code));
        return missingRole is null
            ? Result.Success<IReadOnlyDictionary<string, int>>(result)
            : Result.Failure<IReadOnlyDictionary<string, int>>(PayrollErrors.AccountRoleNotConfigured(missingRole, _userContext.LanguageId));
    }
}
