using Application.Features.Pay.Components;
using Application.Features.Pay.Employees;
using Application.Features.Pay.Payments;
using Application.Features.Pay.PayrollDocuments;
using Application.Features.Pay.Periods;
using Application.Features.Pay.Timesheets;
using FluentValidation;
using SharedKernel.Constants;

namespace Application.Features.Pay.Validators;

public sealed class PayrollEmployeeCreateDtoValidator : AbstractValidator<PayrollEmployeeCreateDto>
{
    public PayrollEmployeeCreateDtoValidator()
    {
        RuleFor(x => x.EmployeeNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Pinfl).Length(14).When(x => !string.IsNullOrWhiteSpace(x.Pinfl));
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Employment).SetValidator(new PayrollEmploymentSaveDtoValidator());
    }
}

public sealed class PayrollEmployeeUpdateDtoValidator : AbstractValidator<PayrollEmployeeUpdateDto>
{
    public PayrollEmployeeUpdateDtoValidator()
    {
        RuleFor(x => x.EmployeeNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Pinfl).Length(14).When(x => !string.IsNullOrWhiteSpace(x.Pinfl));
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

public sealed class PayrollEmploymentSaveDtoValidator : AbstractValidator<PayrollEmploymentSaveDto>
{
    public PayrollEmploymentSaveDtoValidator()
    {
        RuleFor(x => x.EmploymentType).Must(PayrollEmploymentTypeConst.All.Contains);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).When(x => x.EndDate.HasValue);
        RuleFor(x => x.MonthlySalary).GreaterThanOrEqualTo(0);
        RuleFor(x => x.EmploymentRate).GreaterThan(0).LessThanOrEqualTo(2);
        RuleFor(x => x.WeeklyHours).GreaterThan(0).LessThanOrEqualTo(168);
        RuleFor(x => x.CurrencyId).GreaterThan((short)0);
    }
}

public sealed class PayrollEmployeeComponentSaveDtoValidator : AbstractValidator<PayrollEmployeeComponentSaveDto>
{
    public PayrollEmployeeComponentSaveDtoValidator()
    {
        RuleFor(x => x.ComponentId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0).When(x => x.Amount.HasValue);
        RuleFor(x => x.Rate).GreaterThanOrEqualTo(0).When(x => x.Rate.HasValue);
        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue);
    }
}

public sealed class PayrollComponentCreateDtoValidator : AbstractValidator<PayrollComponentCreateDto>
{
    public PayrollComponentCreateDtoValidator() => Configure<PayrollComponentCreateDto>(this);

    internal static void Configure<T>(AbstractValidator<T> validator) where T : PayrollComponentBaseDto
    {
        validator.RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        validator.RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        validator.RuleFor(x => x.ComponentType).Must(PayrollComponentTypeConst.All.Contains);
        validator.RuleFor(x => x.CalculationMethod).Must(PayrollCalculationMethodConst.All.Contains);
        validator.RuleFor(x => x.DefaultAmount).GreaterThanOrEqualTo(0).When(x => x.DefaultAmount.HasValue);
        validator.RuleFor(x => x.DefaultRate).GreaterThanOrEqualTo(0).When(x => x.DefaultRate.HasValue);
        validator.RuleFor(x => x.EffectiveTo).GreaterThanOrEqualTo(x => x.EffectiveFrom).When(x => x.EffectiveTo.HasValue);
        validator.RuleFor(x => x.SortOrder).GreaterThan(0);
        validator.RuleFor(x => x.ExpenseAccountId)
            .NotNull()
            .When(x => x.ComponentType == PayrollComponentTypeConst.Reclassification);
        validator.RuleFor(x => x.LiabilityAccountId)
            .NotNull()
            .When(x => x.ComponentType == PayrollComponentTypeConst.Reclassification);
    }
}

public sealed class PayrollComponentUpdateDtoValidator : AbstractValidator<PayrollComponentUpdateDto>
{
    public PayrollComponentUpdateDtoValidator() =>
        PayrollComponentCreateDtoValidator.Configure<PayrollComponentUpdateDto>(this);
}

public sealed class PayrollPeriodCreateDtoValidator : AbstractValidator<PayrollPeriodCreateDto>
{
    public PayrollPeriodCreateDtoValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween((short)2000, (short)2200);
        RuleFor(x => x.Month).InclusiveBetween((short)1, (short)12);
        RuleFor(x => x.NormWorkDays).GreaterThan(0);
        RuleFor(x => x.NormWorkHours).GreaterThan(0);
    }
}

public sealed class PayrollTimesheetCreateDtoValidator : AbstractValidator<PayrollTimesheetCreateDto>
{
    public PayrollTimesheetCreateDtoValidator()
    {
        RuleFor(x => x.PeriodId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).SetValidator(new PayrollTimesheetLineSaveDtoValidator());
    }
}

public sealed class PayrollTimesheetUpdateDtoValidator : AbstractValidator<PayrollTimesheetUpdateDto>
{
    public PayrollTimesheetUpdateDtoValidator()
    {
        RuleFor(x => x.PeriodId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).SetValidator(new PayrollTimesheetLineSaveDtoValidator());
    }
}

public sealed class PayrollTimesheetLineSaveDtoValidator : AbstractValidator<PayrollTimesheetLineSaveDto>
{
    public PayrollTimesheetLineSaveDtoValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0);
        RuleFor(x => x.WorkedDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.WorkedHours).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LeaveDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SickDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AbsentDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OvertimeHours).GreaterThanOrEqualTo(0);
    }
}

public sealed class PayrollCalculateDtoValidator : AbstractValidator<PayrollCalculateDto>
{
    public PayrollCalculateDtoValidator()
    {
        RuleFor(x => x.PeriodId).GreaterThan(0);
        RuleFor(x => x.DocumentKind)
            .Must(x => x is PayrollDocumentKindConst.Regular or PayrollDocumentKindConst.Correction);
        RuleFor(x => x.CorrectionOfDocId)
            .NotNull()
            .When(x => x.DocumentKind == PayrollDocumentKindConst.Correction);
        RuleFor(x => x.CorrectionOfDocId)
            .Null()
            .When(x => x.DocumentKind == PayrollDocumentKindConst.Regular);
        RuleFor(x => x.SalaryExpenseAccountId).GreaterThan(0);
        RuleFor(x => x.SalaryPayableAccountId).GreaterThan(0);
        RuleFor(x => x.DeductionPayableAccountId).GreaterThan(0);
        RuleFor(x => x.EmployerTaxExpenseAccountId).GreaterThan(0);
        RuleFor(x => x.EmployerTaxPayableAccountId).GreaterThan(0);
        RuleFor(x => x.AdvanceReceivableAccountId).GreaterThan(0);
        RuleForEach(x => x.Adjustments).ChildRules(adjustment =>
        {
            adjustment.RuleFor(x => x.EmployeeId).GreaterThan(0);
            adjustment.RuleFor(x => x.ComponentId).GreaterThan(0);
            adjustment.RuleFor(x => x.Note).MaximumLength(500);
        });
    }
}

public sealed class PayrollPaymentCreateDtoValidator : AbstractValidator<PayrollPaymentCreateDto>
{
    public PayrollPaymentCreateDtoValidator()
    {
        RuleFor(x => x.PeriodId).GreaterThan(0);
        RuleFor(x => x.PaymentKind).Must(x => x is PayrollPaymentKindConst.Advance or PayrollPaymentKindConst.Final);
        RuleFor(x => x.SourceType).Must(x => x is PayrollPaymentSourceConst.Bank or PayrollPaymentSourceConst.Cash);
        RuleFor(x => x.SourceChartAccountId).GreaterThan(0);
        RuleFor(x => x.OffsetAccountId).GreaterThan(0);
        RuleFor(x => x.CurrencyId).GreaterThan((short)0);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(x => x.EmployeeId).GreaterThan(0);
            line.RuleFor(x => x.Amount).GreaterThan(0);
        });
    }
}
