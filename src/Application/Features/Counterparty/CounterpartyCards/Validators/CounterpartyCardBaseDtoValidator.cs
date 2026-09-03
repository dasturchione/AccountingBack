using FluentValidation;

namespace Application.Features.CounterpartyCards;

public class CounterpartyCardBaseDtoValidator : AbstractValidator<CounterpartyCardBaseDto>
{
    public CounterpartyCardBaseDtoValidator()
    {
        RuleFor(x => x.Code).MaximumLength(100).When(x => x.Code != null);
        RuleFor(x => x.ShortName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.FullName).MaximumLength(500).When(x => x.FullName != null);
        RuleFor(x => x.Inn).MaximumLength(20).When(x => x.Inn != null);
        RuleFor(x => x.PhoneNumber).MaximumLength(50).When(x => x.PhoneNumber != null);
        RuleFor(x => x.Email).MaximumLength(250).EmailAddress().When(x => x.Email != null);
        RuleFor(x => x.RegionId).GreaterThan(0).When(x => x.RegionId.HasValue);
        RuleFor(x => x.DistrictId).GreaterThan(0).When(x => x.DistrictId.HasValue);
        RuleFor(x => x.Address).MaximumLength(1000).When(x => x.Address != null);
        RuleFor(x => x.Oked).MaximumLength(20).When(x => x.Oked != null);
        RuleFor(x => x.ExternalId).MaximumLength(100).When(x => x.ExternalId != null);
    }
}
