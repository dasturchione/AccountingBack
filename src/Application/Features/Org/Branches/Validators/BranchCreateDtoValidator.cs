using FluentValidation;

namespace Application.Features.Branches;

public class BranchCreateDtoValidator : AbstractValidator<BranchCreateDto>
{
    public BranchCreateDtoValidator()
    {
        Include(new BranchBaseDtoValidator());
    }
}
