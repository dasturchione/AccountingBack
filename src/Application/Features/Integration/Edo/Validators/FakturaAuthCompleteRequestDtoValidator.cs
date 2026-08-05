using Application.Abstractions.Integration.Faktura;
using FluentValidation;

namespace Application.Features.Integration.Edo.Validators;

public sealed class FakturaAuthCompleteRequestDtoValidator
    : AbstractValidator<FakturaAuthCompleteRequestDto>
{
    public FakturaAuthCompleteRequestDtoValidator()
    {
        RuleFor(request => request.PreparedPkcs7)
            .NotEmpty();

        RuleFor(request => request.RememberMe)
            .Equal(false)
            .WithMessage("Faktura E-IMZO authentication requires RememberMe=false.");
    }
}
