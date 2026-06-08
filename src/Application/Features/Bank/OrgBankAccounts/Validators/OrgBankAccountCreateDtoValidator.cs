using FluentValidation;

namespace Application.Features.OrgBankAccounts;

public class OrgBankAccountCreateDtoValidator : AbstractValidator<OrgBankAccountCreateDto>
{
    public OrgBankAccountCreateDtoValidator()
    {
        Include(new OrgBankAccountBaseDtoValidator());
    }
}
