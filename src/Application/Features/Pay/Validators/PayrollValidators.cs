using Application.Features.Pay.Components;
using Application.Features.Pay.Employees;
using Application.Features.Pay.HrOrders;
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
        RuleFor(x => x.AdvanceMethod).Must(x => PayrollAdvanceMethodConst.All.Contains(x));
        RuleFor(x => x.AdvanceValue).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AdvanceValue).LessThanOrEqualTo(100)
            .When(x => x.AdvanceMethod == PayrollAdvanceMethodConst.Percent);
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
        validator.RuleFor(x => x.ProrationBasis).Must(PayrollProrationBasisConst.All.Contains);
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
    public PayrollPeriodCreateDtoValidator() => Configure(this);

    internal static void Configure<T>(AbstractValidator<T> validator) where T : PayrollPeriodSaveDto
    {
        validator.RuleFor(x => x.Year).InclusiveBetween((short)2000, (short)2200);
        validator.RuleFor(x => x.Month).InclusiveBetween((short)1, (short)12);
        validator.RuleFor(x => x.DailyWorkHours).GreaterThan(0).LessThanOrEqualTo(24);
        validator.RuleFor(x => x.WorkDates)
            .NotEmpty()
            .When(x => x.CalendarDays is null || x.CalendarDays.Count == 0);
        validator.RuleFor(x => x.WorkDates)
            .Must(dates => dates is null || dates.Distinct().Count() == dates.Count)
            .WithMessage("Work dates must be unique.");
        validator.RuleForEach(x => x.WorkDates)
            .Must((dto, date) => date.Year == dto.Year && date.Month == dto.Month)
            .WithMessage("Every work date must belong to the submitted year and month.");
        validator.RuleForEach(x => x.CalendarDays).ChildRules(day =>
        {
            day.RuleFor(x => x.DayType).Must(PayrollPeriodDayTypeConst.All.Contains);
            day.RuleFor(x => x.WorkHours).GreaterThanOrEqualTo(0).LessThanOrEqualTo(24);
        });
        validator.RuleFor(x => x.CalendarDays)
            .Must((dto, days) => days is null || days.All(day => day.Date.Year == dto.Year && day.Date.Month == dto.Month))
            .WithMessage("Every calendar date must belong to the submitted year and month.");
    }
}

public sealed class PayrollPeriodUpdateDtoValidator : AbstractValidator<PayrollPeriodUpdateDto>
{
    public PayrollPeriodUpdateDtoValidator() =>
        PayrollPeriodCreateDtoValidator.Configure(this);
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
        RuleFor(x => x.OvertimeHours).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Note).MaximumLength(500);
        RuleFor(x => x.Days)
            .NotEmpty()
            .Must(days => days is not null && days.Select(day => day.Date).Distinct().Count() == days.Count)
            .WithMessage("Timesheet days must be unique.");
        RuleForEach(x => x.Days).ChildRules(day =>
        {
            day.RuleFor(x => x.StatusCode).NotEmpty().MaximumLength(50);
            day.RuleFor(x => x.AbsenceTypeId).GreaterThan((short)0).When(x => x.AbsenceTypeId.HasValue);
        });
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
        RuleFor(x => x.CorrectionPayoutMode)
            .Must(x => x is null || PayrollCorrectionPayoutModeConst.All.Contains(x.Trim().ToUpperInvariant()));
        // Optional: when omitted the organization's default payroll accounts are used.
        RuleFor(x => x.SalaryExpenseAccountId).GreaterThan(0).When(x => x.SalaryExpenseAccountId.HasValue);
        RuleFor(x => x.SalaryPayableAccountId).GreaterThan(0).When(x => x.SalaryPayableAccountId.HasValue);
        RuleForEach(x => x.Adjustments).ChildRules(adjustment =>
        {
            adjustment.RuleFor(x => x.EmployeeId).GreaterThan(0);
            adjustment.RuleFor(x => x.ComponentId).GreaterThan(0);
            adjustment.RuleFor(x => x.Note).MaximumLength(500);
            // Exactly one of Amount (raw delta) or TargetAmount (new absolute value).
            adjustment.RuleFor(x => x)
                .Must(x => x.Amount.HasValue ^ x.TargetAmount.HasValue)
                .WithMessage("Provide exactly one of Amount or TargetAmount.");
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

public sealed class PayrollHrOrderSaveDtoValidator : AbstractValidator<PayrollHrOrderSaveDto>
{
    public PayrollHrOrderSaveDtoValidator()
    {
        RuleFor(x => x.OrderType).Must(x => PayrollHrOrderTypeConst.All.Contains(x?.Trim().ToUpperInvariant()))
            .WithMessage("Order type must be HIRE, TRANSFER, PAY_CHANGE or DISMISSAL.");
        RuleFor(x => x.EmployeeId).GreaterThan(0);
        RuleFor(x => x.OrderDate).NotEmpty();
        RuleFor(x => x.EffectiveDate).NotEmpty();
        RuleFor(x => x.MonthlySalary).GreaterThanOrEqualTo(0).When(x => x.MonthlySalary.HasValue);
        RuleFor(x => x.EmploymentRate).GreaterThan(0).LessThanOrEqualTo(2).When(x => x.EmploymentRate.HasValue);
        RuleFor(x => x.Basis).MaximumLength(500);
        RuleFor(x => x.Note).MaximumLength(500);

        // PAY_CHANGE uchun yangi oklad majburiy.
        RuleFor(x => x.MonthlySalary)
            .NotNull()
            .When(x => string.Equals(x.OrderType?.Trim(), PayrollHrOrderTypeConst.PayChange, System.StringComparison.OrdinalIgnoreCase))
            .WithMessage("PAY_CHANGE requires MonthlySalary.");
    }
}
