using FluentValidation;

namespace Application.Features.Cmn.CurrencyRevaluations;

public sealed class CurrencyRevaluationCreateDtoValidator : AbstractValidator<CurrencyRevaluationCreateDto>
{
    public CurrencyRevaluationCreateDtoValidator() => Include(new CurrencyRevaluationBaseDtoValidator());
}
