using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CounterpartyRegisterBalances;

public class CounterpartyRegisterBalanceDtoProjection : IProjectionBuilder<CounterpartyRegisterBalance, CounterpartyRegisterBalanceDto>
{
    public Expression<Func<CounterpartyRegisterBalance, CounterpartyRegisterBalanceDto>> Build() =>
        x => new CounterpartyRegisterBalanceDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            DocumentTypeId = x.DocumentTypeId,
            DocumentId = x.DocumentId,
            CounterpartyId = x.CounterpartyId,
            OperationTypeId = x.OperationTypeId,
            CurrencyId = x.CurrencyId,
            Amount = x.Amount,
            DocDate = x.DocDate,
            CreatedDate = x.CreatedDate
        };
}

public class CounterpartyRegisterBalanceListDtoProjection : IProjectionBuilder<CounterpartyRegisterBalance, CounterpartyRegisterBalanceListDto>
{
    public Expression<Func<CounterpartyRegisterBalance, CounterpartyRegisterBalanceListDto>> Build() =>
        x => new CounterpartyRegisterBalanceListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            DocumentTypeId = x.DocumentTypeId,
            DocumentId = x.DocumentId,
            CounterpartyId = x.CounterpartyId,
            OperationTypeId = x.OperationTypeId,
            CurrencyId = x.CurrencyId,
            Amount = x.Amount,
            DocDate = x.DocDate,
            CreatedDate = x.CreatedDate
        };
}
