using Application.Features.Integration.Edocs.Services;
using FluentValidation;

namespace Application.Features.Integration.Edocs.Validators;

public sealed class EdocsAuthCompleteRequestDtoValidator : AbstractValidator<EdocsAuthCompleteRequestDto>
{
    public EdocsAuthCompleteRequestDtoValidator()
    {
        RuleFor(x => x.AuthId).NotEmpty();
        RuleFor(x => x.SerialNumber).NotEmpty();
        RuleFor(x => x.Pkcs7).NotEmpty();
    }
}
