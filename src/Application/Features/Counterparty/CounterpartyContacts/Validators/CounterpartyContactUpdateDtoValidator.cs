using FluentValidation;

namespace Application.Features.CounterpartyContacts;

public class CounterpartyContactUpdateDtoValidator : AbstractValidator<CounterpartyContactUpdateDto>
{
    public CounterpartyContactUpdateDtoValidator()
    {
        Include(new CounterpartyContactBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
