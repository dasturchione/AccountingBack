using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Contracts;

public class ContractByListFilterCriteriaBuilder : ICriteriaBuilder<Contract, ContractListFilter>
{
    public Expression<Func<Contract, bool>> Build(ContractListFilter options) =>
        x => (!options.CounterpartyId.HasValue || x.CounterpartyId == options.CounterpartyId.Value) &&
             (!options.ContractTypeId.HasValue || x.ContractTypeId == options.ContractTypeId.Value);
}
