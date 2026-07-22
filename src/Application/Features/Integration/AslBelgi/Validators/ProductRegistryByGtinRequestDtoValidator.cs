using Application.Features.Integration.AslBelgi.DTOs;
using FluentValidation;

namespace Application.Features.Integration.AslBelgi.Validators;

public sealed class ProductRegistryByGtinRequestDtoValidator : AbstractValidator<ProductRegistryByGtinRequestDto>
{
    public ProductRegistryByGtinRequestDtoValidator()
    {
        RuleFor(x => x.ProductGroup).NotEmpty();
        RuleFor(x => x.Gtin).NotEmpty();
    }
}
