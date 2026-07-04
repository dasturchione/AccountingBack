using FluentValidation;

namespace Application.Features.Cmn.Taxes;

public sealed class TaxBaseDtoValidator : AbstractValidator<TaxBaseDto>
{
    public TaxBaseDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Rate).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(x => x.EffectiveFrom).NotNull();
        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveFrom.HasValue && x.EffectiveTo.HasValue);
    }
}
