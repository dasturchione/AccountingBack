using FluentValidation;

namespace Application.Features.OrgBankAccounts;

public class OrgBankAccountBaseDtoValidator : AbstractValidator<OrgBankAccountBaseDto>
{
    public OrgBankAccountBaseDtoValidator()
    {
        RuleFor(x => x.BankId).GreaterThan(0);
        RuleFor(x => x.BankBranchId).GreaterThan(0).When(x => x.BankBranchId.HasValue);
        RuleFor(x => x.AccountNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CurrencyId).GreaterThan((short)0);
    }
}
