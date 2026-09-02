using FluentValidation;

namespace Application.Features.OrganizationSetup;

public sealed class OrganizationSetupCompanyProfileDtoValidator
    : AbstractValidator<OrganizationSetupCompanyProfileDto>
{
    public OrganizationSetupCompanyProfileDtoValidator()
    {
        RuleFor(dto => dto.ShortName).NotEmpty().MaximumLength(250);
        RuleFor(dto => dto.FullName).NotEmpty().MaximumLength(500);
        RuleFor(dto => dto.Inn).NotEmpty().MaximumLength(20);
        RuleFor(dto => dto.PhoneNumber).MaximumLength(50).When(dto => dto.PhoneNumber is not null);
        RuleFor(dto => dto.RegionId).GreaterThan(0);
        RuleFor(dto => dto.DistrictId).GreaterThan(0).When(dto => dto.DistrictId.HasValue);
        RuleFor(dto => dto.Address).MaximumLength(1000).When(dto => dto.Address is not null);
        RuleFor(dto => dto.Director).MaximumLength(250).When(dto => dto.Director is not null);
        RuleFor(dto => dto.DefaultLanguageId).GreaterThan((short)0).When(dto => dto.DefaultLanguageId.HasValue);
        RuleFor(dto => dto.Email).MaximumLength(200).When(dto => dto.Email is not null);
        RuleFor(dto => dto.Website).MaximumLength(250).When(dto => dto.Website is not null);
        RuleFor(dto => dto.Oked).MaximumLength(20).When(dto => dto.Oked is not null);
    }
}
