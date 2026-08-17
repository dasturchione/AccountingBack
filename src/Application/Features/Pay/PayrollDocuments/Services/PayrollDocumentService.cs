using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.Register.AccountingRegisterEntries;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Pay.PayrollDocuments;

public sealed class PayrollDocumentService : BaseService, IPayrollDocumentService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IDocNumberGenerator _docNumberGenerator;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAccountingPeriodValidator _accountingPeriodValidator;
    private readonly IAccountingDispatcher _dispatcher;
    private readonly IQueryRepository<PayPayrollDoc> _query;
    private readonly ICommandRepository<PayPayrollDoc> _command;
    private readonly IQueryRepository<PayPeriod> _periodQuery;
    private readonly IQueryRepository<PayTimesheet> _timesheetQuery;
    private readonly IQueryRepository<PayEmployment> _employmentQuery;
    private readonly IQueryRepository<PayComponent> _componentQuery;
    private readonly IQueryRepository<PayEmployeeComponent> _assignmentQuery;
    private readonly IQueryRepository<PayPaymentLine> _paymentLineQuery;
    private readonly IQueryRepository<PayPaymentBatch> _paymentBatchQuery;
    private readonly IQueryRepository<PostingBatch> _postingBatchQuery;
    private readonly ICommandRepository<PostingBatch> _postingBatchCommand;
    private readonly IQueryRepository<AccountingRegisterEntry> _accountingEntryQuery;
    private readonly ICommandRepository<AccountingRegisterEntry> _accountingEntryCommand;

    public PayrollDocumentService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IDocNumberGenerator docNumberGenerator,
        IDocumentPostingLock postingLock,
        IAccountingPeriodValidator accountingPeriodValidator,
        IAccountingDispatcher dispatcher,
        IQueryRepository<PayPayrollDoc> query,
        ICommandRepository<PayPayrollDoc> command,
        IQueryRepository<PayPeriod> periodQuery,
        IQueryRepository<PayTimesheet> timesheetQuery,
        IQueryRepository<PayEmployment> employmentQuery,
        IQueryRepository<PayComponent> componentQuery,
        IQueryRepository<PayEmployeeComponent> assignmentQuery,
        IQueryRepository<PayPaymentLine> paymentLineQuery,
        IQueryRepository<PayPaymentBatch> paymentBatchQuery,
        IQueryRepository<PostingBatch> postingBatchQuery,
        ICommandRepository<PostingBatch> postingBatchCommand,
        IQueryRepository<AccountingRegisterEntry> accountingEntryQuery,
        ICommandRepository<AccountingRegisterEntry> accountingEntryCommand,
        ILogger<PayrollDocumentService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _docNumberGenerator = docNumberGenerator;
        _postingLock = postingLock;
        _accountingPeriodValidator = accountingPeriodValidator;
        _dispatcher = dispatcher;
        _query = query;
        _command = command;
        _periodQuery = periodQuery;
        _timesheetQuery = timesheetQuery;
        _employmentQuery = employmentQuery;
        _componentQuery = componentQuery;
        _assignmentQuery = assignmentQuery;
        _paymentLineQuery = paymentLineQuery;
        _paymentBatchQuery = paymentBatchQuery;
        _postingBatchQuery = postingBatchQuery;
        _postingBatchCommand = postingBatchCommand;
        _accountingEntryQuery = accountingEntryQuery;
        _accountingEntryCommand = accountingEntryCommand;
    }

    public Task<Result<PagedResponse<PayrollDocumentListDto>>> GetAllAsync(
        PayrollDocumentListFilter filter,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var page = Math.Max(filter.Page, 1);
            var take = Math.Clamp(filter.PageSize ?? 50, 1, 200);
            var search = filter.Search?.Trim().ToLower();
            var specification = _queryBuilder.For<PayPayrollDoc>()
                .Where(x =>
                    x.StateId == StateIdConst.ACTIVE &&
                    (!filter.PeriodId.HasValue || x.PeriodId == filter.PeriodId.Value) &&
                    (!filter.StatusId.HasValue || x.StatusId == filter.StatusId.Value) &&
                    (string.IsNullOrWhiteSpace(filter.DocumentKind) || x.DocumentKind == filter.DocumentKind) &&
                    (string.IsNullOrWhiteSpace(search) ||
                     x.DocNumber.ToLower().Contains(search) ||
                     (x.Note != null && x.Note.ToLower().Contains(search))))
                .As(x => new PayrollDocumentListDto
                {
                    Id = x.Id,
                    DocNumber = x.DocNumber,
                    DocDate = x.DocDate,
                    PeriodId = x.PeriodId,
                    PeriodName = x.Period.PeriodYear + "-" + x.Period.PeriodMonth,
                    DocumentKind = x.DocumentKind,
                    StatusId = x.StatusId,
                    StatusName = x.Status.Name,
                    GrossAmount = x.GrossAmount,
                    DeductionAmount = x.DeductionAmount,
                    EmployerTaxAmount = x.EmployerTaxAmount,
                    NetAmount = x.NetAmount,
                    PayableAmount = x.PayableAmount
                })
                .OrderBy(x => x.OrderByDescending(y => y.DocDate).ThenByDescending(y => y.Id))
                .Skip((page - 1) * take)
                .Take(take)
                .BuildPaged();
            var paged = await _query.GetPagedAsync(specification, ct);
            return Result.Success(PagedResponseFactory.Create(paged, page, take));
        });

    public Task<Result<PayrollDocumentDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var dto = await GetDtoInternalAsync(id, ct);
            return dto is null
                ? Result.Failure<PayrollDocumentDto>(PayrollErrors.NotFound("PayrollDocument", id, _userContext.LanguageId))
                : Result.Success(dto);
        });

    public Task<Result<long>> CalculateAsync(PayrollCalculateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CalculateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            var period = await GetPeriodAsync(dto.PeriodId, ct);
            if (period is null)
                return Result.Failure<long>(PayrollErrors.NotFound("Period", dto.PeriodId, _userContext.LanguageId));
            if (period.Status != PayrollPeriodStatusConst.Open)
                return Result.Failure<long>(PayrollErrors.PeriodClosed(period.Id));

            var kind = dto.DocumentKind.Trim().ToUpperInvariant();
            if (kind == PayrollDocumentKindConst.Regular &&
                await _query.AnyAsync(x =>
                    x.PeriodId == period.Id &&
                    x.DocumentKind == PayrollDocumentKindConst.Regular &&
                    x.StateId == StateIdConst.ACTIVE &&
                    x.StatusId != DocumentStatusIdConst.CANCELLED, ct))
                return Result.Failure<long>(PayrollErrors.RegularPayrollAlreadyExists(period.Id));

            PayPayrollDoc? correctionSource = null;
            if (kind == PayrollDocumentKindConst.Correction)
            {
                if (dto.CorrectionOfDocId.HasValue)
                {
                    var sourceQuery = _queryBuilder.For<PayPayrollDoc>()
                        .Where(x =>
                            x.Id == dto.CorrectionOfDocId.Value &&
                            x.PeriodId == period.Id &&
                            x.StatusId == DocumentStatusIdConst.POSTED)
                        .Build();
                    correctionSource = await _query.GetAsync(sourceQuery, ct);
                }

                if (correctionSource is null)
                    return Result.Failure<long>(PayrollErrors.CorrectionSourceRequired());
                if (dto.Adjustments.Count == 0)
                    return Result.Failure<long>(PayrollErrors.Business("EmptyCorrection", "Tuzatish hujjatida kamida bitta qo‘lda kiritilgan tuzatish bo‘lishi kerak."));
            }

            var duplicateAdjustment = dto.Adjustments
                .GroupBy(x => new { x.EmployeeId, x.ComponentId })
                .FirstOrDefault(x => x.Count() > 1);
            if (duplicateAdjustment is not null)
                return Result.Failure<long>(PayrollErrors.DuplicateAdjustment(
                    duplicateAdjustment.Key.EmployeeId,
                    duplicateAdjustment.Key.ComponentId));
            if (kind == PayrollDocumentKindConst.Regular && dto.Adjustments.Any(x => x.Amount < 0))
                return Result.Failure<long>(PayrollErrors.Business("NegativeRegularAdjustment", "Manfiy summa faqat tuzatish hujjatida qo‘lda kiritilishi mumkin."));

            var timesheet = await GetPostedTimesheetAsync(period.Id, ct);
            if (timesheet is null)
                return Result.Failure<long>(PayrollErrors.NoPostedTimesheet(period.Id));

            var components = await GetActiveComponentsAsync(period, ct);
            if (components.Count == 0)
                return Result.Failure<long>(PayrollErrors.NoCalculationComponents(period.Id));
            if (kind == PayrollDocumentKindConst.Regular &&
                !components.Any(x =>
                    x.IsMandatory &&
                    x.ComponentType == PayrollComponentTypeConst.Earning &&
                    x.CalculationMethod == PayrollCalculationMethodConst.SalaryProrated))
                return Result.Failure<long>(PayrollErrors.MissingBaseSalaryComponent());

            var employeeIds = kind == PayrollDocumentKindConst.Regular
                ? timesheet.Lines.Select(x => x.EmployeeId).Distinct().ToList()
                : dto.Adjustments.Select(x => x.EmployeeId).Distinct().ToList();

            var employments = await GetEmploymentsAsync(employeeIds, period, ct);
            var employmentByEmployee = employments
                .GroupBy(x => x.EmployeeId)
                .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.StartDate).First());
            var missingEmployment = employeeIds.FirstOrDefault(id => !employmentByEmployee.ContainsKey(id));
            if (missingEmployment > 0)
                return Result.Failure<long>(PayrollErrors.NoActiveEmployment(missingEmployment));

            var assignments = await GetAssignmentsAsync(employeeIds, period, ct);
            var assignmentMap = assignments
                .GroupBy(x => (x.EmployeeId, x.ComponentId))
                .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.EffectiveFrom).First());
            var manualMap = dto.Adjustments.ToDictionary(x => (x.EmployeeId, x.ComponentId));
            var componentById = components.ToDictionary(x => x.Id);
            var invalidComponent = dto.Adjustments.FirstOrDefault(x => !componentById.ContainsKey(x.ComponentId));
            if (invalidComponent is not null)
                return Result.Failure<long>(PayrollErrors.ReferencedRecordNotFound("Component", invalidComponent.ComponentId));

            var timesheetByEmployee = timesheet.Lines.ToDictionary(x => x.EmployeeId);
            var advances = kind == PayrollDocumentKindConst.Regular
                ? await GetPostedAdvancesAsync(period.Id, employeeIds, ct)
                : new Dictionary<long, decimal>();

            var payrollLines = new List<PayPayrollLine>();
            foreach (var employeeId in employeeIds)
            {
                if (!timesheetByEmployee.TryGetValue(employeeId, out var time))
                    return Result.Failure<long>(PayrollErrors.Business("EmployeeMissingFromTimesheet", $"Xodim tasdiqlangan tabelga kiritilmagan (xodim ID: {employeeId})."));

                var selectedComponents = kind == PayrollDocumentKindConst.Correction
                    ? components.Where(component => manualMap.ContainsKey((employeeId, component.Id))).ToList()
                    : components.Where(component =>
                        component.IsMandatory ||
                        assignmentMap.ContainsKey((employeeId, component.Id)) ||
                        manualMap.ContainsKey((employeeId, component.Id))).ToList();

                var line = BuildPayrollLine(
                    organizationId,
                    employmentByEmployee[employeeId],
                    time,
                    period,
                    selectedComponents,
                    assignmentMap,
                    manualMap,
                    advances.GetValueOrDefault(employeeId),
                    kind);
                payrollLines.Add(line);
            }

            if (payrollLines.Count == 0)
                return Result.Failure<long>(PayrollErrors.Business("NoPayrollLines", "Oylik hisoblash natijasida xodimlar bo‘yicha hech qanday qator hosil bo‘lmadi."));

            var payrollCurrencyIds = payrollLines
                .Select(x => x.Employment.CurrencyId)
                .Distinct()
                .ToList();
            if (payrollCurrencyIds.Count != 1)
                return Result.Failure<long>(PayrollErrors.Business(
                    "MixedPayrollCurrencies",
                    "Bitta oylik hisoblash hujjatidagi barcha xodimlarning ish haqi valyutasi bir xil bo‘lishi kerak."));
            if (correctionSource is not null && correctionSource.CurrencyId != payrollCurrencyIds[0])
                return Result.Failure<long>(PayrollErrors.Business(
                    "CorrectionCurrencyMismatch",
                    $"Tuzatish hujjati valyutasi (ID: {payrollCurrencyIds[0]}) asosiy oylik hujjati valyutasiga (ID: {correctionSource.CurrencyId}) mos kelmaydi."));

            var now = DateTime.Now;
            var document = new PayPayrollDoc
            {
                OrganizationId = organizationId,
                PeriodId = period.Id,
                DocNumber = await _docNumberGenerator.GenerateAsync(organizationId, "PAY", dto.DocDate, ct),
                DocDate = dto.DocDate,
                DocumentKind = kind,
                CorrectionOfDocId = dto.CorrectionOfDocId,
                CurrencyId = payrollCurrencyIds[0],
                StatusId = DocumentStatusIdConst.DRAFT,
                GrossAmount = Round(payrollLines.Sum(x => x.GrossAmount)),
                DeductionAmount = Round(payrollLines.Sum(x => x.DeductionAmount)),
                EmployerTaxAmount = Round(payrollLines.Sum(x => x.EmployerTaxAmount)),
                AdvanceAmount = Round(payrollLines.Sum(x => x.AdvanceAmount)),
                NetAmount = Round(payrollLines.Sum(x => x.NetAmount)),
                PayableAmount = Round(payrollLines.Sum(x => x.PayableAmount)),
                Note = dto.Note,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = now,
                CreatedByUserId = _userContext.Id,
                Lines = payrollLines
            };

            await _command.CreateAsync(document, ct);
            _auditLogService.SetNewValues(await GetRequiredDtoInternalAsync(document.Id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayPayrollDoc, document.Id.ToString(), AuditLogOperationTypeConst.Create);
            return Result.Success(document.Id);
        }, ct);

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ConfirmAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.SALARY, id, ct);
            var document = await GetAggregateAsync(id, ct);
            if (document is null)
                return Result.Failure(PayrollErrors.NotFound("PayrollDocument", id, _userContext.LanguageId));
            if (document.StatusId == DocumentStatusIdConst.POSTED)
                return await GetActivePostingBatchAsync(id, ct) is not null
                    ? Result.Success()
                    : Result.Failure(PayrollErrors.Conflict("MissingPostingBatch", $"Oylik hisoblash hujjati tasdiqlangan, ammo faol buxgalteriya o‘tkazmalari to‘plami topilmadi (hujjat ID: {id})."));
            if (document.StatusId is not (DocumentStatusIdConst.DRAFT or DocumentStatusIdConst.PENDING))
                return Result.Failure(PayrollErrors.InvalidStatus("PayrollDocument", id, document.StatusId, "confirmed"));
            if (document.Period.Status != PayrollPeriodStatusConst.Open)
                return Result.Failure(PayrollErrors.PeriodClosed(document.PeriodId));

            var accountingPeriod = await _accountingPeriodValidator.EnsureOpenAsync(document.OrganizationId, document.DocDate, ct);
            if (!accountingPeriod.IsSuccess)
                return accountingPeriod;
            if (document.Lines.Count == 0 ||
                document.Lines.All(x =>
                    x.AdvanceAmount == 0m &&
                    x.CalcLines.All(calc => calc.Amount == 0m)))
                return Result.Failure(PayrollErrors.Business("EmptyPayroll", "Oylik hisoblash hujjatida hisob-kitob qatorlari mavjud emas."));
            if (await GetActivePostingBatchAsync(id, ct) is not null ||
                await _accountingEntryQuery.AnyAsync(x =>
                    x.DocumentTypeId == DocumentTypeIdConst.SALARY &&
                    x.DocumentId == id &&
                    x.ReversalEntryId == null, ct))
                return Result.Failure(PayrollErrors.Conflict("BusinessEffectsExist", $"Oylik hisoblash hujjati bo‘yicha buxgalteriya o‘tkazmalari allaqachon yaratilgan (hujjat ID: {id})."));

            _auditLogService.SetOldValues(await GetRequiredDtoInternalAsync(id, ct));
            var now = DateTime.Now;
            var batch = new PostingBatch
            {
                OrganizationId = document.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.SALARY,
                DocumentId = document.Id,
                Status = PostingBatchStatusConst.POSTED,
                PostedByUserId = _userContext.Id,
                PostedAt = now,
                Comment = "Payroll confirmed"
            };
            await _postingBatchCommand.CreateAsync(batch, ct);

            var postingResult = await _dispatcher.ProcessAsync(document, ct, batch.Id);
            if (!postingResult.IsSuccess)
                return Result.Failure(postingResult.Error);

            document.StatusId = DocumentStatusIdConst.POSTED;
            document.PostedAt = now;
            document.PostedByUserId = _userContext.Id;
            document.UpdatedDate = now;
            document.UpdatedByUserId = _userContext.Id;
            await _command.UpdateAsync(document, ct);

            _auditLogService.SetNewValues(await GetRequiredDtoInternalAsync(id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayPayrollDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Confirmed");
            return Result.Success();
        }, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.SALARY, id, ct);
            var document = await GetAggregateAsync(id, ct);
            if (document is null)
                return Result.Failure(PayrollErrors.NotFound("PayrollDocument", id, _userContext.LanguageId));
            if (document.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();
            if (document.Period.Status != PayrollPeriodStatusConst.Open)
                return Result.Failure(PayrollErrors.PeriodClosed(document.PeriodId));
            if (await _paymentBatchQuery.AnyAsync(x =>
                    x.PayrollDocId == id &&
                    x.StateId == StateIdConst.ACTIVE &&
                    x.StatusId == DocumentStatusIdConst.POSTED, ct))
                return Result.Failure(PayrollErrors.Conflict("PayrollHasPayments", "Oylik hisoblash hujjatini bekor qilishdan oldin unga tegishli tasdiqlangan to‘lovlarni bekor qilish kerak."));

            _auditLogService.SetOldValues(await GetRequiredDtoInternalAsync(id, ct));
            var now = DateTime.Now;
            if (document.StatusId == DocumentStatusIdConst.POSTED)
            {
                var activeBatch = await GetActivePostingBatchAsync(id, ct);
                if (activeBatch is null)
                    return Result.Failure(PayrollErrors.Conflict("MissingPostingBatch", $"Oylik hisoblash hujjatining faol buxgalteriya o‘tkazmalari to‘plami topilmadi (hujjat ID: {id})."));

                var reversalBatch = new PostingBatch
                {
                    OrganizationId = document.OrganizationId,
                    DocumentTypeId = DocumentTypeIdConst.SALARY,
                    DocumentId = document.Id,
                    Status = PostingBatchStatusConst.REVERSAL,
                    PostedByUserId = _userContext.Id,
                    PostedAt = now,
                    Comment = "Payroll cancelled"
                };
                await _postingBatchCommand.CreateAsync(reversalBatch, ct);
                await ReverseEntriesAsync(id, reversalBatch.Id, ct);

                activeBatch.Status = PostingBatchStatusConst.REVERSED;
                activeBatch.ReversedAt = now;
                activeBatch.ReversedByUserId = _userContext.Id;
                await _postingBatchCommand.UpdateAsync(activeBatch, ct);
            }

            document.StatusId = DocumentStatusIdConst.CANCELLED;
            document.CancelledAt = now;
            document.CancelledByUserId = _userContext.Id;
            document.UpdatedDate = now;
            document.UpdatedByUserId = _userContext.Id;
            await _command.UpdateAsync(document, ct);
            _auditLogService.SetNewValues(await GetRequiredDtoInternalAsync(id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayPayrollDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Cancelled");
            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            var document = await GetAggregateAsync(id, ct);
            if (document is null)
                return Result.Failure(PayrollErrors.NotFound("PayrollDocument", id, _userContext.LanguageId));
            if (document.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(PayrollErrors.InvalidStatus("PayrollDocument", id, document.StatusId, "deleted"));

            _auditLogService.SetOldValues(await GetRequiredDtoInternalAsync(id, ct));
            document.StateId = StateIdConst.PASSIVE;
            document.UpdatedDate = DateTime.Now;
            document.UpdatedByUserId = _userContext.Id;
            await _command.UpdateAsync(document, ct);
            _auditLogService.SetNewValues(await GetRequiredDtoInternalAsync(id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayPayrollDoc, id.ToString(), AuditLogOperationTypeConst.Delete);
            return Result.Success();
        }, ct);

    private PayPayrollLine BuildPayrollLine(
        int organizationId,
        PayEmployment employment,
        PayTimesheetLine time,
        PayPeriod period,
        List<PayComponent> components,
        Dictionary<(long EmployeeId, int ComponentId), PayEmployeeComponent> assignments,
        Dictionary<(long EmployeeId, int ComponentId), PayrollManualAdjustmentDto> manualAdjustments,
        decimal advance,
        string documentKind)
    {
        var employeeId = employment.EmployeeId;
        var calcLines = new List<PayPayrollCalcLine>();
        var earnings = components
            .Where(x => x.ComponentType == PayrollComponentTypeConst.Earning)
            .OrderBy(x => x.SortOrder)
            .ToList();

        decimal gross = 0m;
        foreach (var component in earnings.Where(x => x.CalculationMethod != PayrollCalculationMethodConst.PercentOfGross))
        {
            var calc = CalculateComponent(
                organizationId, component, employment, time, period, gross,
                assignments.GetValueOrDefault((employeeId, component.Id)),
                manualAdjustments.GetValueOrDefault((employeeId, component.Id)));
            calcLines.Add(calc);
            gross += calc.Amount;
        }
        foreach (var component in earnings.Where(x => x.CalculationMethod == PayrollCalculationMethodConst.PercentOfGross))
        {
            var calc = CalculateComponent(
                organizationId, component, employment, time, period, gross,
                assignments.GetValueOrDefault((employeeId, component.Id)),
                manualAdjustments.GetValueOrDefault((employeeId, component.Id)));
            calcLines.Add(calc);
            gross += calc.Amount;
        }
        gross = Round(gross);

        foreach (var component in components
                     .Where(x => x.ComponentType != PayrollComponentTypeConst.Earning)
                     .OrderBy(x => x.SortOrder))
        {
            calcLines.Add(CalculateComponent(
                organizationId, component, employment, time, period, gross,
                assignments.GetValueOrDefault((employeeId, component.Id)),
                manualAdjustments.GetValueOrDefault((employeeId, component.Id))));
        }

        var deductions = Round(calcLines
            .Where(x => x.Component.ComponentType == PayrollComponentTypeConst.Deduction)
            .Sum(x => x.Amount));
        var employerTax = Round(calcLines
            .Where(x => x.Component.ComponentType == PayrollComponentTypeConst.EmployerTax)
            .Sum(x => x.Amount));
        var net = Round(gross - deductions);
        var appliedAdvance = documentKind == PayrollDocumentKindConst.Regular ? Round(advance) : 0m;

        return new PayPayrollLine
        {
            OrganizationId = organizationId,
            EmployeeId = employeeId,
            EmploymentId = employment.Id,
            Employment = employment,
            WorkedDays = time.WorkedDays,
            WorkedHours = time.WorkedHours,
            GrossAmount = gross,
            DeductionAmount = deductions,
            EmployerTaxAmount = employerTax,
            AdvanceAmount = appliedAdvance,
            NetAmount = net,
            PayableAmount = Round(net - appliedAdvance),
            CalcLines = calcLines
        };
    }

    private static PayPayrollCalcLine CalculateComponent(
        int organizationId,
        PayComponent component,
        PayEmployment employment,
        PayTimesheetLine time,
        PayPeriod period,
        decimal gross,
        PayEmployeeComponent? assignment,
        PayrollManualAdjustmentDto? manual)
    {
        decimal baseAmount;
        decimal? quantity = null;
        decimal? rate = assignment?.Rate ?? component.DefaultRate;
        decimal amount;

        if (manual is not null)
        {
            baseAmount = gross;
            amount = manual.Amount;
        }
        else
        {
            switch (component.CalculationMethod)
            {
                case PayrollCalculationMethodConst.SalaryProrated:
                    baseAmount = employment.MonthlySalary * employment.EmploymentRate;
                    quantity = time.WorkedDays;
                    var employeeNormDays = time.NormWorkDays > 0m
                        ? time.NormWorkDays
                        : period.NormWorkDays;
                    amount = employeeNormDays == 0m
                        ? 0m
                        : baseAmount * time.WorkedDays / employeeNormDays;
                    break;
                case PayrollCalculationMethodConst.Fixed:
                    baseAmount = assignment?.Amount ?? component.DefaultAmount ?? 0m;
                    amount = baseAmount;
                    break;
                case PayrollCalculationMethodConst.PercentOfGross:
                    baseAmount = gross;
                    amount = baseAmount * (rate ?? 0m) / 100m;
                    break;
                case PayrollCalculationMethodConst.PerHour:
                    baseAmount = rate ?? 0m;
                    quantity = time.WorkedHours;
                    amount = baseAmount * time.WorkedHours;
                    break;
                default:
                    throw new InvalidOperationException($"Qo‘llab-quvvatlanmaydigan oylik hisoblash usuli: '{component.CalculationMethod}'.");
            }
        }

        return new PayPayrollCalcLine
        {
            OrganizationId = organizationId,
            ComponentId = component.Id,
            Component = component,
            BaseAmount = Round(baseAmount),
            Quantity = quantity,
            Rate = rate,
            Amount = Round(amount),
            IsManual = manual is not null,
            Note = manual?.Note
        };
    }

    private async Task<PayPeriod?> GetPeriodAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayPeriod>().Where(x => x.Id == id).Build();
        return await _periodQuery.GetAsync(query, ct);
    }

    private async Task<PayTimesheet?> GetPostedTimesheetAsync(long periodId, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayTimesheet>()
            .Where(x =>
                x.PeriodId == periodId &&
                x.StateId == StateIdConst.ACTIVE &&
                x.StatusId == DocumentStatusIdConst.POSTED)
            .Build();
        query.AddIncludes(x => x.Include(t => t.Lines));
        return await _timesheetQuery.GetAsync(query, ct);
    }

    private async Task<List<PayComponent>> GetActiveComponentsAsync(PayPeriod period, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayComponent>()
            .Where(x =>
                x.StateId == StateIdConst.ACTIVE &&
                x.EffectiveFrom <= period.EndDate &&
                (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= period.StartDate))
            .Build();
        return await _componentQuery.GetAllAsync(query, ct);
    }

    private async Task<List<PayEmployment>> GetEmploymentsAsync(List<long> employeeIds, PayPeriod period, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayEmployment>()
            .Where(x =>
                employeeIds.Contains(x.EmployeeId) &&
                x.StateId == StateIdConst.ACTIVE &&
                x.StartDate <= period.EndDate &&
                (!x.EndDate.HasValue || x.EndDate.Value >= period.StartDate))
            .Build();
        query.AddIncludes(x => x.Include(e => e.Employee));
        return await _employmentQuery.GetAllAsync(query, ct);
    }

    private async Task<List<PayEmployeeComponent>> GetAssignmentsAsync(List<long> employeeIds, PayPeriod period, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayEmployeeComponent>()
            .Where(x =>
                employeeIds.Contains(x.EmployeeId) &&
                x.StateId == StateIdConst.ACTIVE &&
                x.EffectiveFrom <= period.EndDate &&
                (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= period.StartDate))
            .Build();
        return await _assignmentQuery.GetAllAsync(query, ct);
    }

    private async Task<Dictionary<long, decimal>> GetPostedAdvancesAsync(long periodId, List<long> employeeIds, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayPaymentLine>()
            .Where(x =>
                employeeIds.Contains(x.EmployeeId) &&
                x.PaymentBatch.PeriodId == periodId &&
                x.PaymentBatch.PaymentKind == PayrollPaymentKindConst.Advance &&
                x.PaymentBatch.StatusId == DocumentStatusIdConst.POSTED &&
                x.PaymentBatch.StateId == StateIdConst.ACTIVE)
            .As(x => new { x.EmployeeId, x.Amount })
            .Build();
        var items = await _paymentLineQuery.GetAllAsync(query, ct);
        return items.GroupBy(x => x.EmployeeId).ToDictionary(x => x.Key, x => x.Sum(y => y.Amount));
    }

    private async Task<PayPayrollDoc?> GetAggregateAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayPayrollDoc>().Where(x => x.Id == id).Build();
        query.AddIncludes(x => x.Include(d => d.Period));
        query.AddIncludes(x => x.Include(d => d.Lines).ThenInclude(l => l.Employee));
        query.AddIncludes(x => x.Include(d => d.Lines).ThenInclude(l => l.Employment));
        query.AddIncludes(x => x.Include(d => d.Lines).ThenInclude(l => l.CalcLines).ThenInclude(c => c.Component));
        return await _query.GetAsync(query, ct);
    }

    private async Task<PostingBatch?> GetActivePostingBatchAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PostingBatch>()
            .Where(x =>
                x.DocumentTypeId == DocumentTypeIdConst.SALARY &&
                x.DocumentId == id &&
                x.Status == PostingBatchStatusConst.POSTED)
            .Build();
        return await _postingBatchQuery.GetAsync(query, ct);
    }

    private async Task ReverseEntriesAsync(long id, long reversalBatchId, CancellationToken ct)
    {
        var query = _queryBuilder.For<AccountingRegisterEntry>()
            .Where(x =>
                x.DocumentTypeId == DocumentTypeIdConst.SALARY &&
                x.DocumentId == id &&
                x.ReversalEntryId == null)
            .Build();
        query.AddIncludes(x => x.Include(e => e.RegisterEntrySubkontos));
        var entries = await _accountingEntryQuery.GetAllAsync(query, ct);
        var now = DateTime.Now;
        var reversals = entries.Select(entry => new AccountingRegisterEntry
        {
            OrganizationId = entry.OrganizationId,
            DocumentTypeId = entry.DocumentTypeId,
            DocumentId = entry.DocumentId,
            DebitAccountId = entry.CreditAccountId,
            CreditAccountId = entry.DebitAccountId,
            CurrencyId = entry.CurrencyId,
            Amount = entry.Amount,
            DocDate = now,
            CreatedDate = now,
            OperationTypeId = entry.OperationTypeId,
            DebitQuantity = entry.CreditQuantity,
            CreditQuantity = entry.DebitQuantity,
            Content = $"Reversal: {entry.Content}",
            JournalNumber = entry.JournalNumber,
            PostingBatchId = reversalBatchId,
            SourceLineId = entry.SourceLineId,
            ReversalEntryId = entry.Id,
            RegisterEntrySubkontos = entry.RegisterEntrySubkontos.Select(subkonto => new RegisterEntrySubkonto
            {
                Side = subkonto.Side == SubkontoSideConst.DEBIT ? SubkontoSideConst.CREDIT : SubkontoSideConst.DEBIT,
                SubkontoTypeId = subkonto.SubkontoTypeId,
                SortOrder = subkonto.SortOrder,
                EntityId = subkonto.EntityId,
                DisplayValue = subkonto.DisplayValue,
                CreatedDate = now
            }).ToList()
        }).ToList();
        if (reversals.Count > 0)
            await _accountingEntryCommand.CreateAsync(reversals, ct);
    }

    private async Task<PayrollDocumentDto?> GetDtoInternalAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayPayrollDoc>()
            .Where(x => x.Id == id)
            .As(x => new PayrollDocumentDto
            {
                Id = x.Id,
                OrganizationId = x.OrganizationId,
                DocNumber = x.DocNumber,
                DocDate = x.DocDate,
                PeriodId = x.PeriodId,
                PeriodName = x.Period.PeriodYear + "-" + x.Period.PeriodMonth,
                DocumentKind = x.DocumentKind,
                CorrectionOfDocId = x.CorrectionOfDocId,
                CurrencyId = x.CurrencyId,
                StatusId = x.StatusId,
                StatusName = x.Status.Name,
                GrossAmount = x.GrossAmount,
                DeductionAmount = x.DeductionAmount,
                EmployerTaxAmount = x.EmployerTaxAmount,
                AdvanceAmount = x.AdvanceAmount,
                NetAmount = x.NetAmount,
                PayableAmount = x.PayableAmount,
                Note = x.Note,
                StateId = x.StateId,
                CreatedDate = x.CreatedDate,
                PostedAt = x.PostedAt,
                CancelledAt = x.CancelledAt,
                Lines = x.Lines.OrderBy(line => line.Employee.LastName).ThenBy(line => line.Employee.FirstName)
                    .Select(line => new PayrollLineDto
                    {
                        Id = line.Id,
                        EmployeeId = line.EmployeeId,
                        EmployeeNumber = line.Employee.EmployeeNumber,
                        EmployeeName = line.Employee.LastName + " " + line.Employee.FirstName,
                        EmploymentId = line.EmploymentId,
                        DepartmentName = line.Employment.Department != null ? line.Employment.Department.Name : null,
                        PositionName = line.Employment.Position != null ? line.Employment.Position.Name : null,
                        WorkedDays = line.WorkedDays,
                        WorkedHours = line.WorkedHours,
                        GrossAmount = line.GrossAmount,
                        DeductionAmount = line.DeductionAmount,
                        EmployerTaxAmount = line.EmployerTaxAmount,
                        AdvanceAmount = line.AdvanceAmount,
                        NetAmount = line.NetAmount,
                        PayableAmount = line.PayableAmount,
                        CalcLines = line.CalcLines.OrderBy(calc => calc.Component.SortOrder)
                            .Select(calc => new PayrollCalcLineDto
                            {
                                Id = calc.Id,
                                ComponentId = calc.ComponentId,
                                ComponentCode = calc.Component.Code,
                                ComponentName = calc.Component.Name,
                                ComponentType = calc.Component.ComponentType,
                                CalculationMethod = calc.Component.CalculationMethod,
                                BaseAmount = calc.BaseAmount,
                                Quantity = calc.Quantity,
                                Rate = calc.Rate,
                                Amount = calc.Amount,
                                IsManual = calc.IsManual,
                                Note = calc.Note
                            }).ToList()
                    }).ToList()
            })
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<PayrollDocumentDto> GetRequiredDtoInternalAsync(long id, CancellationToken ct) =>
        await GetDtoInternalAsync(id, ct)
        ?? throw new InvalidOperationException("Payroll document audit snapshot is unavailable.");

    private static decimal Round(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
