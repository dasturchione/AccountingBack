using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.MoneyRegisterBalances;

public class MoneyRegisterBalanceDtoProjection : IProjectionBuilder<MoneyRegisterBalance, MoneyRegisterBalanceDto>
{
    public Expression<Func<MoneyRegisterBalance, MoneyRegisterBalanceDto>> Build() =>
        x => new MoneyRegisterBalanceDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            DocumentTypeId = x.DocumentTypeId,
            DocumentId = x.DocumentId,
            SourceType = x.SourceType,
            SourceId = x.SourceId,
            OperationTypeId = x.OperationTypeId,
            CurrencyId = x.CurrencyId,
            Amount = x.Amount,
            DocDate = x.DocDate,
            PostingBatchId = x.PostingBatchId,
            SourceLineId = x.SourceLineId,
            ReversalEntryId = x.ReversalEntryId,
            CreatedDate = x.CreatedDate
        };
}

public class MoneyRegisterBalanceListDtoProjection : IProjectionBuilder<MoneyRegisterBalance, MoneyRegisterBalanceListDto>
{
    public Expression<Func<MoneyRegisterBalance, MoneyRegisterBalanceListDto>> Build() =>
        x => new MoneyRegisterBalanceListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            DocumentTypeId = x.DocumentTypeId,
            DocumentId = x.DocumentId,
            SourceType = x.SourceType,
            SourceId = x.SourceId,
            OperationTypeId = x.OperationTypeId,
            CurrencyId = x.CurrencyId,
            Amount = x.Amount,
            DocDate = x.DocDate,
            CreatedDate = x.CreatedDate
        };
}
