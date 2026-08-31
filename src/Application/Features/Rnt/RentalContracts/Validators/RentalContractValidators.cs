using FluentValidation;

namespace Application.Features.Rnt.RentalContracts;

public sealed class RentalContractObjectInputDtoValidator : AbstractValidator<RentalContractObjectInputDto>
{
    public RentalContractObjectInputDtoValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).When(x => x.Id.HasValue);
        RuleFor(x => x.RentalObjectTypeId).GreaterThan((short)0);
        RuleFor(x => x.ObjectName).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ObjectIdentifier).MaximumLength(250);
        RuleFor(x => x.ObjectAddress).MaximumLength(1000);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate);
        RuleFor(x => x.PeriodUnit).Must(x => x is "DAY" or "MONTH");
        RuleFor(x => x.PeriodValue).GreaterThan(0);
        RuleFor(x => x.ContractAmount).GreaterThan(0m);
        RuleFor(x => x.TaxBaseAmount).GreaterThanOrEqualTo(x => x.ContractAmount);
        RuleFor(x => x.TaxRate).InclusiveBetween(0m, 100m);
        RuleFor(x => x.ExpenseAccountId).GreaterThan(0).When(x => x.ExpenseAccountId.HasValue);
    }
}

public sealed class RentalContractBaseDtoValidator : AbstractValidator<RentalContractBaseDto>
{
    public RentalContractBaseDtoValidator()
    {
        RuleFor(x => x.LessorFullName).NotEmpty().MaximumLength(500);
        RuleFor(x => x.LessorInn).MaximumLength(20);
        RuleFor(x => x.LessorPinfl).MaximumLength(14);
        RuleFor(x => x).Must(x => !string.IsNullOrWhiteSpace(x.LessorInn) || !string.IsNullOrWhiteSpace(x.LessorPinfl))
            .WithMessage("Lessor INN or PINFL is required.");
        RuleFor(x => x.ContractNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ContractDate).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate);
        RuleFor(x => x.CurrencyId).GreaterThan((short)0);
        RuleFor(x => x.LessorPayableAccountId).GreaterThan(0).When(x => x.LessorPayableAccountId.HasValue);
        RuleFor(x => x.TaxPayableAccountId).GreaterThan(0).When(x => x.TaxPayableAccountId.HasValue);
        RuleFor(x => x.Comment).MaximumLength(1000);
        RuleFor(x => x.Objects).NotEmpty();
        RuleForEach(x => x.Objects).SetValidator(new RentalContractObjectInputDtoValidator());
        RuleFor(x => x).Must(x => x.Objects.All(o => o.StartDate.Date >= x.StartDate.Date && o.EndDate.Date <= x.EndDate.Date))
            .WithMessage("Rental object dates must be inside the contract period.");
    }
}

public sealed class RentalContractCreateDtoValidator : AbstractValidator<RentalContractCreateDto>
{
    public RentalContractCreateDtoValidator() => Include(new RentalContractBaseDtoValidator());
}

public sealed class RentalContractUpdateDtoValidator : AbstractValidator<RentalContractUpdateDto>
{
    public RentalContractUpdateDtoValidator() => Include(new RentalContractBaseDtoValidator());
}
