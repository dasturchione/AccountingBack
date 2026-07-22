using Application.Features.Integration.AslBelgi.DTOs;
using FluentValidation;

namespace Application.Features.Integration.AslBelgi.Validators;

public sealed class MarkingCodeCheckRequestDtoValidator : AbstractValidator<MarkingCodeCheckRequestDto>
{
    public MarkingCodeCheckRequestDtoValidator()
    {
        RuleFor(x => x.Codes).NotEmpty();
        RuleForEach(x => x.Codes).NotEmpty();
    }
}
