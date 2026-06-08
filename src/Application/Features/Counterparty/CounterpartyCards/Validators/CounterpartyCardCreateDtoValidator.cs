using FluentValidation;

namespace Application.Features.CounterpartyCards;

public class CounterpartyCardCreateDtoValidator : AbstractValidator<CounterpartyCardCreateDto>
{
    public CounterpartyCardCreateDtoValidator()
    {
        Include(new CounterpartyCardBaseDtoValidator());
    }
}
