using FluentValidation;

namespace Application.Features.FaDisposals;

public class FaDisposalCreateDtoValidator : AbstractValidator<FaDisposalCreateDto>
{
    public FaDisposalCreateDtoValidator()
    {
        Include(new FaDisposalBaseDtoValidator());
    }
}
