using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Contracts;

public sealed class ContractDtoProjection(IUserContext userContext) : IProjectionBuilder<Contract, ContractDto>
{
    public Expression<Func<Contract, ContractDto>> Build()
    {
        var languageId = userContext.LanguageId;

        return x => new ContractDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            CounterpartyId = x.CounterpartyId,
            CounterpartyName = x.Counterparty.FullName == null ? x.Counterparty.ShortName : x.Counterparty.FullName,
            ContractTypeId = x.ContractTypeId,
            ContractTypeName = x.ContractType.ContractTypeTranslations
                .Where(translation => translation.LanguageId == languageId)
                .Select(translation => translation.Name)
                .FirstOrDefault() ?? x.ContractType.Name,
            ContractNumber = x.ContractNumber,
            ContractDate = x.ContractDate,
            ProviderCode = x.ProviderCode,
            ProviderContractNumber = x.ProviderContractNumber,
            ProviderContractDate = x.ProviderContractDate,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            Comment = x.Comment,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
    }
}
