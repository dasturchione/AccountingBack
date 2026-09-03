using FluentValidation;

namespace Application.Features.Cmn.CurrencyRevaluations;

public sealed class CurrencyRevaluationPreviewDtoValidator : AbstractValidator<CurrencyRevaluationPreviewDto>
{
    public CurrencyRevaluationPreviewDtoValidator() => Include(new CurrencyRevaluationBaseDtoValidator());
}
