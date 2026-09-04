using FluentValidation;
using SharedKernel.Constants;

namespace Application.Features.Rnt.RentalContracts;

public sealed class RentalLessorInputDtoValidator : AbstractValidator<RentalLessorInputDto>
{
    public RentalLessorInputDtoValidator()
    {
        RuleFor(x => x.LessorKindCode)
            .Must(x => x is RentalLessorKindConst.Individual or RentalLessorKindConst.LegalEntity);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Inn).MaximumLength(20);
        RuleFor(x => x.Pinfl).MaximumLength(14);
        RuleFor(x => x.PhoneNumber).MaximumLength(50);
        RuleFor(x => x.RegisteredAddress).MaximumLength(1000);
        RuleFor(x => x.ResidentialAddress).MaximumLength(1000);
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Inn) || !string.IsNullOrWhiteSpace(x.Pinfl))
            .WithMessage("Lessor INN or PINFL is required.");
        RuleFor(x => x.Inn)
            .NotEmpty()
            .When(x => x.LessorKindCode == RentalLessorKindConst.LegalEntity);
    }
}

public sealed class RentalContractObjectUtilityInputDtoValidator : AbstractValidator<RentalContractObjectUtilityInputDto>
{
    public RentalContractObjectUtilityInputDtoValidator()
    {
        RuleFor(x => x.UtilityServiceId).GreaterThan((short)0);
        RuleFor(x => x.PayerCode)
            .Must(x => x is RentalUtilityPayerConst.Lessor or RentalUtilityPayerConst.Lessee);
    }
}

public sealed class RentalContractObjectInputDtoValidator : AbstractValidator<RentalContractObjectInputDto>
{
    public RentalContractObjectInputDtoValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).When(x => x.Id.HasValue);
        RuleFor(x => x.RentalObjectTypeId).GreaterThan((short)0);
        RuleFor(x => x.ObjectName).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ObjectIdentifier).MaximumLength(250);
        RuleFor(x => x.ObjectAddress).MaximumLength(1000);
        RuleFor(x => x.TotalArea).GreaterThan(0m).When(x => x.TotalArea.HasValue);
        RuleFor(x => x.RentedArea).GreaterThan(0m).When(x => x.RentedArea.HasValue);
        RuleFor(x => x)
            .Must(x => !x.TotalArea.HasValue || !x.RentedArea.HasValue || x.RentedArea <= x.TotalArea)
            .WithMessage("Rented area cannot exceed total area.");
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate);
        RuleFor(x => x.PeriodUnit).Must(x => x is "DAY" or "MONTH");
        RuleFor(x => x.PeriodValue).GreaterThan(0);
        RuleFor(x => x.ContractAmount).GreaterThanOrEqualTo(0m);
        RuleFor(x => x.TaxBaseAmount).GreaterThanOrEqualTo(x => x.ContractAmount);
        RuleFor(x => x.TaxRate).InclusiveBetween(0m, 100m);
        RuleFor(x => x.ExpenseAccountId).GreaterThan(0).When(x => x.ExpenseAccountId.HasValue);
        RuleFor(x => x.Utilities)
            .Must(x => x.Select(item => item.UtilityServiceId).Distinct().Count() == x.Count)
            .WithMessage("A utility service can be specified only once per rental object.");
        RuleForEach(x => x.Utilities).SetValidator(new RentalContractObjectUtilityInputDtoValidator());
    }
}

public sealed class RentalContractBaseDtoValidator : AbstractValidator<RentalContractBaseDto>
{
    public RentalContractBaseDtoValidator()
    {
        RuleFor(x => x.ContractNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ContractDate).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate);
        RuleFor(x => x.CurrencyId).GreaterThan((short)0);
        RuleFor(x => x.LessorPayableAccountId).GreaterThan(0).When(x => x.LessorPayableAccountId.HasValue);
        RuleFor(x => x.TaxPayableAccountId).GreaterThan(0).When(x => x.TaxPayableAccountId.HasValue);
        RuleFor(x => x.Comment).MaximumLength(1000);
        RuleFor(x => x.Lessors).NotEmpty();
        RuleForEach(x => x.Lessors).SetValidator(new RentalLessorInputDtoValidator());
        RuleFor(x => x.Lessors)
            .Must(HaveUniqueLessorIdentifiers)
            .WithMessage("The same lessor cannot be specified more than once.");
        RuleFor(x => x.Objects).NotEmpty();
        RuleForEach(x => x.Objects).SetValidator(new RentalContractObjectInputDtoValidator());
        RuleFor(x => x)
            .Must(x => x.Objects.All(o => o.StartDate.Date >= x.StartDate.Date && o.EndDate.Date <= x.EndDate.Date))
            .WithMessage("Rental object dates must be inside the contract period.");
        RuleFor(x => x.Objects)
            .Must(x => x.All(o => o.ContractAmount == 0m && o.TaxBaseAmount == 0m && o.TaxRate == 0m))
            .When(x => x.IsFreeOfCharge)
            .WithMessage("Free rental objects must have zero contract and tax amounts.");
        RuleFor(x => x.Objects)
            .Must(x => x.All(o => o.ContractAmount > 0m))
            .When(x => !x.IsFreeOfCharge)
            .WithMessage("Paid rental objects must have a positive contract amount.");
    }

    private static bool HaveUniqueLessorIdentifiers(IReadOnlyCollection<RentalLessorInputDto> lessors)
    {
        var pinflValues = lessors.Select(x => Normalize(x.Pinfl)).Where(x => x is not null).ToArray();
        var innValues = lessors.Select(x => Normalize(x.Inn)).Where(x => x is not null).ToArray();
        return pinflValues.Distinct(StringComparer.Ordinal).Count() == pinflValues.Length &&
               innValues.Distinct(StringComparer.Ordinal).Count() == innValues.Length;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class RentalContractCreateDtoValidator : AbstractValidator<RentalContractCreateDto>
{
    public RentalContractCreateDtoValidator() => Include(new RentalContractBaseDtoValidator());
}

public sealed class RentalContractUpdateDtoValidator : AbstractValidator<RentalContractUpdateDto>
{
    public RentalContractUpdateDtoValidator() => Include(new RentalContractBaseDtoValidator());
}
