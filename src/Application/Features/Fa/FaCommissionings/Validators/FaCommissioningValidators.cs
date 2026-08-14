using FluentValidation;

namespace Application.Features.FaCommissionings;

public class FaCommissioningBaseDtoValidator : AbstractValidator<FaCommissioningBaseDto>
{
    public FaCommissioningBaseDtoValidator()
    {
        RuleFor(dto => dto.DocDate).NotEmpty();
        RuleFor(dto => dto.Note).MaximumLength(500);
        RuleFor(dto => dto.Lines).NotEmpty();
        RuleForEach(dto => dto.Lines).SetValidator(new FaCommissioningLineWriteDtoValidator());
    }
}

public class FaCommissioningCreateDtoValidator : AbstractValidator<FaCommissioningCreateDto>
{
    public FaCommissioningCreateDtoValidator()
    {
        Include(new FaCommissioningBaseDtoValidator());
    }
}

public class FaCommissioningUpdateDtoValidator : AbstractValidator<FaCommissioningUpdateDto>
{
    public FaCommissioningUpdateDtoValidator()
    {
        Include(new FaCommissioningBaseDtoValidator());
    }
}

public class FaCommissioningLineWriteDtoValidator :
    AbstractValidator<FaCommissioningLineWriteDto>
{
    public FaCommissioningLineWriteDtoValidator()
    {
        RuleFor(line => line.FaAssetId).GreaterThan(0);
        RuleFor(line => line.DeprStartDate).NotEmpty();
        RuleFor(line => line.SalvageValue).GreaterThanOrEqualTo(0);
        RuleFor(line => line.UsefulLifeMonths).GreaterThan(0);
        RuleFor(line => line.DepreciationMethodId).GreaterThan((short)0);
        RuleFor(line => line.PlannedUnitsTotal)
            .GreaterThan(0)
            .When(line => line.PlannedUnitsTotal.HasValue);
        RuleFor(line => line.DepartmentId)
            .GreaterThan(0)
            .When(line => line.DepartmentId.HasValue);
        RuleFor(line => line.ResponsibleUserId)
            .GreaterThan(0)
            .When(line => line.ResponsibleUserId.HasValue);
        RuleFor(line => line.AccumulatedDepreciationAccountId).GreaterThan(0);
        RuleFor(line => line.DepreciationExpenseAccountId).GreaterThan(0);
        RuleFor(line => line.Note).MaximumLength(500);
    }
}
