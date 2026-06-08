using FluentValidation;

namespace Application.Features.CounterpartyBankAccounts;

public class CounterpartyBankAccountUpdateDtoValidator : AbstractValidator<CounterpartyBankAccountUpdateDto>
{
    public CounterpartyBankAccountUpdateDtoValidator()
    {
        Include(new CounterpartyBankAccountBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
