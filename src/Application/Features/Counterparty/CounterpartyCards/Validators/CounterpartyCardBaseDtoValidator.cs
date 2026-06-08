using FluentValidation;

namespace Application.Features.CounterpartyCards;

public class CounterpartyCardBaseDtoValidator : AbstractValidator<CounterpartyCardBaseDto>
{
    public CounterpartyCardBaseDtoValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.CounterpartyTypeId).GreaterThan((short)0);
        RuleFor(x => x.ShortName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.FullName).MaximumLength(500).When(x => x.FullName != null);
        RuleFor(x => x.Inn).MaximumLength(20).When(x => x.Inn != null);
        RuleFor(x => x.PhoneNumber).MaximumLength(50).When(x => x.PhoneNumber != null);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => x.Email != null);
        RuleFor(x => x.Address).MaximumLength(500).When(x => x.Address != null);
    }
}
