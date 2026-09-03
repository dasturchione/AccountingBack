using FluentValidation;

namespace Application.Features.RegulatedObligationSettings;

public class RegulatedObligationSettingBaseDtoValidator<TDto> : AbstractValidator<TDto>
    where TDto : RegulatedObligationSettingBaseDto
{
    protected RegulatedObligationSettingBaseDtoValidator()
    {
        RuleFor(x => x.RegulatedObligationId).GreaterThan((short)0);
        RuleFor(x => x.PeriodicityId).GreaterThan((short)0);
        RuleFor(x => x.ClassifierCode).MaximumLength(30);
        RuleFor(x => x.Rate).InclusiveBetween(0, 100).When(x => x.Rate.HasValue);
        RuleFor(x => x.ChartAccountId).GreaterThan(0);
        RuleFor(x => x.EffectiveFrom).NotEmpty();
        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue);
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
