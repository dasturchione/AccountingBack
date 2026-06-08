using FluentValidation;

namespace Application.Features.Branches;

public class BranchUpdateDtoValidator : AbstractValidator<BranchUpdateDto>
{
    public BranchUpdateDtoValidator()
    {
        Include(new BranchBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
