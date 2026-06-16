using FluentValidation;

namespace Application.Features.Contracts;

public class ContractCreateDtoValidator : AbstractValidator<ContractCreateDto>
{
    public ContractCreateDtoValidator()
    {
        Include(new ContractBaseDtoValidator());
    }
}
