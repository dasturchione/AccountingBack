using Application.Features.Integration.AslBelgi.DTOs;
using FluentValidation;

namespace Application.Features.Integration.AslBelgi.Validators;

public sealed class CrptSignedDocumentPayloadDtoValidator : AbstractValidator<CrptSignedDocumentPayloadDto>
{
    public CrptSignedDocumentPayloadDtoValidator()
    {
        RuleFor(x => x.DocumentBody)
            .NotEmpty()
            .Must(BeNonEmptyBase64)
            .WithMessage("DocumentBody must be a non-empty Base64 value.");

        RuleFor(x => x.Signature).NotEmpty();
    }

    private static bool BeNonEmptyBase64(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            return Convert.FromBase64String(value).Length > 0;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
