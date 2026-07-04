using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Cmn.CurrencyRevaluations;

public sealed class CurrencyRevaluationDtoProjection : IProjectionBuilder<CurrencyRevaluation, CurrencyRevaluationDto>
{
    public Expression<Func<CurrencyRevaluation, CurrencyRevaluationDto>> Build() =>
        x => new CurrencyRevaluationDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            RevaluationDate = x.RevaluationDate,
            ProviderRateDate = x.ProviderRateDate,
            StatusId = x.StatusId,
            StateId = x.StateId,
            CreatedDate = x.CreatedDate,
            ConfirmedAt = x.ConfirmedAt,
            CancelledAt = x.CancelledAt,
            Lines = x.Lines.Select(l => new CurrencyRevaluationLineDto
            {
                BaseCurrencyId = l.BaseCurrencyId,
                TargetCurrencyId = l.TargetCurrencyId,
                TargetCurrencyCode = l.TargetCurrency.Code,
                BalanceAmount = l.BalanceAmount,
                OpeningRate = l.OpeningRate,
                CurrentRate = l.CurrentRate,
                DifferenceAmount = l.DifferenceAmount
            }).ToList()
        };
}

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
