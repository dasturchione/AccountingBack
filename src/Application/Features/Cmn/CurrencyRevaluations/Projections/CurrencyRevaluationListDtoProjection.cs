using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Cmn.CurrencyRevaluations;

public sealed class CurrencyRevaluationListDtoProjection : IProjectionBuilder<CurrencyRevaluation, CurrencyRevaluationListDto>
{
    public Expression<Func<CurrencyRevaluation, CurrencyRevaluationListDto>> Build() =>
        x => new CurrencyRevaluationListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            RevaluationDate = x.RevaluationDate,
            ProviderRateDate = x.ProviderRateDate,
            StatusId = x.StatusId,
            StateId = x.StateId,
            CreatedDate = x.CreatedDate,
            ConfirmedAt = x.ConfirmedAt,
            CancelledAt = x.CancelledAt
        };
}
