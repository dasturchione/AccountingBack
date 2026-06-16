using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Contracts;

public class ContractListDtoProjection : IProjectionBuilder<Contract, ContractListDto>
{
    public Expression<Func<Contract, ContractListDto>> Build() =>
        x => new ContractListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            CounterpartyId = x.CounterpartyId,
            CounterpartyName = x.Counterparty.ShortName,
            ContractType = x.ContractType,
            ContractNumber = x.ContractNumber,
            ContractDate = x.ContractDate,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
