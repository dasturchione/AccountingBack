using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.DocumentNumbers;
using Application.Features.Register.AccountingRegisterEntries;
using Application.Features.Pay.Taxes;
using Application.Features.Pay.Components;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Application.Features.Pay.PayrollDocuments;

public sealed class PayrollDocumentService : BaseService, IPayrollDocumentService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAccountingPeriodValidator _accountingPeriodValidator;
    private readonly IAccountingDispatcher _dispatcher;
    private readonly IQueryRepository<PayPayrollDoc> _query;
    private readonly ICommandRepository<PayPayrollDoc> _command;
    private readonly IQueryRepository<PayPayrollRecalculation> _recalculationQuery;
    private readonly ICommandRepository<PayPayrollRecalculation> _recalculationCommand;
    private readonly IQueryRepository<PayPeriod> _periodQuery;
    private readonly IQueryRepository<PayTimesheet> _timesheetQuery;
    private readonly IQueryRepository<PayEmployment> _employmentQuery;
    private readonly IQueryRepository<PayComponent> _componentQuery;
    private readonly IQueryRepository<PayTaxDefinition> _taxDefinitionQuery;
    private readonly IQueryRepository<ChartAccount> _accountQuery;
    private readonly IQueryRepository<PayEmployeeComponent> _assignmentQuery;
    private readonly IQueryRepository<PayPaymentLine> _paymentLineQuery;
    private readonly IQueryRepository<PayPaymentBatch> _paymentBatchQuery;
    private readonly IQueryRepository<PostingBatch> _postingBatchQuery;
    private readonly ICommandRepository<PostingBatch> _postingBatchCommand;
    private readonly IQueryRepository<AccountingRegisterEntry> _accountingEntryQuery;
    private readonly ICommandRepository<AccountingRegisterEntry> _accountingEntryCommand;
    private readonly IQueryRepository<DocumentAccountSetting> _documentAccountSettingQuery;
    private readonly IQueryRepository<PayPayrollCalcLine> _calcLineQuery;
    private readonly IQueryRepository<HrAbsenceType> _absenceTypeQuery;
    private readonly IQueryRepository<PayPayrollLine> _payrollLineQuery;
    private readonly IUnitOfWork _serviceUnitOfWork;

    public PayrollDocumentService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IDocumentNumberService documentNumberService,
        IDocumentPostingLock postingLock,
        IAccountingPeriodValidator accountingPeriodValidator,
        IAccountingDispatcher dispatcher,
        IQueryRepository<PayPayrollDoc> query,
        ICommandRepository<PayPayrollDoc> command,
        IQueryRepository<PayPayrollRecalculation> recalculationQuery,
        ICommandRepository<PayPayrollRecalculation> recalculationCommand,
        IQueryRepository<PayPeriod> periodQuery,
        IQueryRepository<PayTimesheet> timesheetQuery,
        IQueryRepository<PayEmployment> employmentQuery,
        IQueryRepository<PayComponent> componentQuery,
        IQueryRepository<PayTaxDefinition> taxDefinitionQuery,
        IQueryRepository<ChartAccount> accountQuery,
        IQueryRepository<PayEmployeeComponent> assignmentQuery,
        IQueryRepository<PayPaymentLine> paymentLineQuery,
        IQueryRepository<PayPaymentBatch> paymentBatchQuery,
        IQueryRepository<PostingBatch> postingBatchQuery,
        ICommandRepository<PostingBatch> postingBatchCommand,
        IQueryRepository<AccountingRegisterEntry> accountingEntryQuery,
        ICommandRepository<AccountingRegisterEntry> accountingEntryCommand,
        IQueryRepository<DocumentAccountSetting> documentAccountSettingQuery,
        IQueryRepository<PayPayrollCalcLine> calcLineQuery,
        IQueryRepository<HrAbsenceType> absenceTypeQuery,
        IQueryRepository<PayPayrollLine> payrollLineQuery,
        ILogger<PayrollDocumentService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _documentNumberService = documentNumberService;
        _postingLock = postingLock;
        _accountingPeriodValidator = accountingPeriodValidator;
        _dispatcher = dispatcher;
        _query = query;
        _command = command;
        _recalculationQuery = recalculationQuery;
        _recalculationCommand = recalculationCommand;
        _periodQuery = periodQuery;
        _timesheetQuery = timesheetQuery;
        _employmentQuery = employmentQuery;
        _componentQuery = componentQuery;
        _taxDefinitionQuery = taxDefinitionQuery;
        _accountQuery = accountQuery;
        _assignmentQuery = assignmentQuery;
        _paymentLineQuery = paymentLineQuery;
        _paymentBatchQuery = paymentBatchQuery;
        _postingBatchQuery = postingBatchQuery;
        _postingBatchCommand = postingBatchCommand;
        _accountingEntryQuery = accountingEntryQuery;
        _accountingEntryCommand = accountingEntryCommand;
        _documentAccountSettingQuery = documentAccountSettingQuery;
        _calcLineQuery = calcLineQuery;
        _absenceTypeQuery = absenceTypeQuery;
        _payrollLineQuery = payrollLineQuery;
        _serviceUnitOfWork = unitOfWork;
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
                    CorrectionPayoutMode = x.CorrectionPayoutMode,
                    StatusId = x.StatusId,
                    StatusName = x.Status.Name,
                    GrossAmount = x.GrossAmount,
                    DeductionAmount = x.DeductionAmount,
                    EmployerTaxAmount = x.EmployerTaxAmount,
                    NetAmount = x.NetAmount,
                    PayableAmount = x.PayableAmount,
                    HasPendingRecalculation = x.RecalculationRequests.Any(request =>
                        request.Status == PayrollRecalculationStatusConst.Pending ||
                        request.Status == PayrollRecalculationStatusConst.Processing),
                    PendingRecalculationId = x.RecalculationRequests
                        .Where(request => request.Status == PayrollRecalculationStatusConst.Pending ||
                                          request.Status == PayrollRecalculationStatusConst.Processing)
                        .OrderByDescending(request => request.Id)
                        .Select(request => (long?)request.Id)
                        .FirstOrDefault()
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

            // 1C-style one-click: fall back to the organization's default payroll
            // posting accounts when the caller does not supply them. The admin can
            // still override every account on the DRAFT document afterwards.
            var defaultAccounts = await GetDefaultPostingAccountsAsync(organizationId, ct);
            var salaryExpenseAccountId = dto.SalaryExpenseAccountId ?? defaultAccounts.SalaryExpenseAccountId;
            var salaryPayableAccountId = dto.SalaryPayableAccountId ?? defaultAccounts.SalaryPayableAccountId;
            if (salaryExpenseAccountId is null)
                return Result.Failure<long>(PayrollErrors.DefaultPostingAccountMissing(PayrollAccountRoleCodeConst.SalaryExpense, _userContext.LanguageId));
            if (salaryPayableAccountId is null)
                return Result.Failure<long>(PayrollErrors.DefaultPostingAccountMissing(PayrollAccountRoleCodeConst.SalaryPayable, _userContext.LanguageId));
            dto.SalaryExpenseAccountId = salaryExpenseAccountId;
            dto.SalaryPayableAccountId = salaryPayableAccountId;

            var accountIds = new[] { salaryExpenseAccountId.Value, salaryPayableAccountId.Value }.Distinct().ToList();
            var accountQuery = _queryBuilder.For<ChartAccount>()
                .Where(x =>
                    x.OrganizationId == organizationId &&
                    x.StateId == StateIdConst.ACTIVE &&
                    accountIds.Contains(x.Id))
                .As(x => x.Id)
                .Build();
            var existingAccountIds = await _accountQuery.GetAllAsync(accountQuery, ct);
            var missingAccountId = accountIds.Except(existingAccountIds).FirstOrDefault();
            if (missingAccountId > 0)
                return Result.Failure<long>(PayrollErrors.ReferencedRecordNotFound("ChartAccount", missingAccountId, _userContext.LanguageId));

            var period = await GetPeriodAsync(dto.PeriodId, ct);
            if (period is null)
                return Result.Failure<long>(PayrollErrors.NotFound("Period", dto.PeriodId, _userContext.LanguageId));
            if (period.Status != PayrollPeriodStatusConst.Open)
                return Result.Failure<long>(PayrollErrors.PeriodClosed(period.Id, _userContext.LanguageId));

            var kind = dto.DocumentKind.Trim().ToUpperInvariant();
            if (kind == PayrollDocumentKindConst.Regular &&
                await _query.AnyAsync(x =>
                    x.PeriodId == period.Id &&
                    x.DocumentKind == PayrollDocumentKindConst.Regular &&
                    x.StateId == StateIdConst.ACTIVE &&
                    x.StatusId != DocumentStatusIdConst.CANCELLED, ct))
                return Result.Failure<long>(PayrollErrors.RegularPayrollAlreadyExists(period.Id, _userContext.LanguageId));

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
                    return Result.Failure<long>(PayrollErrors.CorrectionSourceRequired(_userContext.LanguageId));
                if (dto.Adjustments.Count == 0)
                    return Result.Failure<long>(PayrollErrors.Business("EmptyCorrection", "Tuzatish hujjatida kamida bitta qo‘lda kiritilgan tuzatish bo‘lishi kerak.", _userContext.LanguageId));

                // 1C-style delta: when the accountant sends a TargetAmount (new absolute
                // value), record only the difference against the currently posted amount so
                // the correction never re-adds the original salary. Raw Amount is kept as-is.
                var sourceAmounts = await GetPostedComponentAmountsAsync(correctionSource.Id, ct);
                foreach (var adjustment in dto.Adjustments)
                {
                    if (adjustment.Amount.HasValue == adjustment.TargetAmount.HasValue)
                        return Result.Failure<long>(PayrollErrors.AdjustmentAmountOrTargetRequired(
                            adjustment.EmployeeId, adjustment.ComponentId, _userContext.LanguageId));
                    var current = sourceAmounts.GetValueOrDefault((adjustment.EmployeeId, adjustment.ComponentId));
                    adjustment.Amount = PayrollCorrectionDeltaResolver.Resolve(
                        adjustment.TargetAmount, adjustment.Amount, current);
                }
            }

            var duplicateAdjustment = dto.Adjustments
                .GroupBy(x => new { x.EmployeeId, x.ComponentId })
                .FirstOrDefault(x => x.Count() > 1);
            if (duplicateAdjustment is not null)
                return Result.Failure<long>(PayrollErrors.DuplicateAdjustment(
                    duplicateAdjustment.Key.EmployeeId,
                    duplicateAdjustment.Key.ComponentId,
                    _userContext.LanguageId));
            if (kind == PayrollDocumentKindConst.Regular && dto.Adjustments.Any(x => x.Amount < 0))
                return Result.Failure<long>(PayrollErrors.Business("NegativeRegularAdjustment", "Manfiy summa faqat tuzatish hujjatida qo‘lda kiritilishi mumkin.", _userContext.LanguageId));

            var timesheet = await GetPostedTimesheetAsync(period.Id, ct);
            if (timesheet is null)
                return Result.Failure<long>(PayrollErrors.NoPostedTimesheet(period.Id, _userContext.LanguageId));

            var components = await GetActiveComponentsAsync(
                period,
                DateOnly.FromDateTime(dto.DocDate),
                ct);
            if (components.Count == 0)
                return Result.Failure<long>(PayrollErrors.NoCalculationComponents(period.Id, _userContext.LanguageId));
            if (kind == PayrollDocumentKindConst.Regular &&
                !components.Any(x =>
                    x.IsMandatory &&
                    x.ComponentType == PayrollComponentTypeConst.Earning &&
                    x.CalculationMethod == PayrollCalculationMethodConst.SalaryProrated))
                return Result.Failure<long>(PayrollErrors.MissingBaseSalaryComponent(_userContext.LanguageId));

            // Manual correction documents use the tax snapshot of their source
            // and therefore do not invent a fresh statutory tax amount. The
            // automatic recalculation path below compares source/current tax
            // lines explicitly.
            var taxDefinitions = kind == PayrollDocumentKindConst.Regular
                ? await GetActiveTaxDefinitionsAsync(
                    organizationId,
                    DateOnly.FromDateTime(dto.DocDate),
                    ct)
                : [];
            var duplicateTaxDefinition = taxDefinitions
                .GroupBy(x => x.Code)
                .FirstOrDefault(x => x.Count() > 1);
            if (duplicateTaxDefinition is not null)
                return Result.Failure<long>(PayrollErrors.Conflict(
                    "TaxDefinitionEffectiveDateOverlap",
                    $"'{duplicateTaxDefinition.Key}' soliq qoidasining tanlangan sana uchun bir nechta faol versiyasi mavjud.",
                    _userContext.LanguageId));

            var employeeIds = kind == PayrollDocumentKindConst.Regular
                ? timesheet.Lines.Select(x => x.EmployeeId).Distinct().ToList()
                : dto.Adjustments.Select(x => x.EmployeeId).Distinct().ToList();

            var employments = await GetEmploymentsAsync(employeeIds, period, ct);
            var employmentByEmployee = employments
                .GroupBy(x => x.EmployeeId)
                .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.StartDate).First());
            var missingEmployment = employeeIds.FirstOrDefault(id => !employmentByEmployee.ContainsKey(id));
            if (missingEmployment > 0)
                return Result.Failure<long>(PayrollErrors.NoActiveEmployment(missingEmployment, _userContext.LanguageId));

            var assignments = await GetAssignmentsAsync(
                employeeIds,
                period,
                DateOnly.FromDateTime(dto.DocDate),
                ct);
            var assignmentHistory = kind == PayrollDocumentKindConst.Regular
                ? await GetAssignmentHistoryAsync(employeeIds, period, ct)
                : new Dictionary<long, List<PayEmployeeComponent>>();
            var assignmentMap = assignments
                .GroupBy(x => (x.EmployeeId, x.ComponentId))
                .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.EffectiveFrom).First());
            var manualMap = dto.Adjustments.ToDictionary(x => (x.EmployeeId, x.ComponentId));
            var componentById = components.ToDictionary(x => x.Id);
            var invalidComponent = dto.Adjustments.FirstOrDefault(x => !componentById.ContainsKey(x.ComponentId));
            if (invalidComponent is not null)
                return Result.Failure<long>(PayrollErrors.ReferencedRecordNotFound("Component", invalidComponent.ComponentId, _userContext.LanguageId));

            var timesheetByEmployee = timesheet.Lines.ToDictionary(x => x.EmployeeId);
            var advances = kind == PayrollDocumentKindConst.Regular
                ? await GetPostedAdvancesAsync(period.Id, employeeIds, ct)
                : new Dictionary<long, decimal>();

            // Average-earnings benefits (отпускные/больничные): lookback base and the
            // sick-benefit percentages per absence type. Regular runs only.
            var averageEarnings = kind == PayrollDocumentKindConst.Regular
                ? await GetAverageEarningsAsync(employeeIds, period, ct)
                : new Dictionary<long, (decimal Gross, decimal WorkedDays)>();
            var sickBenefitPercentByType = kind == PayrollDocumentKindConst.Regular
                ? await GetSickBenefitPercentsAsync(ct)
                : new Dictionary<short, decimal>();

            var payrollLines = new List<PayPayrollLine>();
            foreach (var employeeId in employeeIds)
            {
                if (!timesheetByEmployee.TryGetValue(employeeId, out var time))
                    return Result.Failure<long>(PayrollErrors.Business("EmployeeMissingFromTimesheet", $"Xodim tasdiqlangan tabelga kiritilmagan (xodim ID: {employeeId}).", _userContext.LanguageId));

                var selectedComponents = kind == PayrollDocumentKindConst.Correction
                    ? components.Where(component => manualMap.ContainsKey((employeeId, component.Id))).ToList()
                    : components.Where(component =>
                        component.IsMandatory ||
                        assignmentMap.ContainsKey((employeeId, component.Id)) ||
                        manualMap.ContainsKey((employeeId, component.Id))).ToList();

                var invalidReclassification = selectedComponents.FirstOrDefault(component =>
                    component.ComponentType == PayrollComponentTypeConst.Reclassification &&
                    (!component.ExpenseAccountId.HasValue || !component.LiabilityAccountId.HasValue));
                if (invalidReclassification is not null)
                    return Result.Failure<long>(PayrollErrors.ReclassificationAccountsRequired(
                        invalidReclassification.Id,
                        _userContext.LanguageId));

                var missingComponentAccount = selectedComponents.FirstOrDefault(component =>
                    (component.ComponentType == PayrollComponentTypeConst.Deduction &&
                     !component.LiabilityAccountId.HasValue) ||
                    (component.ComponentType == PayrollComponentTypeConst.EmployerTax &&
                     (!component.ExpenseAccountId.HasValue || !component.LiabilityAccountId.HasValue)));
                if (missingComponentAccount is not null)
                    return Result.Failure<long>(PayrollErrors.Business(
                        "ComponentPostingAccountsRequired",
                        $"'{missingComponentAccount.Name}' komponenti uchun provodka hisobvaraqlari ko‘rsatilishi kerak (ID: {missingComponentAccount.Id}).",
                        _userContext.LanguageId));

                var employeeEmployments = employments.Where(x => x.EmployeeId == employeeId).ToList();

                var hasLeaveBenefit = selectedComponents.Any(c => c.CalculationMethod == PayrollCalculationMethodConst.AverageLeave);
                var hasSickBenefit = selectedComponents.Any(c => c.CalculationMethod == PayrollCalculationMethodConst.AverageSick);
                var employment = employmentByEmployee[employeeId];
                var averages = averageEarnings.GetValueOrDefault(employeeId);
                var dailyAverage = PayrollAverageEarningsCalculator.DailyAverage(averages.Gross, averages.WorkedDays);
                if (dailyAverage <= 0m)
                    dailyAverage = period.NormWorkDays > 0m
                        ? Round(employment.MonthlySalary * employment.EmploymentRate / period.NormWorkDays)
                        : 0m;
                var weightedSickDays = hasSickBenefit
                    ? ComputeWeightedSickDays(time, sickBenefitPercentByType)
                    : 0m;
                var benefit = new PayrollBenefitContext(dailyAverage, weightedSickDays, hasLeaveBenefit, hasSickBenefit);

                var segmentComputations = kind == PayrollDocumentKindConst.Regular
                    ? ComputeSegments(period, time, employeeEmployments, !hasLeaveBenefit, !hasSickBenefit)
                    : [];

                var line = BuildPayrollLineWithTaxes(
                    organizationId,
                    employment,
                    time,
                    period,
                    selectedComponents,
                    taxDefinitions,
                    assignmentMap,
                    manualMap,
                    advances.GetValueOrDefault(employeeId),
                    kind,
                    segmentComputations,
                    benefit);
                if (kind == PayrollDocumentKindConst.Regular)
                {
                    line.Segments = BuildPayrollLineSegments(
                        organizationId,
                        line,
                        segmentComputations,
                        employeeEmployments,
                        selectedComponents,
                        assignmentHistory.GetValueOrDefault(employeeId) ?? []);
                }
                AssignPostingAccounts(line, dto);
                payrollLines.Add(line);
            }

            if (payrollLines.Count == 0)
                return Result.Failure<long>(PayrollErrors.Business("NoPayrollLines", "Oylik hisoblash natijasida xodimlar bo‘yicha hech qanday qator hosil bo‘lmadi.", _userContext.LanguageId));

            var postingAccountValidation = await ValidatePostingAccountsAsync(
                organizationId,
                payrollLines,
                ct);
            if (!postingAccountValidation.IsSuccess)
                return Result.Failure<long>(postingAccountValidation.Error);

            var payrollCurrencyIds = payrollLines
                .Select(x => x.Employment.CurrencyId)
                .Distinct()
                .ToList();
            if (payrollCurrencyIds.Count != 1)
                return Result.Failure<long>(PayrollErrors.Business(
                    "MixedPayrollCurrencies",
                    "Bitta oylik hisoblash hujjatidagi barcha xodimlarning ish haqi valyutasi bir xil bo‘lishi kerak.",
                    _userContext.LanguageId));
            if (correctionSource is not null && correctionSource.CurrencyId != payrollCurrencyIds[0])
                return Result.Failure<long>(PayrollErrors.Business(
                    "CorrectionCurrencyMismatch",
                    $"Tuzatish hujjati valyutasi (ID: {payrollCurrencyIds[0]}) asosiy oylik hujjati valyutasiga (ID: {correctionSource.CurrencyId}) mos kelmaydi.",
                    _userContext.LanguageId));

            var documentNumberResult = await _documentNumberService.GetNextAsync(
                organizationId,
                DocumentTypeIdConst.SALARY,
                dto.DocDate,
                ct);
            if (!documentNumberResult.IsSuccess)
                return Result.Failure<long>(documentNumberResult.Error);

            var now = DateTime.Now;
            var document = new PayPayrollDoc
            {
                OrganizationId = organizationId,
                PeriodId = period.Id,
                DocNumber = documentNumberResult.Value.DocumentNumber,
                DocDate = dto.DocDate,
                DocumentKind = kind,
                CorrectionOfDocId = dto.CorrectionOfDocId,
                CorrectionPayoutMode = PayrollDocumentPaymentPolicy.NormalizePayoutMode(kind, dto.CorrectionPayoutMode),
                CurrencyId = payrollCurrencyIds[0],
                StatusId = DocumentStatusIdConst.DRAFT,
                SalaryExpenseAccountId = dto.SalaryExpenseAccountId,
                SalaryPayableAccountId = dto.SalaryPayableAccountId,
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

    public Task<Result<long>> RecalculateAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(RecalculateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.SALARY, id, ct);
            var document = await GetAggregateAsync(id, ct);
            if (document is null)
                return Result.Failure<long>(PayrollErrors.NotFound("PayrollDocument", id, _userContext.LanguageId));

            if (!PayrollRecalculationPolicy.CanRequest(document.StatusId))
                return Result.Failure<long>(PayrollErrors.RecalculationRequiresPosted(id, document.StatusId, _userContext.LanguageId));

            if (document.DocumentKind != PayrollDocumentKindConst.Regular)
                return Result.Failure<long>(PayrollErrors.Business(
                    "RecalculationRegularOnly",
                    "Avtomatik qayta hisoblash faqat asosiy oylik hujjati uchun ishlaydi; tuzatish hujjatini alohida tuzating.",
                    _userContext.LanguageId));

            var postedTimesheet = await GetPostedTimesheetAsync(document.PeriodId, ct);
            if (postedTimesheet is null)
                return Result.Failure<long>(PayrollErrors.NoPostedTimesheet(document.PeriodId, _userContext.LanguageId));

            var sourceRevision = await BuildRecalculationSourceRevisionAsync(
                document,
                postedTimesheet,
                ct);
            var activeRequestQuery = _queryBuilder.For<PayPayrollRecalculation>()
                .Where(x => x.PayrollDocId == id &&
                            x.SourceRevision == sourceRevision &&
                            (x.Status == PayrollRecalculationStatusConst.Pending ||
                             x.Status == PayrollRecalculationStatusConst.Processing ||
                             x.Status == PayrollRecalculationStatusConst.Completed))
                .As(x => x.Id)
                .Build();
            var activeRequestId = await _recalculationQuery.GetAsync(activeRequestQuery, ct);
            if (activeRequestId > 0)
                return Result.Success(activeRequestId);

            var request = new PayPayrollRecalculation
            {
                OrganizationId = document.OrganizationId,
                PayrollDocId = document.Id,
                Status = PayrollRecalculationStatusConst.Pending,
                Reason = "Manual recalculation requested",
                SourceRevision = sourceRevision,
                RequestedDate = DateTime.Now,
                RequestedByUserId = _userContext.Id
            };
            await _recalculationCommand.CreateAsync(request, ct);

            // Move the request through PROCESSING before rebuilding the
            // snapshot.  Keeping this transition explicit makes the queue
            // observable and leaves a safe state if processing is later moved
            // to a hosted worker.
            request.Status = PayrollRecalculationStatusConst.Processing;
            await _recalculationCommand.UpdateAsync(request, ct);

            Result<long> correctionResult;
            try
            {
                correctionResult = await BuildRecalculationCorrectionAsync(document, postedTimesheet, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                request.Status = PayrollRecalculationStatusConst.Failed;
                request.ErrorMessage = ex.Message;
                request.CompletedDate = DateTime.Now;
                await _recalculationCommand.UpdateAsync(request, ct);
                // Preserve the failed queue item even though the API reports
                // the processing error to the caller. BaseService's rollback
                // is a no-op after this explicit commit.
                await _serviceUnitOfWork.CommitAsync(ct);
                return Result.Failure<long>(PayrollErrors.Business(
                    "RecalculationFailed",
                    "Qayta hisoblash jarayonida xatolik yuz berdi.",
                    _userContext.LanguageId));
            }
            if (!correctionResult.IsSuccess)
            {
                request.Status = PayrollRecalculationStatusConst.Failed;
                request.ErrorMessage = correctionResult.Error.Description;
                request.CompletedDate = DateTime.Now;
                await _recalculationCommand.UpdateAsync(request, ct);
                await _serviceUnitOfWork.CommitAsync(ct);
                return Result.Failure<long>(correctionResult.Error);
            }

            request.Status = PayrollRecalculationStatusConst.Completed;
            request.CorrectionDocId = correctionResult.Value > 0 ? correctionResult.Value : null;
            request.CompletedDate = DateTime.Now;
            await _recalculationCommand.UpdateAsync(request, ct);
            return Result.Success(request.Id);
        }, ct);

    private async Task<string> BuildRecalculationSourceRevisionAsync(
        PayPayrollDoc source,
        PayTimesheet timesheet,
        CancellationToken ct)
    {
        var parts = new List<string>
        {
            $"doc:{source.Id}:{(source.UpdatedDate ?? source.PostedAt ?? source.CreatedDate).Ticks}",
            $"timesheet:{timesheet.Id}:{(timesheet.UpdatedDate ?? timesheet.PostedAt ?? timesheet.CreatedDate).Ticks}"
        };

        var components = await GetActiveComponentsAsync(
            source.Period,
            DateOnly.FromDateTime(source.DocDate),
            ct);
        parts.AddRange(components
            .OrderBy(x => x.Id)
            .Select(x =>
                $"component:{x.Id}:{x.Code}:{x.ComponentType}:{x.CalculationMethod}:{x.ProrationBasis}:" +
                $"{x.DefaultAmount}:{x.DefaultRate}:{x.IsMandatory}:{x.ExpenseAccountId}:{x.LiabilityAccountId}:" +
                $"{x.EffectiveFrom}:{x.EffectiveTo}:{x.UpdatedDate ?? x.CreatedDate}"));

        var taxDefinitions = await GetActiveTaxDefinitionsAsync(
            source.OrganizationId,
            DateOnly.FromDateTime(source.DocDate),
            ct);
        parts.AddRange(taxDefinitions
            .OrderBy(x => x.Id)
            .Select(x =>
                $"tax:{x.Id}:{x.Code}:{x.TaxType}:{x.BaseType}:{x.Rate}:{x.ExemptionAmount}:{x.LimitAmount}:" +
                $"{x.LiabilityAccountId}:{x.EffectiveFrom}:{x.EffectiveTo}:{x.UpdatedDate ?? x.CreatedDate}"));

        var employeeIds = timesheet.Lines.Select(x => x.EmployeeId).Distinct().ToList();
        var employments = await GetEmploymentsAsync(employeeIds, source.Period, ct);
        parts.AddRange(employments
            .OrderBy(x => x.Id)
            .Select(x =>
                $"employment:{x.Id}:{x.EmployeeId}:{x.StartDate}:{x.EndDate}:{x.MonthlySalary}:{x.EmploymentRate}:" +
                $"{x.CurrencyId}:{x.ExpenseAccountId}:{x.UpdatedDate ?? x.CreatedDate}"));

        var assignments = await GetAssignmentsAsync(
            employeeIds,
            source.Period,
            DateOnly.FromDateTime(source.DocDate),
            ct);
        parts.AddRange(assignments
            .OrderBy(x => x.Id)
            .Select(x =>
                $"assignment:{x.Id}:{x.EmployeeId}:{x.ComponentId}:{x.Amount}:{x.Rate}:" +
                $"{x.EffectiveFrom}:{x.EffectiveTo}:{x.UpdatedDate ?? x.CreatedDate}"));

        var advances = await GetPostedAdvancesAsync(source.PeriodId, employeeIds, ct);
        parts.AddRange(advances.OrderBy(x => x.Key).Select(x => $"advance:{x.Key}:{x.Value}"));

        var payload = string.Join("|", parts);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash);
    }

    /// <summary>
    /// Rebuilds the regular payroll from the current posted timesheet and the
    /// effective payroll rules, then stores only the difference as a draft
    /// correction document.  The posted source document is deliberately never
    /// mutated: this is the same audit-safe pattern used for manual payroll
    /// corrections.
    /// </summary>
    private async Task<Result<long>> BuildRecalculationCorrectionAsync(
        PayPayrollDoc source,
        PayTimesheet timesheet,
        CancellationToken ct)
    {
        if (source.Period.Status != PayrollPeriodStatusConst.Open)
            return Result.Failure<long>(PayrollErrors.PeriodClosed(source.PeriodId, _userContext.LanguageId));

        if (!source.SalaryExpenseAccountId.HasValue)
            return Result.Failure<long>(PayrollErrors.StoredPostingAccountMissing(
                "salary expense",
                source.Id,
                _userContext.LanguageId));
        if (!source.SalaryPayableAccountId.HasValue)
            return Result.Failure<long>(PayrollErrors.StoredPostingAccountMissing(
                "salary payable",
                source.Id,
                _userContext.LanguageId));

        var components = await GetActiveComponentsAsync(
            source.Period,
            DateOnly.FromDateTime(source.DocDate),
            ct);
        if (components.Count == 0)
            return Result.Failure<long>(PayrollErrors.NoCalculationComponents(source.PeriodId, _userContext.LanguageId));
        if (!components.Any(x =>
                x.IsMandatory &&
                x.ComponentType == PayrollComponentTypeConst.Earning &&
                x.CalculationMethod == PayrollCalculationMethodConst.SalaryProrated))
            return Result.Failure<long>(PayrollErrors.MissingBaseSalaryComponent(_userContext.LanguageId));

        var taxDefinitions = await GetActiveTaxDefinitionsAsync(
            source.OrganizationId,
            DateOnly.FromDateTime(source.DocDate),
            ct);
        var duplicateTaxDefinition = taxDefinitions
            .GroupBy(x => x.Code)
            .FirstOrDefault(x => x.Count() > 1);
        if (duplicateTaxDefinition is not null)
            return Result.Failure<long>(PayrollErrors.Conflict(
                "TaxDefinitionEffectiveDateOverlap",
                $"'{duplicateTaxDefinition.Key}' soliq qoidasining tanlangan sana uchun bir nechta faol versiyasi mavjud.",
                _userContext.LanguageId));

        var timesheetEmployeeIds = timesheet.Lines
            .Select(x => x.EmployeeId)
            .Distinct()
            .ToList();
        var allEmployeeIds = source.Lines
            .Select(x => x.EmployeeId)
            .Concat(timesheetEmployeeIds)
            .Distinct()
            .ToList();

        var employments = await GetEmploymentsAsync(timesheetEmployeeIds, source.Period, ct);
        var employmentByEmployee = employments
            .GroupBy(x => x.EmployeeId)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.StartDate).First());
        var missingEmployment = timesheetEmployeeIds.FirstOrDefault(id => !employmentByEmployee.ContainsKey(id));
        if (missingEmployment > 0)
            return Result.Failure<long>(PayrollErrors.NoActiveEmployment(missingEmployment, _userContext.LanguageId));

        var currencyMismatch = employmentByEmployee.Values.FirstOrDefault(x => x.CurrencyId != source.CurrencyId);
        if (currencyMismatch is not null)
            return Result.Failure<long>(PayrollErrors.Business(
                "RecalculationCurrencyMismatch",
                $"Qayta hisoblashdagi xodim ish haqi valyutasi asosiy hujjat valyutasiga mos kelmaydi (xodim ID: {currencyMismatch.EmployeeId}).",
                _userContext.LanguageId));

        var assignments = await GetAssignmentsAsync(
            timesheetEmployeeIds,
            source.Period,
            DateOnly.FromDateTime(source.DocDate),
            ct);
        var assignmentMap = assignments
            .GroupBy(x => (x.EmployeeId, x.ComponentId))
            .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.EffectiveFrom).First());
        var sourceByEmployee = source.Lines
            .GroupBy(x => x.EmployeeId)
            .ToDictionary(x => x.Key, x => x.First());
        var timesheetByEmployee = timesheet.Lines
            .GroupBy(x => x.EmployeeId)
            .ToDictionary(x => x.Key, x => x.First());
        var advances = await GetPostedAdvancesAsync(source.PeriodId, timesheetEmployeeIds, ct);
        var averageEarnings = await GetAverageEarningsAsync(timesheetEmployeeIds, source.Period, ct);
        var sickBenefitPercentByType = await GetSickBenefitPercentsAsync(ct);

        var accounts = new PayrollCalculateDto
        {
            SalaryExpenseAccountId = source.SalaryExpenseAccountId.Value,
            SalaryPayableAccountId = source.SalaryPayableAccountId.Value
        };
        var freshLines = new Dictionary<long, PayPayrollLine>();

        foreach (var employeeId in timesheetEmployeeIds)
        {
            var employment = employmentByEmployee[employeeId];
            var sourceLine = sourceByEmployee.GetValueOrDefault(employeeId);
            var manualMap = sourceLine?.CalcLines
                .Where(x => x.IsManual)
                .GroupBy(x => x.ComponentId)
                .ToDictionary(
                    x => (employeeId, x.Key),
                    x => new PayrollManualAdjustmentDto
                    {
                        EmployeeId = employeeId,
                        ComponentId = x.Key,
                        Amount = x.First().Amount,
                        Note = x.First().Note
                    })
                ?? new Dictionary<(long EmployeeId, int ComponentId), PayrollManualAdjustmentDto>();

            var selectedComponents = components
                .Where(component =>
                    component.IsMandatory ||
                    assignmentMap.ContainsKey((employeeId, component.Id)) ||
                    manualMap.ContainsKey((employeeId, component.Id)))
                .ToList();

            var invalidReclassification = selectedComponents.FirstOrDefault(component =>
                component.ComponentType == PayrollComponentTypeConst.Reclassification &&
                (!component.ExpenseAccountId.HasValue || !component.LiabilityAccountId.HasValue));
            if (invalidReclassification is not null)
                return Result.Failure<long>(PayrollErrors.ReclassificationAccountsRequired(
                    invalidReclassification.Id,
                    _userContext.LanguageId));

            var missingComponentAccount = selectedComponents.FirstOrDefault(component =>
                (component.ComponentType == PayrollComponentTypeConst.Deduction &&
                 !component.LiabilityAccountId.HasValue) ||
                (component.ComponentType == PayrollComponentTypeConst.EmployerTax &&
                 (!component.ExpenseAccountId.HasValue || !component.LiabilityAccountId.HasValue)));
            if (missingComponentAccount is not null)
                return Result.Failure<long>(PayrollErrors.Business(
                    "ComponentPostingAccountsRequired",
                    $"'{missingComponentAccount.Name}' komponenti uchun provodka hisobvaraqlari ko‘rsatilishi kerak (ID: {missingComponentAccount.Id}).",
                    _userContext.LanguageId));

            var hasLeaveBenefit = selectedComponents.Any(c => c.CalculationMethod == PayrollCalculationMethodConst.AverageLeave);
            var hasSickBenefit = selectedComponents.Any(c => c.CalculationMethod == PayrollCalculationMethodConst.AverageSick);
            var averages = averageEarnings.GetValueOrDefault(employeeId);
            var dailyAverage = PayrollAverageEarningsCalculator.DailyAverage(averages.Gross, averages.WorkedDays);
            if (dailyAverage <= 0m)
                dailyAverage = source.Period.NormWorkDays > 0m
                    ? Round(employment.MonthlySalary * employment.EmploymentRate / source.Period.NormWorkDays)
                    : 0m;
            var weightedSickDays = hasSickBenefit
                ? ComputeWeightedSickDays(timesheetByEmployee[employeeId], sickBenefitPercentByType)
                : 0m;
            var benefit = new PayrollBenefitContext(dailyAverage, weightedSickDays, hasLeaveBenefit, hasSickBenefit);

            var recalcSegments = ComputeSegments(
                source.Period,
                timesheetByEmployee[employeeId],
                employments.Where(x => x.EmployeeId == employeeId).ToList(),
                !hasLeaveBenefit,
                !hasSickBenefit);
            var fresh = BuildPayrollLineWithTaxes(
                source.OrganizationId,
                employment,
                timesheetByEmployee[employeeId],
                source.Period,
                selectedComponents,
                taxDefinitions,
                assignmentMap,
                manualMap,
                advances.GetValueOrDefault(employeeId),
                PayrollDocumentKindConst.Correction,
                recalcSegments,
                benefit);
            AssignPostingAccounts(fresh, accounts);
            freshLines[employeeId] = fresh;
        }

        var correctionLines = new List<PayPayrollLine>();
        foreach (var employeeId in allEmployeeIds)
        {
            var sourceLine = sourceByEmployee.GetValueOrDefault(employeeId);
            var fresh = freshLines.GetValueOrDefault(employeeId) ?? CreateZeroPayrollLine(sourceLine!);
            var original = sourceLine ?? CreateZeroPayrollLine(fresh);
            var delta = PayrollRecalculationDeltaCalculator.Calculate(original, fresh);
            var correctionLine = BuildRecalculationDeltaLine(original, fresh, delta);
            if (correctionLine is not null)
                correctionLines.Add(correctionLine);
        }

        if (correctionLines.Count == 0)
            return Result.Success(0L);

        var postingAccountValidation = await ValidatePostingAccountsAsync(
            source.OrganizationId,
            correctionLines,
            ct);
        if (!postingAccountValidation.IsSuccess)
            return Result.Failure<long>(postingAccountValidation.Error);

        var documentNumberResult = await _documentNumberService.GetNextAsync(
            source.OrganizationId,
            DocumentTypeIdConst.SALARY,
            source.DocDate,
            ct);
        if (!documentNumberResult.IsSuccess)
            return Result.Failure<long>(documentNumberResult.Error);

        var now = DateTime.Now;
        var correction = new PayPayrollDoc
        {
            OrganizationId = source.OrganizationId,
            PeriodId = source.PeriodId,
            DocNumber = documentNumberResult.Value.DocumentNumber,
            DocDate = source.DocDate,
            DocumentKind = PayrollDocumentKindConst.Correction,
            CorrectionOfDocId = source.Id,
            CorrectionPayoutMode = PayrollCorrectionPayoutModeConst.Separate,
            CurrencyId = source.CurrencyId,
            StatusId = DocumentStatusIdConst.DRAFT,
            SalaryExpenseAccountId = source.SalaryExpenseAccountId,
            SalaryPayableAccountId = source.SalaryPayableAccountId,
            GrossAmount = Round(correctionLines.Sum(x => x.GrossAmount)),
            DeductionAmount = Round(correctionLines.Sum(x => x.DeductionAmount)),
            EmployerTaxAmount = Round(correctionLines.Sum(x => x.EmployerTaxAmount)),
            AdvanceAmount = 0m,
            NetAmount = Round(correctionLines.Sum(x => x.NetAmount)),
            PayableAmount = Round(correctionLines.Sum(x => x.PayableAmount)),
            Note = $"Avtomatik qayta hisoblash: {source.DocNumber}",
            StateId = StateIdConst.ACTIVE,
            CreatedDate = now,
            CreatedByUserId = _userContext.Id,
            Lines = correctionLines
        };

        await _command.CreateAsync(correction, ct);
        _auditLogService.SetNewValues(await GetRequiredDtoInternalAsync(correction.Id, ct));
        await _auditLogService.CreateAsync(
            AuditLogTableConst.PayPayrollDoc,
            correction.Id.ToString(),
            AuditLogOperationTypeConst.Create);
        return Result.Success(correction.Id);
    }

    private static PayPayrollLine CreateZeroPayrollLine(PayPayrollLine source)
    {
        return new PayPayrollLine
        {
            OrganizationId = source.OrganizationId,
            EmployeeId = source.EmployeeId,
            EmploymentId = source.EmploymentId,
            Employment = source.Employment,
            Employee = source.Employee,
            CalcLines = [],
            TaxLines = []
        };
    }

    private static PayPayrollLine? BuildRecalculationDeltaLine(
        PayPayrollLine original,
        PayPayrollLine recalculated,
        PayrollRecalculationLineDelta delta)
    {
        var hasFinancialDelta = delta.GrossAmount != 0m ||
                                delta.DeductionAmount != 0m ||
                                delta.EmployerTaxAmount != 0m ||
                                delta.NetAmount != 0m ||
                                delta.ComponentDeltas.Values.Any(x => x != 0m) ||
                                delta.TaxDeltas.Values.Any(x => x != 0m);
        if (!hasFinancialDelta)
            return null;

        var calcReferences = original.CalcLines
            .Concat(recalculated.CalcLines)
            .GroupBy(x => x.ComponentId)
            .ToDictionary(x => x.Key, x => x.Last());
        var taxReferences = original.TaxLines
            .Concat(recalculated.TaxLines)
            .GroupBy(x => x.TaxDefinitionId)
            .ToDictionary(x => x.Key, x => x.Last());

        var calcLines = delta.ComponentDeltas
            .Where(x => x.Value != 0m && calcReferences.ContainsKey(x.Key))
            .Select(x =>
            {
                var originalCalc = original.CalcLines.FirstOrDefault(line => line.ComponentId == x.Key);
                var recalculatedCalc = recalculated.CalcLines.FirstOrDefault(line => line.ComponentId == x.Key);
                var reference = recalculatedCalc ?? originalCalc!;
                return new PayPayrollCalcLine
                {
                    OrganizationId = recalculated.OrganizationId,
                    ComponentId = reference.ComponentId,
                    Component = reference.Component,
                    BaseAmount = Round((recalculatedCalc?.BaseAmount ?? 0m) - (originalCalc?.BaseAmount ?? 0m)),
                    Quantity = recalculatedCalc?.Quantity ?? originalCalc?.Quantity,
                    Rate = recalculatedCalc?.Rate ?? originalCalc?.Rate,
                    Amount = x.Value,
                    DebitAccountId = reference.DebitAccountId,
                    CreditAccountId = reference.CreditAccountId,
                    IsManual = false,
                    Note = "Avtomatik qayta hisoblash farqi"
                };
            })
            .ToList();
        var taxLines = delta.TaxDeltas
            .Where(x => x.Value != 0m && taxReferences.ContainsKey(x.Key))
            .Select(x =>
            {
                var originalTax = original.TaxLines.FirstOrDefault(line => line.TaxDefinitionId == x.Key);
                var recalculatedTax = recalculated.TaxLines.FirstOrDefault(line => line.TaxDefinitionId == x.Key);
                var reference = recalculatedTax ?? originalTax!;
                return new PayPayrollTaxLine
                {
                    OrganizationId = recalculated.OrganizationId,
                    TaxDefinitionId = reference.TaxDefinitionId,
                    TaxDefinition = reference.TaxDefinition,
                    BaseAmount = Round((recalculatedTax?.BaseAmount ?? 0m) - (originalTax?.BaseAmount ?? 0m)),
                    ExemptionAmount = Round((recalculatedTax?.ExemptionAmount ?? 0m) - (originalTax?.ExemptionAmount ?? 0m)),
                    TaxableBase = Round((recalculatedTax?.TaxableBase ?? 0m) - (originalTax?.TaxableBase ?? 0m)),
                    Rate = recalculatedTax?.Rate ?? originalTax?.Rate ?? 0m,
                    Amount = x.Value,
                    LiabilityAccountId = reference.LiabilityAccountId,
                    CreatedDate = DateTime.Now
                };
            })
            .ToList();

        return new PayPayrollLine
        {
            OrganizationId = recalculated.OrganizationId,
            EmployeeId = recalculated.EmployeeId != 0 ? recalculated.EmployeeId : original.EmployeeId,
            EmploymentId = recalculated.EmploymentId != 0 ? recalculated.EmploymentId : original.EmploymentId,
            Employment = recalculated.Employment ?? original.Employment,
            Employee = recalculated.Employee ?? original.Employee,
            WorkedDays = delta.WorkedDays,
            WorkedHours = delta.WorkedHours,
            PaidLeaveDays = delta.PaidLeaveDays,
            PaidSickDays = delta.PaidSickDays,
            OvertimeHours = delta.OvertimeHours,
            NightHours = delta.NightHours,
            HolidayHours = delta.HolidayHours,
            WeekendHours = delta.WeekendHours,
            GrossAmount = delta.GrossAmount,
            DeductionAmount = delta.DeductionAmount,
            EmployerTaxAmount = delta.EmployerTaxAmount,
            AdvanceAmount = 0m,
            NetAmount = delta.NetAmount,
            // A separate correction must not reverse the original advance
            // offset. Its payable amount is the net correction only.
            PayableAmount = delta.NetAmount,
            CalcLines = calcLines,
            TaxLines = taxLines
        };
    }

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
                    : Result.Failure(PayrollErrors.Conflict("MissingPostingBatch", $"Oylik hisoblash hujjati tasdiqlangan, ammo faol buxgalteriya o‘tkazmalari to‘plami topilmadi (hujjat ID: {id}).", _userContext.LanguageId));
            if (document.StatusId is not (DocumentStatusIdConst.DRAFT or DocumentStatusIdConst.PENDING))
                return Result.Failure(PayrollErrors.InvalidStatus("PayrollDocument", id, document.StatusId, "confirmed", _userContext.LanguageId));
            if (document.Period.Status != PayrollPeriodStatusConst.Open)
                return Result.Failure(PayrollErrors.PeriodClosed(document.PeriodId, _userContext.LanguageId));

            var accountingPeriod = await _accountingPeriodValidator.EnsureOpenAsync(document.OrganizationId, document.DocDate, ct);
            if (!accountingPeriod.IsSuccess)
                return accountingPeriod;
            if (document.Lines.Count == 0 ||
                document.Lines.All(x =>
                    x.AdvanceAmount == 0m &&
                    x.CalcLines.All(calc => calc.Amount == 0m)))
                return Result.Failure(PayrollErrors.Business("EmptyPayroll", "Oylik hisoblash hujjatida hisob-kitob qatorlari mavjud emas.", _userContext.LanguageId));
            if (await GetActivePostingBatchAsync(id, ct) is not null ||
                await _accountingEntryQuery.AnyAsync(x =>
                    x.DocumentTypeId == DocumentTypeIdConst.SALARY &&
                    x.DocumentId == id &&
                    x.ReversalEntryId == null, ct))
                return Result.Failure(PayrollErrors.Conflict("BusinessEffectsExist", $"Oylik hisoblash hujjati bo‘yicha buxgalteriya o‘tkazmalari allaqachon yaratilgan (hujjat ID: {id}).", _userContext.LanguageId));

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

    public Task<Result> UpdateDraftAsync(long id, PayrollDraftUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateDraftAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var document = await GetAggregateAsync(id, ct);
            if (document is null)
                return Result.Failure(PayrollErrors.NotFound("PayrollDocument", id, _userContext.LanguageId));
            if (document.StatusId is not (DocumentStatusIdConst.DRAFT or DocumentStatusIdConst.PENDING))
                return Result.Failure(PayrollErrors.InvalidStatus("PayrollDocument", id, document.StatusId, "updated", _userContext.LanguageId));
            if (document.Period.Status != PayrollPeriodStatusConst.Open)
                return Result.Failure(PayrollErrors.PeriodClosed(document.PeriodId, _userContext.LanguageId));
            if (await GetActivePostingBatchAsync(id, ct) is not null)
                return Result.Failure(PayrollErrors.Conflict("BusinessEffectsExist", $"Oylik hisoblash hujjati bo‘yicha buxgalteriya o‘tkazmalari allaqachon yaratilgan (hujjat ID: {id}).", _userContext.LanguageId));

            _auditLogService.SetOldValues(await GetRequiredDtoInternalAsync(id, ct));

            if (dto.SalaryExpenseAccountId.HasValue)
                document.SalaryExpenseAccountId = dto.SalaryExpenseAccountId.Value;
            if (dto.SalaryPayableAccountId.HasValue)
                document.SalaryPayableAccountId = dto.SalaryPayableAccountId.Value;

            var linesById = document.Lines.ToDictionary(x => x.Id);
            foreach (var lineDto in dto.Lines)
            {
                if (!linesById.TryGetValue(lineDto.LineId, out var line))
                    return Result.Failure(PayrollErrors.NotFound("PayrollLine", lineDto.LineId, _userContext.LanguageId));

                var calcById = line.CalcLines.ToDictionary(x => x.Id);
                foreach (var calcDto in lineDto.CalcLines)
                {
                    if (!calcById.TryGetValue(calcDto.CalcLineId, out var calc))
                        return Result.Failure(PayrollErrors.NotFound("PayrollLine", calcDto.CalcLineId, _userContext.LanguageId));
                    if (calcDto.Amount.HasValue)
                    {
                        calc.Amount = Round(calcDto.Amount.Value);
                        calc.IsManual = true;
                    }
                    if (calcDto.DebitAccountId.HasValue)
                        calc.DebitAccountId = calcDto.DebitAccountId.Value;
                    if (calcDto.CreditAccountId.HasValue)
                        calc.CreditAccountId = calcDto.CreditAccountId.Value;
                }

                var taxById = line.TaxLines.ToDictionary(x => x.Id);
                foreach (var taxDto in lineDto.TaxLines)
                {
                    if (!taxById.TryGetValue(taxDto.TaxLineId, out var tax))
                        return Result.Failure(PayrollErrors.NotFound("PayrollLine", taxDto.TaxLineId, _userContext.LanguageId));
                    if (taxDto.Amount.HasValue)
                        tax.Amount = Round(taxDto.Amount.Value);
                    if (taxDto.LiabilityAccountId.HasValue)
                        tax.LiabilityAccountId = taxDto.LiabilityAccountId.Value;
                }

                PayrollDocumentTotalsCalculator.RecomputeLine(line);
            }
            PayrollDocumentTotalsCalculator.RecomputeDocument(document);

            var accountValidation = await ValidatePostingAccountsAsync(document.OrganizationId, document.Lines, ct);
            if (!accountValidation.IsSuccess)
                return accountValidation;

            document.UpdatedDate = DateTime.Now;
            document.UpdatedByUserId = _userContext.Id;
            await _serviceUnitOfWork.SaveChangesAsync(ct);

            _auditLogService.SetNewValues(await GetRequiredDtoInternalAsync(id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayPayrollDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Draft edited");
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
                return Result.Failure(PayrollErrors.PeriodClosed(document.PeriodId, _userContext.LanguageId));
            if (await _paymentBatchQuery.AnyAsync(x =>
                    x.PayrollDocId == id &&
                    x.StateId == StateIdConst.ACTIVE &&
                    x.StatusId == DocumentStatusIdConst.POSTED, ct))
                return Result.Failure(PayrollErrors.Conflict("PayrollHasPayments", "Oylik hisoblash hujjatini bekor qilishdan oldin unga tegishli tasdiqlangan to‘lovlarni bekor qilish kerak.", _userContext.LanguageId));

            _auditLogService.SetOldValues(await GetRequiredDtoInternalAsync(id, ct));
            var now = DateTime.Now;
            if (document.StatusId == DocumentStatusIdConst.POSTED)
            {
                var activeBatch = await GetActivePostingBatchAsync(id, ct);
                if (activeBatch is null)
                    return Result.Failure(PayrollErrors.Conflict("MissingPostingBatch", $"Oylik hisoblash hujjatining faol buxgalteriya o‘tkazmalari to‘plami topilmadi (hujjat ID: {id}).", _userContext.LanguageId));

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
                return Result.Failure(PayrollErrors.InvalidStatus("PayrollDocument", id, document.StatusId, "deleted", _userContext.LanguageId));

            _auditLogService.SetOldValues(await GetRequiredDtoInternalAsync(id, ct));
            document.StateId = StateIdConst.PASSIVE;
            document.UpdatedDate = DateTime.Now;
            document.UpdatedByUserId = _userContext.Id;
            await _command.UpdateAsync(document, ct);
            _auditLogService.SetNewValues(await GetRequiredDtoInternalAsync(id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayPayrollDoc, id.ToString(), AuditLogOperationTypeConst.Delete);
            return Result.Success();
        }, ct);

    // Kept as a compatibility seam for existing callers/tests that build a line
    // without a tax registry. Production calculation uses the tax-aware method.
    private PayPayrollLine BuildPayrollLine(
        int organizationId,
        PayEmployment employment,
        PayTimesheetLine time,
        PayPeriod period,
        List<PayComponent> components,
        Dictionary<(long EmployeeId, int ComponentId), PayEmployeeComponent> assignments,
        Dictionary<(long EmployeeId, int ComponentId), PayrollManualAdjustmentDto> manualAdjustments,
        decimal advance,
        string documentKind) =>
        BuildPayrollLineWithTaxes(
            organizationId,
            employment,
            time,
            period,
            components,
            [],
            assignments,
            manualAdjustments,
            advance,
            documentKind);

    private PayPayrollLine BuildPayrollLineWithTaxes(
        int organizationId,
        PayEmployment employment,
        PayTimesheetLine time,
        PayPeriod period,
        List<PayComponent> components,
        List<PayTaxDefinition> taxDefinitions,
        Dictionary<(long EmployeeId, int ComponentId), PayEmployeeComponent> assignments,
        Dictionary<(long EmployeeId, int ComponentId), PayrollManualAdjustmentDto> manualAdjustments,
        decimal advance,
        string documentKind,
        IReadOnlyList<PayrollSegmentComputation>? segments = null,
        PayrollBenefitContext? benefit = null)
    {
        var benefitContext = benefit ?? PayrollBenefitContext.None;
        var employeeId = employment.EmployeeId;
        var calcLines = new List<PayPayrollCalcLine>();
        var earnings = PayrollComponentFormulaPolicy.Order(
            components.Where(x => x.ComponentType == PayrollComponentTypeConst.Earning));
        var calcAmounts = new Dictionary<int, decimal>();

        decimal gross = 0m;
        foreach (var component in earnings.Where(x => x.CalculationMethod != PayrollCalculationMethodConst.PercentOfGross))
        {
            var calc = CalculateComponent(
                organizationId, component, employment, time, period, gross,
                assignments.GetValueOrDefault((employeeId, component.Id)),
                manualAdjustments.GetValueOrDefault((employeeId, component.Id)),
                component.DependsOnComponentId is { } dependencyId && calcAmounts.TryGetValue(dependencyId, out var dependencyAmount)
                    ? dependencyAmount
                    : null,
                segments,
                benefitContext);
            calcLines.Add(calc);
            calcAmounts[component.Id] = calc.Amount;
            gross += calc.Amount;
        }
        foreach (var component in earnings.Where(x => x.CalculationMethod == PayrollCalculationMethodConst.PercentOfGross))
        {
            var calc = CalculateComponent(
                organizationId, component, employment, time, period, gross,
                assignments.GetValueOrDefault((employeeId, component.Id)),
                manualAdjustments.GetValueOrDefault((employeeId, component.Id)),
                component.DependsOnComponentId is { } dependencyId && calcAmounts.TryGetValue(dependencyId, out var dependencyAmount)
                    ? dependencyAmount
                    : null,
                segments,
                benefitContext);
            calcLines.Add(calc);
            calcAmounts[component.Id] = calc.Amount;
            gross += calc.Amount;
        }
        gross = Round(gross);

        foreach (var component in PayrollComponentFormulaPolicy.Order(
                     components.Where(x => x.ComponentType != PayrollComponentTypeConst.Earning)))
        {
            var calc = CalculateComponent(
                organizationId, component, employment, time, period, gross,
                assignments.GetValueOrDefault((employeeId, component.Id)),
                manualAdjustments.GetValueOrDefault((employeeId, component.Id)),
                component.DependsOnComponentId is { } dependencyId && calcAmounts.TryGetValue(dependencyId, out var dependencyAmount)
                    ? dependencyAmount
                    : null);
            calcLines.Add(calc);
            calcAmounts[component.Id] = calc.Amount;
        }

        var deductions = Round(calcLines
            .Where(x => x.Component.ComponentType == PayrollComponentTypeConst.Deduction)
            .Sum(x => x.Amount));
        var employerTax = Round(calcLines
            .Where(x => x.Component.ComponentType == PayrollComponentTypeConst.EmployerTax)
            .Sum(x => x.Amount));
        var taxLines = taxDefinitions
            .Select(definition => BuildTaxLine(
                organizationId,
                definition,
                gross,
                deductions,
                calcLines
                    .Where(x => x.Component.ComponentType == PayrollComponentTypeConst.Earning && x.Component.IsTaxable)
                    .Sum(x => x.Amount)))
            .Where(line => line.Amount != 0m)
            .ToList();
        deductions = Round(deductions + taxLines
            .Where(line => line.TaxDefinition.TaxType == PayrollTaxTypeConst.Withholding)
            .Sum(line => line.Amount));
        employerTax = Round(employerTax + taxLines
            .Where(line => line.TaxDefinition.TaxType == PayrollTaxTypeConst.Employer)
            .Sum(line => line.Amount));
        var net = Round(gross - deductions);
        var appliedAdvance = documentKind == PayrollDocumentKindConst.Regular ? Round(advance) : 0m;
        var attendance = PayrollAttendanceSnapshotCalculator.FromTimesheet(time);

        return new PayPayrollLine
        {
            OrganizationId = organizationId,
            EmployeeId = employeeId,
            EmploymentId = employment.Id,
            Employment = employment,
            WorkedDays = attendance.WorkedDays,
            WorkedHours = attendance.WorkedHours,
            PaidLeaveDays = attendance.PaidLeaveDays,
            PaidSickDays = attendance.PaidSickDays,
            OvertimeHours = attendance.OvertimeHours,
            NightHours = attendance.NightHours,
            HolidayHours = attendance.HolidayHours,
            WeekendHours = attendance.WeekendHours,
            GrossAmount = gross,
            DeductionAmount = deductions,
            EmployerTaxAmount = employerTax,
            AdvanceAmount = appliedAdvance,
            NetAmount = net,
            PayableAmount = Round(net - appliedAdvance),
            CalcLines = calcLines,
            TaxLines = taxLines
        };
    }

    private static PayPayrollTaxLine BuildTaxLine(
        int organizationId,
        PayTaxDefinition definition,
        decimal gross,
        decimal deductions,
        decimal taxableGross)
    {
        var baseAmount = definition.BaseType switch
        {
            PayrollTaxBaseTypeConst.Net => Math.Max(gross - deductions, 0m),
            PayrollTaxBaseTypeConst.TaxableEarnings => taxableGross,
            _ => gross
        };
        var calculation = PayrollTaxCalculator.Calculate(new PayrollTaxCalculationInput(
            baseAmount,
            definition.Rate,
            definition.ExemptionAmount ?? 0m,
            definition.LimitAmount));
        return new PayPayrollTaxLine
        {
            OrganizationId = organizationId,
            TaxDefinitionId = definition.Id,
            TaxDefinition = definition,
            BaseAmount = Round(baseAmount),
            ExemptionAmount = Round(Math.Max(definition.ExemptionAmount ?? 0m, 0m)),
            TaxableBase = calculation.TaxableBase,
            Rate = definition.Rate,
            Amount = calculation.Amount,
            LiabilityAccountId = definition.LiabilityAccountId
        };
    }

    private static void AssignPostingAccounts(PayPayrollLine line, PayrollCalculateDto accounts)
    {
        foreach (var calc in line.CalcLines)
        {
            (calc.DebitAccountId, calc.CreditAccountId) = calc.Component.ComponentType switch
            {
                PayrollComponentTypeConst.Earning =>
                    (calc.Component.ExpenseAccountId
                         ?? line.Employment.ExpenseAccountId
                         ?? accounts.SalaryExpenseAccountId,
                     calc.Component.LiabilityAccountId
                         ?? accounts.SalaryPayableAccountId),

                PayrollComponentTypeConst.Deduction =>
                    (accounts.SalaryPayableAccountId,
                     calc.Component.LiabilityAccountId!.Value),

                PayrollComponentTypeConst.EmployerTax =>
                    (calc.Component.ExpenseAccountId!.Value,
                     calc.Component.LiabilityAccountId!.Value),

                PayrollComponentTypeConst.Reclassification =>
                    (calc.Component.ExpenseAccountId!.Value,
                     calc.Component.LiabilityAccountId!.Value),

                _ => throw new InvalidOperationException(
                    $"Unsupported payroll component type '{calc.Component.ComponentType}'.")
            };
        }
    }

    private async Task<Result> ValidatePostingAccountsAsync(
        int organizationId,
        IEnumerable<PayPayrollLine> lines,
        CancellationToken ct)
    {
        var accountIds = lines
            .SelectMany(line =>
                line.CalcLines
                    .SelectMany(calc => new[] { calc.DebitAccountId, calc.CreditAccountId })
                    .Concat(line.TaxLines.Select(tax => (int?)tax.LiabilityAccountId)))
            .Where(id => id.HasValue && id.Value > 0)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        if (accountIds.Count == 0)
            return Result.Success();

        var query = _queryBuilder.For<ChartAccount>()
            .Where(x =>
                accountIds.Contains(x.Id) &&
                x.OrganizationId == organizationId &&
                x.StateId == StateIdConst.ACTIVE)
            .As(x => new { x.Id, x.IsGroup })
            .Build();
        var accounts = await _accountQuery.GetAllAsync(query, ct);
        var missingAccount = accountIds
            .Except(accounts.Select(x => x.Id))
            .FirstOrDefault();
        if (missingAccount > 0)
            return Result.Failure(PayrollErrors.ReferencedRecordNotFound(
                "ChartAccount",
                missingAccount,
                _userContext.LanguageId));

        var groupAccounts = accounts
            .Where(x => x.IsGroup)
            .Select(x => x.Id)
            .ToList();
        return groupAccounts.Count == 0
            ? Result.Success()
            : Result.Failure(PayrollErrors.Business(
                "PostingAccountNotPostable",
                $"Guruh hisobvarag‘iga o‘tkazma yozib bo‘lmaydi: {string.Join(", ", groupAccounts)}.",
                _userContext.LanguageId));
    }

    private static PayPayrollCalcLine CalculateComponent(
        int organizationId,
        PayComponent component,
        PayEmployment employment,
        PayTimesheetLine time,
        PayPeriod period,
        decimal gross,
        PayEmployeeComponent? assignment,
        PayrollManualAdjustmentDto? manual,
        decimal? dependencyBase = null,
        IReadOnlyList<PayrollSegmentComputation>? segments = null,
        PayrollBenefitContext? benefit = null)
    {
        var benefitContext = benefit ?? PayrollBenefitContext.None;
        decimal baseAmount;
        decimal? quantity = null;
        decimal? rate = assignment?.Rate ?? component.DefaultRate;
        decimal amount;

        if (manual is not null)
        {
            baseAmount = gross;
            amount = manual.Amount ?? 0m;
        }
        else
        {
            switch (component.CalculationMethod)
            {
                case PayrollCalculationMethodConst.SalaryProrated:
                    var basis = PayrollProrationBasisConst.All.Contains(component.ProrationBasis)
                        ? component.ProrationBasis
                        : PayrollProrationBasisConst.Days;
                    quantity = basis == PayrollProrationBasisConst.Hours
                        ? time.WorkedHours
                        : time.WorkedDays;
                    if (segments is { Count: > 0 })
                    {
                        // 1C-style: prorate the base salary within each employment
                        // segment using that segment's own salary, rate and norm, then
                        // sum. This keeps the header consistent with the stored
                        // segments when salary/position changes mid-period.
                        amount = PayrollSalaryProrationCalculator.CalculateSegmented(
                            segments.Select(s => new PayrollProrationSegment(
                                s.Segment.MonthlySalary,
                                s.Segment.EmploymentRate,
                                s.WorkedDays,
                                s.NormWorkDays,
                                s.WorkedHours,
                                s.NormWorkHours,
                                s.PaidLeaveDays,
                                (s.PaidLeaveDays + s.PaidSickDays) * period.DailyWorkHours,
                                s.PaidSickDays)),
                            basis);
                        var last = segments[^1].Segment;
                        baseAmount = last.MonthlySalary * last.EmploymentRate;
                    }
                    else
                    {
                        baseAmount = employment.MonthlySalary * employment.EmploymentRate;
                        var employeeNormDays = time.NormWorkDays > 0m
                            ? time.NormWorkDays
                            : period.NormWorkDays;
                        var employeeNormHours = time.NormWorkHours > 0m
                            ? time.NormWorkHours
                            : period.NormWorkHours;
                        // When an average-earnings benefit pays these days, exclude them
                        // from the salary base to avoid paying the same day twice.
                        var paidLeaveDays = benefitContext.HasLeaveBenefit ? 0m : time.PaidLeaveDays;
                        var paidSickDays = benefitContext.HasSickBenefit ? 0m : time.PaidSickDays;
                        amount = PayrollSalaryProrationCalculator.Calculate(
                            employment.MonthlySalary,
                            employment.EmploymentRate,
                            time.WorkedDays,
                            employeeNormDays,
                            time.WorkedHours,
                            employeeNormHours,
                            basis,
                            paidLeaveDays,
                            (paidLeaveDays + paidSickDays) * period.DailyWorkHours,
                            paidSickDays);
                    }
                    break;
                case PayrollCalculationMethodConst.AverageLeave:
                    baseAmount = benefitContext.DailyAverage;
                    quantity = time.PaidLeaveDays;
                    amount = PayrollAverageEarningsCalculator.Benefit(benefitContext.DailyAverage, time.PaidLeaveDays, 100m);
                    break;
                case PayrollCalculationMethodConst.AverageSick:
                    baseAmount = benefitContext.DailyAverage;
                    quantity = benefitContext.WeightedSickDays;
                    // Percentages are already folded into WeightedSickDays per absence type.
                    amount = PayrollAverageEarningsCalculator.Benefit(benefitContext.DailyAverage, benefitContext.WeightedSickDays, 100m);
                    break;
                case PayrollCalculationMethodConst.Fixed:
                    baseAmount = assignment?.Amount ?? component.DefaultAmount ?? 0m;
                    amount = baseAmount;
                    break;
                case PayrollCalculationMethodConst.PercentOfGross:
                    baseAmount = dependencyBase ?? gross;
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

        amount = PayrollComponentFormulaPolicy.ApplyCaps(amount, component.MinimumAmount, component.MaximumAmount);

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

    private async Task<(int? SalaryExpenseAccountId, int? SalaryPayableAccountId)> GetDefaultPostingAccountsAsync(
        int organizationId,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<DocumentAccountSetting>()
            .Where(x =>
                x.OrganizationId == organizationId &&
                x.StateId == StateIdConst.ACTIVE &&
                x.IsDefault &&
                x.DocumentAccountTypeRole.DocumentAccountType.Code == PayrollDocumentAccountTypeCodeConst.PayrollAccrual &&
                (x.DocumentAccountTypeRole.DocumentAccountRole.Code == PayrollAccountRoleCodeConst.SalaryExpense ||
                 x.DocumentAccountTypeRole.DocumentAccountRole.Code == PayrollAccountRoleCodeConst.SalaryPayable))
            .As(x => new DefaultAccountRow(
                x.DocumentAccountTypeRole.DocumentAccountRole.Code,
                x.ChartAccountId))
            .Build();
        var rows = await _documentAccountSettingQuery.GetAllAsync(query, ct);
        int? expense = rows
            .Where(r => r.RoleCode == PayrollAccountRoleCodeConst.SalaryExpense)
            .Select(r => (int?)r.ChartAccountId)
            .FirstOrDefault();
        int? payable = rows
            .Where(r => r.RoleCode == PayrollAccountRoleCodeConst.SalaryPayable)
            .Select(r => (int?)r.ChartAccountId)
            .FirstOrDefault();
        return (expense, payable);
    }

    private sealed record DefaultAccountRow(string RoleCode, int ChartAccountId);

    private async Task<PayPeriod?> GetPeriodAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayPeriod>().Where(x => x.Id == id).Build();
        query.AddIncludes(x => x.Include(period => period.WorkDays));
        return await _periodQuery.GetAsync(query, ct);
    }

    public Task<Result<PayrollCorrectionBasisDto>> GetCorrectionBasisAsync(long sourceDocId, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetCorrectionBasisAsync), async () =>
        {
            if (!await _query.AnyAsync(x => x.Id == sourceDocId && x.StatusId == DocumentStatusIdConst.POSTED, ct))
                return Result.Failure<PayrollCorrectionBasisDto>(PayrollErrors.CorrectionSourceRequired(_userContext.LanguageId));

            var query = _queryBuilder.For<PayPayrollCalcLine>()
                .Where(x => x.PayrollLine.PayrollDocId == sourceDocId)
                .As(x => new PayrollCorrectionBasisLineDto
                {
                    EmployeeId = x.PayrollLine.EmployeeId,
                    EmployeeNumber = x.PayrollLine.Employee.EmployeeNumber,
                    EmployeeName = x.PayrollLine.Employee.LastName + " " + x.PayrollLine.Employee.FirstName,
                    ComponentId = x.ComponentId,
                    ComponentCode = x.Component.Code,
                    ComponentName = x.Component.Name,
                    ComponentType = x.Component.ComponentType,
                    CurrentAmount = x.Amount
                })
                .Build();
            var lines = await _calcLineQuery.GetAllAsync(query, ct);
            return Result.Success(new PayrollCorrectionBasisDto
            {
                SourceDocId = sourceDocId,
                Lines = lines
            });
        });

    private async Task<Dictionary<(long EmployeeId, int ComponentId), decimal>> GetPostedComponentAmountsAsync(
        long sourceDocId,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<PayPayrollCalcLine>()
            .Where(x => x.PayrollLine.PayrollDocId == sourceDocId)
            .As(x => new { x.PayrollLine.EmployeeId, x.ComponentId, x.Amount })
            .Build();
        var rows = await _calcLineQuery.GetAllAsync(query, ct);
        return rows
            .GroupBy(x => (x.EmployeeId, x.ComponentId))
            .ToDictionary(g => g.Key, g => g.Sum(y => y.Amount));
    }

    private async Task<PayTimesheet?> GetPostedTimesheetAsync(long periodId, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayTimesheet>()
            .Where(x =>
                x.PeriodId == periodId &&
                x.StateId == StateIdConst.ACTIVE &&
                x.StatusId == DocumentStatusIdConst.POSTED)
            .Build();
        query.AddIncludes(x => x.Include(t => t.Lines).ThenInclude(line => line.Days));
        return await _timesheetQuery.GetAsync(query, ct);
    }

    private async Task<List<PayComponent>> GetActiveComponentsAsync(
        PayPeriod period,
        DateOnly effectiveDate,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<PayComponent>()
            .Where(x =>
                x.OrganizationId == period.OrganizationId &&
                x.StateId == StateIdConst.ACTIVE &&
                x.EffectiveFrom <= effectiveDate &&
                (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= effectiveDate))
            .Build();
        return await _componentQuery.GetAllAsync(query, ct);
    }

    private async Task<List<PayTaxDefinition>> GetActiveTaxDefinitionsAsync(
        int organizationId,
        DateOnly effectiveDate,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<PayTaxDefinition>()
            .Where(x => x.OrganizationId == organizationId &&
                        x.StateId == StateIdConst.ACTIVE &&
                        x.EffectiveFrom <= effectiveDate &&
                        (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= effectiveDate))
            .Build();
        return await _taxDefinitionQuery.GetAllAsync(query, ct);
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

    private async Task<List<PayEmployeeComponent>> GetAssignmentsAsync(
        List<long> employeeIds,
        PayPeriod period,
        DateOnly effectiveDate,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<PayEmployeeComponent>()
            .Where(x =>
                employeeIds.Contains(x.EmployeeId) &&
                x.StateId == StateIdConst.ACTIVE &&
                x.EffectiveFrom <= effectiveDate &&
                (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= effectiveDate))
            .Build();
        return await _assignmentQuery.GetAllAsync(query, ct);
    }

    private async Task<Dictionary<long, List<PayEmployeeComponent>>> GetAssignmentHistoryAsync(
        List<long> employeeIds,
        PayPeriod period,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<PayEmployeeComponent>()
            .Where(x =>
                employeeIds.Contains(x.EmployeeId) &&
                x.StateId == StateIdConst.ACTIVE &&
                x.EffectiveFrom <= period.EndDate &&
                (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= period.StartDate))
            .Build();
        var assignments = await _assignmentQuery.GetAllAsync(query, ct);
        return assignments
            .GroupBy(x => x.EmployeeId)
            .ToDictionary(x => x.Key, x => x.OrderBy(y => y.EffectiveFrom).ThenBy(y => y.Id).ToList());
    }

    private sealed record PayrollBenefitContext(
        decimal DailyAverage,
        decimal WeightedSickDays,
        bool HasLeaveBenefit,
        bool HasSickBenefit)
    {
        public static readonly PayrollBenefitContext None = new(0m, 0m, false, false);
    }

    // Lookback average earnings base: 12 months of regular posted payroll before this
    // period. Daily average = Σ gross / Σ worked days over that window.
    private async Task<Dictionary<long, (decimal Gross, decimal WorkedDays)>> GetAverageEarningsAsync(
        List<long> employeeIds,
        PayPeriod period,
        CancellationToken ct)
    {
        var from = period.StartDate.AddMonths(-12);
        var query = _queryBuilder.For<PayPayrollLine>()
            .Where(x =>
                employeeIds.Contains(x.EmployeeId) &&
                x.PayrollDoc.DocumentKind == PayrollDocumentKindConst.Regular &&
                x.PayrollDoc.StateId == StateIdConst.ACTIVE &&
                x.PayrollDoc.StatusId == DocumentStatusIdConst.POSTED &&
                x.PayrollDoc.Period.StartDate >= from &&
                x.PayrollDoc.Period.StartDate < period.StartDate)
            .As(x => new { x.EmployeeId, x.GrossAmount, x.WorkedDays })
            .Build();
        var rows = await _payrollLineQuery.GetAllAsync(query, ct);
        return rows
            .GroupBy(x => x.EmployeeId)
            .ToDictionary(g => g.Key, g => (g.Sum(y => y.GrossAmount), g.Sum(y => y.WorkedDays)));
    }

    private async Task<Dictionary<short, decimal>> GetSickBenefitPercentsAsync(CancellationToken ct)
    {
        var query = _queryBuilder.For<HrAbsenceType>()
            .Where(x => x.TimesheetCategory == HrTimesheetCategoryConst.Sick && x.IsPaid)
            .As(x => new { x.Id, x.BenefitPercent })
            .Build();
        var rows = await _absenceTypeQuery.GetAllAsync(query, ct);
        return rows.ToDictionary(x => x.Id, x => x.BenefitPercent);
    }

    // Paid sick days weighted by each absence type's benefit percentage.
    private static decimal ComputeWeightedSickDays(PayTimesheetLine time, IReadOnlyDictionary<short, decimal> sickPercentByType)
    {
        var days = time.Days ?? [];
        decimal weighted = 0m;
        foreach (var day in days)
        {
            if (day.TimesheetCategory == HrTimesheetCategoryConst.Sick &&
                day.AbsenceTypeId.HasValue &&
                sickPercentByType.TryGetValue(day.AbsenceTypeId.Value, out var percent))
                weighted += percent / 100m;
        }
        return decimal.Round(weighted, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Splits the payroll period into employment segments and resolves the worked,
    /// norm and paid-absence quantities for each one. This is the single source of
    /// truth shared by the header salary proration and the persisted line segments,
    /// so a mid-period salary/position change is calculated consistently.
    /// </summary>
    private static List<PayrollSegmentComputation> ComputeSegments(
        PayPeriod period,
        PayTimesheetLine time,
        IReadOnlyCollection<PayEmployment> employments,
        bool includePaidLeave = true,
        bool includePaidSick = true)
    {
        var segments = PayrollEmploymentSegmentCalculator.Split(period, employments);
        if (segments.Count == 0)
            return [];

        var periodDays = Math.Max(period.EndDate.DayNumber - period.StartDate.DayNumber + 1, 1);
        var availableTimesheetDays = time.Days ?? [];
        var availableWorkDays = period.WorkDays ?? [];
        var totalSegmentDays = segments.Sum(x => x.EndDate.DayNumber - x.StartDate.DayNumber + 1);

        var computations = segments.Select(segment =>
        {
            var segmentDays = availableTimesheetDays
                .Where(x => x.WorkDate >= segment.StartDate && x.WorkDate <= segment.EndDate)
                .ToList();
            decimal workedDays = segmentDays.Count(x => x.StatusCode == HrCalendarStatusConst.Worked);
            var workedHours = segmentDays
                .Where(x => x.StatusCode == HrCalendarStatusConst.Worked)
                .Sum(x => x.WorkedHours);
            if (segmentDays.Count == 0)
            {
                var ratio = totalSegmentDays == 0
                    ? 0m
                    : (segment.EndDate.DayNumber - segment.StartDate.DayNumber + 1) / (decimal)totalSegmentDays;
                workedDays = time.WorkedDays * ratio;
                workedHours = time.WorkedHours * ratio;
            }

            var segmentWorkDays = availableWorkDays
                .Where(x => x.WorkDate >= segment.StartDate && x.WorkDate <= segment.EndDate && x.IsWorkDay)
                .ToList();
            decimal normWorkDays = segmentWorkDays.Count;
            var normWorkHours = segmentWorkDays.Sum(x => x.WorkHours);
            if (availableWorkDays.Count == 0)
            {
                var ratio = (segment.EndDate.DayNumber - segment.StartDate.DayNumber + 1) / (decimal)periodDays;
                normWorkDays = Math.Round((time.NormWorkDays > 0m ? time.NormWorkDays : period.NormWorkDays) * ratio, 2, MidpointRounding.AwayFromZero);
                normWorkHours = (time.NormWorkHours > 0m ? time.NormWorkHours : period.NormWorkHours) * ratio;
            }

            return new PayrollSegmentComputation
            {
                Segment = segment,
                WorkedDays = workedDays,
                WorkedHours = workedHours,
                NormWorkDays = normWorkDays,
                NormWorkHours = normWorkHours
            };
        }).ToList();

        // Paid leave/sick are tracked at the line level only, so distribute them
        // across segments proportionally to each segment's norm days (falling back
        // to segment length). The monthly totals are conserved by assigning any
        // rounding remainder to the last segment.
        AllocatePaidAbsence(computations, includePaidLeave ? time.PaidLeaveDays : 0m, static (c, v) => c.PaidLeaveDays = v);
        AllocatePaidAbsence(computations, includePaidSick ? time.PaidSickDays : 0m, static (c, v) => c.PaidSickDays = v);
        return computations;
    }

    private static void AllocatePaidAbsence(
        List<PayrollSegmentComputation> computations,
        decimal total,
        Action<PayrollSegmentComputation, decimal> assign)
    {
        if (computations.Count == 0 || total <= 0m)
            return;

        var weights = computations.Select(x => x.NormWorkDays).ToList();
        var weightSum = weights.Sum();
        if (weightSum <= 0m)
        {
            weights = computations
                .Select(x => (decimal)(x.Segment.EndDate.DayNumber - x.Segment.StartDate.DayNumber + 1))
                .ToList();
            weightSum = weights.Sum();
        }
        if (weightSum <= 0m)
        {
            assign(computations[^1], total);
            return;
        }

        decimal allocated = 0m;
        for (var i = 0; i < computations.Count - 1; i++)
        {
            var portion = decimal.Round(total * weights[i] / weightSum, 2, MidpointRounding.AwayFromZero);
            assign(computations[i], portion);
            allocated += portion;
        }
        assign(computations[^1], total - allocated);
    }

    private static List<PayPayrollLineSegment> BuildPayrollLineSegments(
        int organizationId,
        PayPayrollLine line,
        IReadOnlyList<PayrollSegmentComputation> computations,
        IReadOnlyCollection<PayEmployment> employments,
        IReadOnlyCollection<PayComponent> components,
        IReadOnlyCollection<PayEmployeeComponent> assignmentHistory)
    {
        if (computations.Count == 0)
            return [];

        return computations.Select(computation =>
        {
            var segment = computation.Segment;
            var componentSnapshot = components
                .Select(component =>
                {
                    var assignment = assignmentHistory
                        .Where(x => x.ComponentId == component.Id &&
                                    x.EffectiveFrom <= segment.EndDate &&
                                    (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= segment.StartDate))
                        .OrderByDescending(x => x.EffectiveFrom)
                        .ThenByDescending(x => x.Id)
                        .FirstOrDefault();
                    return new PayrollComponentSnapshot(
                        component.Id,
                        component.Code,
                        component.CalculationMethod,
                        component.ProrationBasis,
                        component.DefaultAmount,
                        component.DefaultRate,
                        assignment?.Amount,
                        assignment?.Rate,
                        assignment?.EffectiveFrom,
                        assignment?.EffectiveTo);
                })
                .ToList();

            return new PayPayrollLineSegment
            {
                OrganizationId = organizationId,
                PayrollLine = line,
                Employment = employments.First(x => x.Id == segment.EmploymentId),
                EmploymentId = segment.EmploymentId,
                SegmentStartDate = segment.StartDate,
                SegmentEndDate = segment.EndDate,
                MonthlySalary = segment.MonthlySalary,
                EmploymentRate = segment.EmploymentRate,
                WorkedDays = decimal.Round(computation.WorkedDays, 2),
                WorkedHours = decimal.Round(computation.WorkedHours, 2),
                NormWorkDays = decimal.Round(computation.NormWorkDays, 2),
                NormWorkHours = decimal.Round(computation.NormWorkHours, 2),
                ComponentSnapshotJson = JsonSerializer.Serialize(componentSnapshot)
            };
        }).ToList();
    }

    private sealed class PayrollSegmentComputation
    {
        public required PayrollEmploymentSegment Segment { get; init; }
        public decimal WorkedDays { get; set; }
        public decimal WorkedHours { get; set; }
        public decimal NormWorkDays { get; set; }
        public decimal NormWorkHours { get; set; }
        public decimal PaidLeaveDays { get; set; }
        public decimal PaidSickDays { get; set; }
    }

    private sealed record PayrollComponentSnapshot(
        int ComponentId,
        string ComponentCode,
        string CalculationMethod,
        string ProrationBasis,
        decimal? DefaultAmount,
        decimal? DefaultRate,
        decimal? AssignmentAmount,
        decimal? AssignmentRate,
        DateOnly? AssignmentEffectiveFrom,
        DateOnly? AssignmentEffectiveTo);

    private async Task<Dictionary<long, decimal>> GetPostedAdvancesAsync(long periodId, List<long> employeeIds, CancellationToken ct)
    {
        // Only prepayment advances (not tied to a payroll line) offset the regular final.
        // Advance lines that settle a WITH_ADVANCE correction carry a PayrollLineId and
        // must not reduce the regular document's payable.
        var query = _queryBuilder.For<PayPaymentLine>()
            .Where(x =>
                employeeIds.Contains(x.EmployeeId) &&
                x.PayrollLineId == null &&
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
        query.AddIncludes(x => x.Include(d => d.Lines).ThenInclude(l => l.TaxLines).ThenInclude(t => t.TaxDefinition));
        query.AddIncludes(x => x.Include(d => d.Lines).ThenInclude(l => l.Segments));
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
                CorrectionPayoutMode = x.CorrectionPayoutMode,
                CurrencyId = x.CurrencyId,
                SalaryExpenseAccountId = x.SalaryExpenseAccountId,
                SalaryPayableAccountId = x.SalaryPayableAccountId,
                StatusId = x.StatusId,
                StatusName = x.Status.Name,
                GrossAmount = x.GrossAmount,
                DeductionAmount = x.DeductionAmount,
                EmployerTaxAmount = x.EmployerTaxAmount,
                AdvanceAmount = x.AdvanceAmount,
                NetAmount = x.NetAmount,
                PayableAmount = x.PayableAmount,
                HasPendingRecalculation = x.RecalculationRequests.Any(request =>
                    request.Status == PayrollRecalculationStatusConst.Pending ||
                    request.Status == PayrollRecalculationStatusConst.Processing),
                PendingRecalculationId = x.RecalculationRequests
                    .Where(request => request.Status == PayrollRecalculationStatusConst.Pending ||
                                      request.Status == PayrollRecalculationStatusConst.Processing)
                    .OrderByDescending(request => request.Id)
                    .Select(request => (long?)request.Id)
                    .FirstOrDefault(),
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
                        PaidLeaveDays = line.PaidLeaveDays,
                        PaidSickDays = line.PaidSickDays,
                        OvertimeHours = line.OvertimeHours,
                        NightHours = line.NightHours,
                        HolidayHours = line.HolidayHours,
                        WeekendHours = line.WeekendHours,
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
                                DebitAccountId = calc.DebitAccountId,
                                CreditAccountId = calc.CreditAccountId,
                                IsManual = calc.IsManual,
                                Note = calc.Note
                            }).ToList(),
                        TaxLines = line.TaxLines.OrderBy(tax => tax.TaxDefinition.Code)
                            .Select(tax => new PayrollTaxLineDto
                            {
                                Id = tax.Id,
                                TaxDefinitionId = tax.TaxDefinitionId,
                                TaxCode = tax.TaxDefinition.Code,
                                TaxName = tax.TaxDefinition.Name,
                                TaxType = tax.TaxDefinition.TaxType,
                                BaseType = tax.TaxDefinition.BaseType,
                                BaseAmount = tax.BaseAmount,
                                ExemptionAmount = tax.ExemptionAmount,
                                TaxableBase = tax.TaxableBase,
                                Rate = tax.Rate,
                                Amount = tax.Amount,
                                LiabilityAccountId = tax.LiabilityAccountId
                            }).ToList(),
                        Segments = line.Segments.OrderBy(segment => segment.SegmentStartDate)
                            .Select(segment => new PayrollLineSegmentDto
                            {
                                Id = segment.Id,
                                EmploymentId = segment.EmploymentId,
                                SegmentStartDate = segment.SegmentStartDate,
                                SegmentEndDate = segment.SegmentEndDate,
                                MonthlySalary = segment.MonthlySalary,
                                EmploymentRate = segment.EmploymentRate,
                                WorkedDays = segment.WorkedDays,
                                WorkedHours = segment.WorkedHours,
                                NormWorkDays = segment.NormWorkDays,
                                NormWorkHours = segment.NormWorkHours,
                                ComponentSnapshotJson = segment.ComponentSnapshotJson
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
