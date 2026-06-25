using FluentValidation;

namespace Application.Features.OrgBankAccounts;

public class OrgBankAccountCreateManyDtoValidator : AbstractValidator<OrgBankAccountCreateManyDto>
{
    public OrgBankAccountCreateManyDtoValidator()
    {
        RuleFor(x => x.Accounts).NotEmpty();
        RuleForEach(x => x.Accounts).SetValidator(new OrgBankAccountCreateDtoValidator());
    }
}
