using FluentValidation;

namespace Application.Features.CounterpartyContacts;

public class CounterpartyContactBaseDtoValidator : AbstractValidator<CounterpartyContactBaseDto>
{
    public CounterpartyContactBaseDtoValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.CounterpartyId).GreaterThan(0);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.PhoneNumber).MaximumLength(50).When(x => x.PhoneNumber != null);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => x.Email != null);
        RuleFor(x => x.Position).MaximumLength(250).When(x => x.Position != null);
        RuleFor(x => x.Comment).MaximumLength(1000).When(x => x.Comment != null);
    }
}
