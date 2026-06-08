using FluentValidation;

namespace Application.Features.CounterpartyCards;

public class CounterpartyCardUpdateDtoValidator : AbstractValidator<CounterpartyCardUpdateDto>
{
    public CounterpartyCardUpdateDtoValidator()
    {
        Include(new CounterpartyCardBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
