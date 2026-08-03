using FluentValidation;

namespace Application.Features.FaDisposals;

public class FaDisposalUpdateDtoValidator : AbstractValidator<FaDisposalUpdateDto>
{
    public FaDisposalUpdateDtoValidator()
    {
        Include(new FaDisposalBaseDtoValidator());
    }
}
