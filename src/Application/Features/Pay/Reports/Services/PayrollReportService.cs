using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Pay.PayrollDocuments;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Pay.Reports;

public sealed class PayrollReportService : IPayrollReportService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<PayPeriod> _periodQuery;
    private readonly IQueryRepository<PayPayrollLine> _payrollLineQuery;
    private readonly IQueryRepository<PayPaymentLine> _paymentLineQuery;

    public PayrollReportService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<PayPeriod> periodQuery,
        IQueryRepository<PayPayrollLine> payrollLineQuery,
        IQueryRepository<PayPaymentLine> paymentLineQuery)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _periodQuery = periodQuery;
        _payrollLineQuery = payrollLineQuery;
        _paymentLineQuery = paymentLineQuery;
    }

    public async Task<Result<PayrollRegisterReportDto>> GetRegisterAsync(long periodId, CancellationToken ct = default)
    {
        var period = await GetPeriodAsync(periodId, ct);
        if (period is null)
            return Result.Failure<PayrollRegisterReportDto>(PayrollErrors.NotFound("Period", periodId, _userContext.LanguageId));

        var allLines = await GetPayrollLinesAsync(periodId, null, ct);
        var eligibleLineIds = allLines
            .Where(IncludeInMainPayrollTotals)
            .Select(x => x.Id)
            .ToList();
        var paid = await GetPaidByEmployeeAsync(periodId, eligibleLineIds, ct);

        var employees = allLines
            .GroupBy(x => x.EmployeeId)
            .Select(group =>
            {
                var latest = group.OrderByDescending(x => x.PayrollDoc.DocDate).First();
                var main = group.Where(IncludeInMainPayrollTotals).ToList();
                var payable = main.Sum(x => x.PayableAmount);
                var calculatedPayable = main.Sum(x => Round(x.NetAmount - x.AdvanceAmount));
                var paidAmount = paid.GetValueOrDefault(group.Key);
                var regular = group.Where(x => x.PayrollDoc.DocumentKind == PayrollDocumentKindConst.Regular).ToList();
                var corrections = group.Where(x => x.PayrollDoc.DocumentKind == PayrollDocumentKindConst.Correction).ToList();
                return new PayrollRegisterEmployeeDto
                {
                    EmployeeId = group.Key,
                    EmployeeNumber = latest.Employee.EmployeeNumber,
                    EmployeeName = $"{latest.Employee.LastName} {latest.Employee.FirstName}",
                    DepartmentName = latest.Employment.Department?.Name,
                    PositionName = latest.Employment.Position?.Name,
                    WorkedDays = Round(main.Sum(x => x.WorkedDays)),
                    WorkedHours = Round(main.Sum(x => x.WorkedHours)),
                    GrossAmount = Round(main.Sum(x => x.GrossAmount)),
                    DeductionAmount = Round(main.Sum(x => x.DeductionAmount)),
                    EmployerTaxAmount = Round(main.Sum(x => x.EmployerTaxAmount)),
                    AdvanceAmount = Round(main.Sum(x => x.AdvanceAmount)),
                    NetAmount = Round(main.Sum(x => x.NetAmount)),
                    PaidAmount = Round(paidAmount),
                    OutstandingAmount = Round(payable - paidAmount),
                    PaidLeaveDays = Round(main.Sum(x => x.PaidLeaveDays)),
                    PaidSickDays = Round(main.Sum(x => x.PaidSickDays)),
                    OvertimeHours = Round(main.Sum(x => x.OvertimeHours)),
                    NightHours = Round(main.Sum(x => x.NightHours)),
                    HolidayHours = Round(main.Sum(x => x.HolidayHours)),
                    WeekendHours = Round(main.Sum(x => x.WeekendHours)),
                    ReconciliationVariance = Round(payable - calculatedPayable),
                    RegularGrossAmount = Round(regular.Sum(x => x.GrossAmount)),
                    RegularNetAmount = Round(regular.Sum(x => x.NetAmount)),
                    CorrectionGrossAmount = Round(corrections.Sum(x => x.GrossAmount)),
                    CorrectionNetAmount = Round(corrections.Sum(x => x.NetAmount))
                };
            })
            .OrderBy(x => x.EmployeeName)
            .ToList();

        return Result.Success(new PayrollRegisterReportDto
        {
            PeriodId = period.Id,
            PeriodName = $"{period.PeriodYear:D4}-{period.PeriodMonth:D2}",
            GrossAmount = Round(employees.Sum(x => x.GrossAmount)),
            DeductionAmount = Round(employees.Sum(x => x.DeductionAmount)),
            EmployerTaxAmount = Round(employees.Sum(x => x.EmployerTaxAmount)),
            NetAmount = Round(employees.Sum(x => x.NetAmount)),
            PaidAmount = Round(employees.Sum(x => x.PaidAmount)),
            OutstandingAmount = Round(employees.Sum(x => x.OutstandingAmount)),
            RegularGrossAmount = Round(employees.Sum(x => x.RegularGrossAmount)),
            RegularNetAmount = Round(employees.Sum(x => x.RegularNetAmount)),
            CorrectionGrossAmount = Round(employees.Sum(x => x.CorrectionGrossAmount)),
            CorrectionNetAmount = Round(employees.Sum(x => x.CorrectionNetAmount)),
            PaidLeaveDays = Round(employees.Sum(x => x.PaidLeaveDays)),
            PaidSickDays = Round(employees.Sum(x => x.PaidSickDays)),
            OvertimeHours = Round(employees.Sum(x => x.OvertimeHours)),
            NightHours = Round(employees.Sum(x => x.NightHours)),
            HolidayHours = Round(employees.Sum(x => x.HolidayHours)),
            WeekendHours = Round(employees.Sum(x => x.WeekendHours)),
            ReconciliationVariance = Round(employees.Sum(x => x.ReconciliationVariance)),
            Employees = employees
        });
    }

    public async Task<Result<PayrollPayslipDto>> GetPayslipAsync(
        long periodId,
        long employeeId,
        CancellationToken ct = default)
    {
        var period = await GetPeriodAsync(periodId, ct);
        if (period is null)
            return Result.Failure<PayrollPayslipDto>(PayrollErrors.NotFound("Period", periodId, _userContext.LanguageId));

        var allLines = await GetPayrollLinesAsync(periodId, employeeId, ct);
        if (allLines.Count == 0)
            return Result.Failure<PayrollPayslipDto>(PayrollErrors.NotFound("PayrollLine", employeeId, _userContext.LanguageId));

        var lines = allLines.Where(IncludeInMainPayrollTotals).ToList();

        var paid = await GetPaidByEmployeeAsync(
            periodId,
            lines.Select(x => x.Id).ToList(),
            ct);
        var latest = allLines.OrderByDescending(x => x.PayrollDoc.DocDate).First();
        var payable = lines.Sum(x => x.PayableAmount);
        var calculatedPayable = lines.Sum(x => Round(x.NetAmount - x.AdvanceAmount));
        var paidAmount = paid.GetValueOrDefault(employeeId);
        var regular = allLines.Where(x => x.PayrollDoc.DocumentKind == PayrollDocumentKindConst.Regular).ToList();
        var corrections = allLines.Where(x => x.PayrollDoc.DocumentKind == PayrollDocumentKindConst.Correction).ToList();

        return Result.Success(new PayrollPayslipDto
        {
            PeriodId = period.Id,
            PeriodName = $"{period.PeriodYear:D4}-{period.PeriodMonth:D2}",
            EmployeeId = employeeId,
            EmployeeNumber = latest.Employee.EmployeeNumber,
            EmployeeName = $"{latest.Employee.LastName} {latest.Employee.FirstName}",
            DepartmentName = latest.Employment.Department?.Name,
            PositionName = latest.Employment.Position?.Name,
            WorkedDays = lines.Sum(x => x.WorkedDays),
            WorkedHours = lines.Sum(x => x.WorkedHours),
            PaidLeaveDays = lines.Sum(x => x.PaidLeaveDays),
            PaidSickDays = lines.Sum(x => x.PaidSickDays),
            OvertimeHours = lines.Sum(x => x.OvertimeHours),
            NightHours = lines.Sum(x => x.NightHours),
            HolidayHours = lines.Sum(x => x.HolidayHours),
            WeekendHours = lines.Sum(x => x.WeekendHours),
            GrossAmount = Round(lines.Sum(x => x.GrossAmount)),
            DeductionAmount = Round(lines.Sum(x => x.DeductionAmount)),
            EmployerTaxAmount = Round(lines.Sum(x => x.EmployerTaxAmount)),
            AdvanceAmount = Round(lines.Sum(x => x.AdvanceAmount)),
            NetAmount = Round(lines.Sum(x => x.NetAmount)),
            PaidAmount = Round(paidAmount),
            OutstandingAmount = Round(payable - paidAmount),
            ReconciliationVariance = Round(payable - calculatedPayable),
            RegularGrossAmount = Round(regular.Sum(x => x.GrossAmount)),
            RegularNetAmount = Round(regular.Sum(x => x.NetAmount)),
            CorrectionGrossAmount = Round(corrections.Sum(x => x.GrossAmount)),
            CorrectionNetAmount = Round(corrections.Sum(x => x.NetAmount)),
            Components = lines
                .SelectMany(x => x.CalcLines)
                .GroupBy(x => new { x.Component.Code, x.Component.Name, x.Component.ComponentType })
                .Select(group => new PayrollPayslipComponentDto
                {
                    Code = group.Key.Code,
                    Name = group.Key.Name,
                    ComponentType = group.Key.ComponentType,
                    BaseAmount = Round(group.Sum(x => x.BaseAmount)),
                    Rate = group.Select(x => x.Rate).LastOrDefault(x => x.HasValue),
                    Amount = Round(group.Sum(x => x.Amount))
                })
                .OrderBy(x => x.ComponentType)
                .ThenBy(x => x.Code)
                .ToList()
        });
    }

    private async Task<PayPeriod?> GetPeriodAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayPeriod>().Where(x => x.Id == id).Build();
        return await _periodQuery.GetAsync(query, ct);
    }

    private async Task<List<PayPayrollLine>> GetPayrollLinesAsync(
        long periodId,
        long? employeeId,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<PayPayrollLine>()
            .Where(x =>
                x.PayrollDoc.PeriodId == periodId &&
                x.PayrollDoc.StateId == StateIdConst.ACTIVE &&
                x.PayrollDoc.StatusId == DocumentStatusIdConst.POSTED &&
                (!employeeId.HasValue || x.EmployeeId == employeeId.Value))
            .Build();
        query.AddIncludes(x => x.Include(line => line.Employee));
        query.AddIncludes(x => x.Include(line => line.Employment).ThenInclude(employment => employment.Department));
        query.AddIncludes(x => x.Include(line => line.Employment).ThenInclude(employment => employment.Position));
        query.AddIncludes(x => x.Include(line => line.PayrollDoc));
        query.AddIncludes(x => x.Include(line => line.CalcLines).ThenInclude(calc => calc.Component));
        return await _payrollLineQuery.GetAllAsync(query, ct);
    }

    private static bool IncludeInMainPayrollTotals(PayPayrollLine line) =>
        PayrollDocumentPaymentPolicy.IsIncludedInMainPayroll(
            line.PayrollDoc.DocumentKind,
            line.PayrollDoc.CorrectionPayoutMode);

    private async Task<Dictionary<long, decimal>> GetPaidByEmployeeAsync(
        long periodId,
        List<long> payrollLineIds,
        CancellationToken ct)
    {
        if (payrollLineIds.Count == 0)
            return new Dictionary<long, decimal>();

        var query = _queryBuilder.For<PayPaymentLine>()
            .Where(x =>
                x.PayrollLineId.HasValue &&
                payrollLineIds.Contains(x.PayrollLineId.Value) &&
                x.PaymentBatch.PeriodId == periodId &&
                x.PaymentBatch.PaymentKind == PayrollPaymentKindConst.Final &&
                x.PaymentBatch.StateId == StateIdConst.ACTIVE &&
                x.PaymentBatch.StatusId == DocumentStatusIdConst.POSTED &&
                x.PaymentBatch.PayrollDoc != null)
            .As(x => new { x.EmployeeId, x.Amount })
            .Build();
        var lines = await _paymentLineQuery.GetAllAsync(query, ct);
        return lines.GroupBy(x => x.EmployeeId).ToDictionary(x => x.Key, x => x.Sum(y => y.Amount));
    }

    private static decimal Round(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
