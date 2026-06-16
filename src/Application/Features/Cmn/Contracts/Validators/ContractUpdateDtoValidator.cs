using FluentValidation;

namespace Application.Features.Contracts;

public class ContractUpdateDtoValidator : AbstractValidator<ContractUpdateDto>
{
    public ContractUpdateDtoValidator()
    {
        Include(new ContractBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
