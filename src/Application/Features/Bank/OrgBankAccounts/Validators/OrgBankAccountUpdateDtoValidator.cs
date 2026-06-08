using FluentValidation;

namespace Application.Features.OrgBankAccounts;

public class OrgBankAccountUpdateDtoValidator : AbstractValidator<OrgBankAccountUpdateDto>
{
    public OrgBankAccountUpdateDtoValidator()
    {
        Include(new OrgBankAccountBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
