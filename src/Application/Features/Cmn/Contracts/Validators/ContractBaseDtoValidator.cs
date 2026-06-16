using FluentValidation;

namespace Application.Features.Contracts;

public class ContractBaseDtoValidator : AbstractValidator<ContractBaseDto>
{
    public ContractBaseDtoValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.CounterpartyId).GreaterThan(0);
        RuleFor(x => x.ContractType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ContractNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ContractDate).NotEmpty();
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}
