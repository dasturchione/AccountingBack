using FluentValidation;

namespace Application.Features.CounterpartyBankAccounts;

public class CounterpartyBankAccountCreateDtoValidator : AbstractValidator<CounterpartyBankAccountCreateDto>
{
    public CounterpartyBankAccountCreateDtoValidator()
    {
        Include(new CounterpartyBankAccountBaseDtoValidator());
    }
}
