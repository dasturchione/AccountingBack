using FluentValidation;

namespace Application.Features.CounterpartyCards;

public class CounterpartyCardCreateManyDtoValidator : AbstractValidator<CounterpartyCardCreateManyDto>
{
    public CounterpartyCardCreateManyDtoValidator()
    {
        RuleFor(x => x.Counterparties).NotEmpty();
        RuleForEach(x => x.Counterparties).SetValidator(new CounterpartyCardCreateDtoValidator());
    }
}
