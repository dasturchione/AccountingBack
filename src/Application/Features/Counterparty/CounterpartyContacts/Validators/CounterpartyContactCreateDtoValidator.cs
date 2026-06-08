using FluentValidation;

namespace Application.Features.CounterpartyContacts;

public class CounterpartyContactCreateDtoValidator : AbstractValidator<CounterpartyContactCreateDto>
{
    public CounterpartyContactCreateDtoValidator()
    {
        Include(new CounterpartyContactBaseDtoValidator());
    }
}
