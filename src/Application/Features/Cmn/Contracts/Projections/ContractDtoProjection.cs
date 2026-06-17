using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Contracts;

public class ContractDtoProjection : IProjectionBuilder<Contract, ContractDto>
{
    public Expression<Func<Contract, ContractDto>> Build() =>
        x => new ContractDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            CounterpartyId = x.CounterpartyId,
            CounterpartyName = x.Counterparty.ShortName,
            ContractTypeId = x.ContractTypeId,
            ContractTypeName = x.ContractType.Name,
            ContractNumber = x.ContractNumber,
            ContractDate = x.ContractDate,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            Comment = x.Comment,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
