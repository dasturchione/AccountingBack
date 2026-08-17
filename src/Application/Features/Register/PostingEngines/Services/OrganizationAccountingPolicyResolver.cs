using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;

namespace Application.Features.Register.PostingEngines;

public sealed class OrganizationAccountingPolicyResolver : IOrganizationAccountingPolicyResolver
{
    private readonly IQueryRepository<OrganizationConfig> _organizationConfigQuery;
    private readonly IQueryBuilder _queryBuilder;

    public OrganizationAccountingPolicyResolver(
        IQueryRepository<OrganizationConfig> organizationConfigQuery,
        IQueryBuilder queryBuilder)
    {
        _organizationConfigQuery = organizationConfigQuery;
        _queryBuilder = queryBuilder;
    }

    public async Task<short> ResolveAsync(int organizationId, CancellationToken ct = default)
    {
        var config = await _organizationConfigQuery.GetAsync(_queryBuilder.For<OrganizationConfig>()
            .Where(x => x.OrganizationId == organizationId)
            .Build(), ct);

        return config?.AccountingPolicyId ?? AccountingPolicyIdConst.STANDARD_UZ;
    }
}
