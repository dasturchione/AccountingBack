using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Contracts;

public class ContractByListFilterCriteriaBuilder : ICriteriaBuilder<Contract, ContractListFilter>
{
    public Expression<Func<Contract, bool>> Build(ContractListFilter options) =>
        x => (!options.OrganizationId.HasValue || x.OrganizationId == options.OrganizationId.Value) &&
             (!options.CounterpartyId.HasValue || x.CounterpartyId == options.CounterpartyId.Value) &&
             (string.IsNullOrEmpty(options.ContractType) || x.ContractType == options.ContractType);
}
