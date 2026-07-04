using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query.Specifications;

namespace Application.Features.Register.PostingEngines;

public sealed class OrganizationAccountingPolicyResolver : IOrganizationAccountingPolicyResolver
{
    private readonly IQueryRepository<OrganizationConfig> _organizationConfigQuery;

    public OrganizationAccountingPolicyResolver(IQueryRepository<OrganizationConfig> organizationConfigQuery)
    {
        _organizationConfigQuery = organizationConfigQuery;
    }

    public async Task<short> ResolveAsync(int organizationId, CancellationToken ct = default)
    {
        var config = await _organizationConfigQuery.GetAsync(
            new QuerySpecification<OrganizationConfig>
            {
                Criteria = x => x.OrganizationId == organizationId
            },
            ct);

        return config?.AccountingPolicyId ?? AccountingPolicyIdConst.STANDARD_UZ;
    }
}
