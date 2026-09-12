using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.BankOperations;
using Application.Features.CashOperations;
using Application.Features.DocumentNumbers;
using Application.Features.Pay.PayrollDocuments;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Pay.Payments;

public sealed class PayrollPaymentService : BaseService, IPayrollPaymentService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IBankOperationService _bankOperationService;
    private readonly ICashOperationService _cashOperationService;
    private readonly IQueryRepository<PayPaymentBatch> _query;
    private readonly ICommandRepository<PayPaymentBatch> _command;
    private readonly IQueryRepository<PayPaymentLine> _paymentLineQuery;
    private readonly IQueryRepository<PayPeriod> _periodQuery;
    private readonly IQueryRepository<PayPayrollDoc> _payrollDocQuery;
    private readonly IQueryRepository<PayPayrollLine> _payrollLineQuery;
    private readonly IQueryRepository<PayEmployment> _employmentQuery;
    private readonly IQueryRepository<PayEmployee> _employeeQuery;
    private readonly IQueryRepository<BankAccount> _bankAccountQuery;
    private readonly IQueryRepository<CashBox> _cashBoxQuery;
    private readonly IQueryRepository<ChartAccount> _accountQuery;
    private readonly IQueryRepository<Currency> _currencyQuery;

    public PayrollPaymentService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IDocumentNumberService documentNumberService,
        IDocumentPostingLock postingLock,
        IBankOperationService bankOperationService,
        ICashOperationService cashOperationService,
        IQueryRepository<PayPaymentBatch> query,
        ICommandRepository<PayPaymentBatch> command,
        IQueryRepository<PayPaymentLine> paymentLineQuery,
        IQueryRepository<PayPeriod> periodQuery,
        IQueryRepository<PayPayrollDoc> payrollDocQuery,
        IQueryRepository<PayPayrollLine> payrollLineQuery,
        IQueryRepository<PayEmployment> employmentQuery,
        IQueryRepository<PayEmployee> employeeQuery,
        IQueryRepository<BankAccount> bankAccountQuery,
        IQueryRepository<CashBox> cashBoxQuery,
        IQueryRepository<ChartAccount> accountQuery,
        IQueryRepository<Currency> currencyQuery,
        ILogger<PayrollPaymentService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _documentNumberService = documentNumberService;
        _postingLock = postingLock;
        _bankOperationService = bankOperationService;
        _cashOperationService = cashOperationService;
        _query = query;
        _command = command;
        _paymentLineQuery = paymentLineQuery;
        _periodQuery = periodQuery;
        _payrollDocQuery = payrollDocQuery;
        _payrollLineQuery = payrollLineQuery;
        _employmentQuery = employmentQuery;
        _employeeQuery = employeeQuery;
        _bankAccountQuery = bankAccountQuery;
        _cashBoxQuery = cashBoxQuery;
        _accountQuery = accountQuery;
        _currencyQuery = currencyQuery;
    }

    public Task<Result<PagedResponse<PayrollPaymentListDto>>> GetAllAsync(
        PayrollPaymentListFilter filter,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var page = Math.Max(filter.Page, 1);
            var take = Math.Clamp(filter.PageSize ?? 50, 1, 200);
            var search = filter.Search?.Trim().ToLower();
            var specification = _queryBuilder.For<PayPaymentBatch>()
                .Where(x =>
                    x.StateId == StateIdConst.ACTIVE &&
                    (!filter.PeriodId.HasValue || x.PeriodId == filter.PeriodId.Value) &&
                    (!filter.StatusId.HasValue || x.StatusId == filter.StatusId.Value) &&
                    (string.IsNullOrWhiteSpace(filter.PaymentKind) || x.PaymentKind == filter.PaymentKind) &&
                    (string.IsNullOrWhiteSpace(filter.SourceType) || x.SourceType == filter.SourceType) &&
                    (string.IsNullOrWhiteSpace(search) ||
                     x.DocNumber.ToLower().Contains(search) ||
                     (x.Note != null && x.Note.ToLower().Contains(search))))
                .As(x => new PayrollPaymentListDto
                {
                    Id = x.Id,
                    DocNumber = x.DocNumber,
                    DocDate = x.DocDate,
                    PeriodId = x.PeriodId,
                    PeriodName = x.Period.PeriodYear + "-" + x.Period.PeriodMonth,
                    PaymentKind = x.PaymentKind,
                    SourceType = x.SourceType,
                    TotalAmount = x.TotalAmount,
                    StatusId = x.StatusId,
                    StatusName = x.Status.Name,
                    BankOperationId = x.BankOperationId,
                    CashOperationId = x.CashOperationId
                })
                .OrderBy(x => x.OrderByDescending(y => y.DocDate).ThenByDescending(y => y.Id))
                .Skip((page - 1) * take)
                .Take(take)
                .BuildPaged();
            var paged = await _query.GetPagedAsync(specification, ct);
            return Result.Success(PagedResponseFactory.Create(paged, page, take));
        });

    public Task<Result<PayrollPaymentDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var dto = await GetDtoInternalAsync(id, ct);
            return dto is null
                ? Result.Failure<PayrollPaymentDto>(PayrollErrors.NotFound("PaymentBatch", id, _userContext.LanguageId))
                : Result.Success(dto);
        });

    public Task<Result<long>> CreateAsync(PayrollPaymentCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            var periodQuery = _queryBuilder.For<PayPeriod>().Where(x => x.Id == dto.PeriodId).Build();
            var period = await _periodQuery.GetAsync(periodQuery, ct);
            if (period is null)
                return Result.Failure<long>(PayrollErrors.NotFound("Period", dto.PeriodId, _userContext.LanguageId));
            if (period.Status != PayrollPeriodStatusConst.Open)
                return Result.Failure<long>(PayrollErrors.PeriodClosed(period.Id, _userContext.LanguageId));

            var sourceValidation = await ValidateSourceAsync(dto, organizationId, ct);
            if (!sourceValidation.IsSuccess)
                return Result.Failure<long>(sourceValidation.Error);

            var duplicateEmployee = dto.Lines.GroupBy(x => x.EmployeeId).FirstOrDefault(x => x.Count() > 1);
            if (duplicateEmployee is not null)
                return Result.Failure<long>(PayrollErrors.Conflict("DuplicatePaymentEmployee", $"Xodim to‘lov hujjatida takroran kiritilgan (xodim ID: {duplicateEmployee.Key}).", _userContext.LanguageId));

            var employeeIds = dto.Lines.Select(x => x.EmployeeId).Distinct().ToList();
            var employeeQuery = _queryBuilder.For<PayEmployee>()
                .Where(x => employeeIds.Contains(x.Id) &&
                            x.OrganizationId == organizationId &&
                            x.StateId == StateIdConst.ACTIVE)
                .As(x => x.Id)
                .Build();
            var validEmployeeIds = await _employeeQuery.GetAllAsync(employeeQuery, ct);
            var missingEmployee = employeeIds.Except(validEmployeeIds).FirstOrDefault();
            if (missingEmployee > 0)
                return Result.Failure<long>(PayrollErrors.NotFound("Employee", missingEmployee, _userContext.LanguageId));

            var allocationsByEmployee = new Dictionary<long, IReadOnlyList<PayrollPaymentAllocationItem>>();
            if (dto.PaymentKind == PayrollPaymentKindConst.Final)
            {
                if (!dto.PayrollDocId.HasValue)
                    return Result.Failure<long>(PayrollErrors.Business("PayrollDocumentRequired", "Yakuniy to‘lov uchun tasdiqlangan oylik hisoblash hujjati kerak.", _userContext.LanguageId));

                var docQuery = _queryBuilder.For<PayPayrollDoc>()
                    .Where(x =>
                        x.Id == dto.PayrollDocId.Value &&
                        x.PeriodId == period.Id &&
                        x.StatusId == DocumentStatusIdConst.POSTED)
                    .Build();
                var payrollDoc = await _payrollDocQuery.GetAsync(docQuery, ct);
                if (payrollDoc is null)
                    return Result.Failure<long>(PayrollErrors.CorrectionSourceRequired(_userContext.LanguageId));
                if (payrollDoc.CurrencyId != dto.CurrencyId)
                    return Result.Failure<long>(PayrollErrors.Business(
                        "PaymentCurrencyMismatch",
                        $"To‘lov valyutasi (ID: {dto.CurrencyId}) oylik hisoblash valyutasiga (ID: {payrollDoc.CurrencyId}) mos kelmaydi.",
                        _userContext.LanguageId));
                // Only a SEPARATE correction is paid on its own. WITH_SALARY is paid with
                // the regular final; WITH_ADVANCE is paid through the advance vedomost.
                if (payrollDoc.DocumentKind == PayrollDocumentKindConst.Correction)
                {
                    var payoutMode = PayrollDocumentPaymentPolicy.NormalizePayoutMode(payrollDoc.DocumentKind, payrollDoc.CorrectionPayoutMode);
                    if (payoutMode == PayrollCorrectionPayoutModeConst.WithSalary)
                        return Result.Failure<long>(PayrollErrors.WithSalaryCorrectionNotDirectlyPayable(payrollDoc.Id, _userContext.LanguageId));
                    if (payoutMode == PayrollCorrectionPayoutModeConst.WithAdvance)
                        return Result.Failure<long>(PayrollErrors.WithAdvanceCorrectionViaAdvanceOnly(payrollDoc.Id, _userContext.LanguageId));
                }

                var docIds = await GetPayableDocIdsAsync(period.Id, payrollDoc, ct);
                var outstandingLines = await GetOutstandingLinesAsync(docIds, payrollDoc.Id, employeeIds, ct);
                foreach (var line in dto.Lines)
                {
                    if (!outstandingLines.TryGetValue(line.EmployeeId, out var targets) || targets.Count == 0)
                        return Result.Failure<long>(PayrollErrors.Business(
                            "EmployeeMissingFromPayroll",
                            $"Xodim oylik hisoblash hujjatiga kiritilmagan (xodim ID: {line.EmployeeId}, hujjat ID: {payrollDoc.Id}).",
                            _userContext.LanguageId));
                    var available = PayrollPaymentAllocator.TotalOutstanding(targets);
                    if (line.Amount > available)
                        return Result.Failure<long>(PayrollErrors.PaymentExceedsOutstanding(line.EmployeeId, line.Amount, available, _userContext.LanguageId));
                    allocationsByEmployee[line.EmployeeId] = PayrollPaymentAllocator.Allocate(line.Amount, targets);
                }
            }
            else if (dto.PayrollDocId.HasValue)
            {
                // Advance that settles a WITH_ADVANCE correction: per-line, like a final.
                var docQuery = _queryBuilder.For<PayPayrollDoc>()
                    .Where(x =>
                        x.Id == dto.PayrollDocId.Value &&
                        x.PeriodId == period.Id &&
                        x.StatusId == DocumentStatusIdConst.POSTED)
                    .Build();
                var correctionDoc = await _payrollDocQuery.GetAsync(docQuery, ct);
                if (correctionDoc is null)
                    return Result.Failure<long>(PayrollErrors.CorrectionSourceRequired(_userContext.LanguageId));
                if (correctionDoc.CurrencyId != dto.CurrencyId)
                    return Result.Failure<long>(PayrollErrors.Business(
                        "PaymentCurrencyMismatch",
                        $"To‘lov valyutasi (ID: {dto.CurrencyId}) tuzatish hujjati valyutasiga (ID: {correctionDoc.CurrencyId}) mos kelmaydi.",
                        _userContext.LanguageId));
                if (correctionDoc.DocumentKind != PayrollDocumentKindConst.Correction ||
                    PayrollDocumentPaymentPolicy.NormalizePayoutMode(correctionDoc.DocumentKind, correctionDoc.CorrectionPayoutMode) != PayrollCorrectionPayoutModeConst.WithAdvance)
                    return Result.Failure<long>(PayrollErrors.Business(
                        "AdvanceDocumentMustBeWithAdvanceCorrection",
                        $"Avans qaydnomasiga faqat 'avans bilan' to'lanadigan tuzatish hujjati biriktiriladi (hujjat ID: {correctionDoc.Id}).",
                        _userContext.LanguageId));

                var outstandingLines = await GetOutstandingLinesAsync([correctionDoc.Id], correctionDoc.Id, employeeIds, ct);
                foreach (var line in dto.Lines)
                {
                    if (!outstandingLines.TryGetValue(line.EmployeeId, out var targets) || targets.Count == 0)
                        return Result.Failure<long>(PayrollErrors.Business(
                            "EmployeeMissingFromPayroll",
                            $"Xodim tuzatish hujjatiga kiritilmagan (xodim ID: {line.EmployeeId}, hujjat ID: {correctionDoc.Id}).",
                            _userContext.LanguageId));
                    var available = PayrollPaymentAllocator.TotalOutstanding(targets);
                    if (line.Amount > available)
                        return Result.Failure<long>(PayrollErrors.PaymentExceedsOutstanding(line.EmployeeId, line.Amount, available, _userContext.LanguageId));
                    allocationsByEmployee[line.EmployeeId] = PayrollPaymentAllocator.Allocate(line.Amount, targets);
                }
            }
            else
            {
                // Prepayment advance (against the coming salary): manual/computed amount,
                // not tied to a payroll line.
                var validAdvanceEmployeesQuery = _queryBuilder.For<PayEmployment>()
                    .Where(x =>
                        employeeIds.Contains(x.EmployeeId) &&
                        x.OrganizationId == organizationId &&
                        x.StateId == StateIdConst.ACTIVE &&
                        x.CurrencyId == dto.CurrencyId &&
                        x.StartDate <= period.EndDate &&
                        (!x.EndDate.HasValue || x.EndDate.Value >= period.StartDate))
                    .As(x => x.EmployeeId)
                    .Build();
                var validAdvanceEmployees = await _employmentQuery.GetAllAsync(validAdvanceEmployeesQuery, ct);
                var currencyMismatchEmployee = employeeIds.Except(validAdvanceEmployees).FirstOrDefault();
                if (currencyMismatchEmployee > 0)
                    return Result.Failure<long>(PayrollErrors.Business(
                        "AdvanceCurrencyMismatch",
                        $"Xodimda to‘lov valyutasiga mos amaldagi ishga qabul yozuvi mavjud emas (xodim ID: {currencyMismatchEmployee}, valyuta ID: {dto.CurrencyId}).",
                        _userContext.LanguageId));
            }

            var documentNumberResult = await _documentNumberService.GetNextAsync(
                organizationId,
                DocumentTypeIdConst.PAYROLLPAYMENT,
                dto.DocDate,
                ct);
            if (!documentNumberResult.IsSuccess)
                return Result.Failure<long>(documentNumberResult.Error);

            var now = DateTime.Now;
            var entity = new PayPaymentBatch
            {
                OrganizationId = organizationId,
                PeriodId = period.Id,
                PayrollDocId = dto.PayrollDocId,
                DocNumber = documentNumberResult.Value.DocumentNumber,
                DocDate = dto.DocDate,
                PaymentKind = dto.PaymentKind,
                SourceType = dto.SourceType,
                BankAccountId = dto.SourceType == PayrollPaymentSourceConst.Bank ? dto.BankAccountId : null,
                CashBoxId = dto.SourceType == PayrollPaymentSourceConst.Cash ? dto.CashBoxId : null,
                SourceChartAccountId = dto.SourceChartAccountId,
                OffsetAccountId = dto.OffsetAccountId,
                CurrencyId = dto.CurrencyId,
                TotalAmount = dto.Lines.Sum(x => x.Amount),
                StatusId = DocumentStatusIdConst.DRAFT,
                Note = dto.Note,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = now,
                CreatedByUserId = _userContext.Id,
                Lines = allocationsByEmployee.Count > 0
                    ? dto.Lines.SelectMany(line => allocationsByEmployee[line.EmployeeId].Select(item => new PayPaymentLine
                    {
                        OrganizationId = organizationId,
                        EmployeeId = line.EmployeeId,
                        PayrollLineId = item.PayrollLineId,
                        Amount = item.Amount,
                        Note = line.Note
                    })).ToList()
                    : dto.Lines.Select(line => new PayPaymentLine
                    {
                        OrganizationId = organizationId,
                        EmployeeId = line.EmployeeId,
                        PayrollLineId = null,
                        Amount = line.Amount,
                        Note = line.Note
                    }).ToList()
            };
            await _command.CreateAsync(entity, ct);
            _auditLogService.SetNewValues(await GetRequiredDtoInternalAsync(entity.Id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayPaymentBatch, entity.Id.ToString(), AuditLogOperationTypeConst.Create);
            return Result.Success(entity.Id);
        }, ct);

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ConfirmAsync), async () =>
        {
            var periodId = await GetPaymentPeriodIdAsync(id, ct);
            if (!periodId.HasValue)
                return Result.Failure(PayrollErrors.NotFound("PaymentBatch", id, _userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.SALARY, -periodId.Value, ct);
            var batch = await GetAggregateAsync(id, ct);
            if (batch is null)
                return Result.Failure(PayrollErrors.NotFound("PaymentBatch", id, _userContext.LanguageId));
            if (batch.StatusId == DocumentStatusIdConst.POSTED)
                return Result.Success();
            if (batch.StatusId is not (DocumentStatusIdConst.DRAFT or DocumentStatusIdConst.PENDING))
                return Result.Failure(PayrollErrors.InvalidStatus("PaymentBatch", id, batch.StatusId, "confirmed", _userContext.LanguageId));
            if (batch.Period.Status != PayrollPeriodStatusConst.Open)
                return Result.Failure(PayrollErrors.PeriodClosed(batch.PeriodId, _userContext.LanguageId));

            if (batch.PaymentKind == PayrollPaymentKindConst.Final)
            {
                if (!batch.PayrollDocId.HasValue)
                    return Result.Failure(PayrollErrors.Business("PayrollDocumentRequired", "Yakuniy to‘lov manba hujjatisiz tasdiqlanmaydi.", _userContext.LanguageId));

                // Re-validate against the current per-line outstanding (other posted final
                // payments only — this DRAFT batch's own lines are not yet posted).
                var lineIds = batch.Lines.Where(x => x.PayrollLineId.HasValue).Select(x => x.PayrollLineId!.Value).Distinct().ToList();
                var outstandingByLine = await GetOutstandingByLineAsync(lineIds, ct);
                foreach (var line in batch.Lines)
                {
                    var available = line.PayrollLineId.HasValue
                        ? outstandingByLine.GetValueOrDefault(line.PayrollLineId.Value)
                        : 0m;
                    if (line.Amount > available)
                        return Result.Failure(PayrollErrors.PaymentExceedsOutstanding(line.EmployeeId, line.Amount, available, _userContext.LanguageId));
                }
            }

            if (!batch.OffsetAccountId.HasValue)
                return Result.Failure(PayrollErrors.StoredPostingAccountMissing("offsetAccountId", batch.Id, _userContext.LanguageId));
            var offsetAccountId = batch.OffsetAccountId.Value;

            _auditLogService.SetOldValues(await GetRequiredDtoInternalAsync(id, ct));
            Result operationResult;
            if (batch.SourceType == PayrollPaymentSourceConst.Bank)
            {
                if (!batch.BankOperationId.HasValue)
                {
                    var createResult = await _bankOperationService.CreateAsync(new BankOperationCreateDto
                    {
                        BankAccountId = batch.BankAccountId!.Value,
                        DirectionId = MovementDirectionIdConst.OUT,
                        PaymentTypeId = PaymentTypeIdConst.BANK,
                        BankChartAccountId = batch.SourceChartAccountId,
                        OffsetAccountId = offsetAccountId,
                        DocDate = batch.DocDate,
                        CurrencyId = batch.CurrencyId,
                        Amount = batch.TotalAmount,
                        ExchangeRate = 1m,
                        Comment = $"{batch.PaymentKind} payroll payment {batch.DocNumber}"
                    }, ct);
                    if (!createResult.IsSuccess)
                        return Result.Failure(createResult.Error);

                    batch.BankOperationId = createResult.Value;
                    await _command.UpdateAsync(batch, ct);
                }
                operationResult = await _bankOperationService.ConfirmAsync(batch.BankOperationId.Value, ct);
            }
            else
            {
                if (!batch.CashOperationId.HasValue)
                {
                    var createResult = await _cashOperationService.CreateAsync(new CashOperationCreateDto
                    {
                        CashBoxId = batch.CashBoxId!.Value,
                        OperationTypeId = OperationTypeIdConst.OUT,
                        PaymentTypeId = PaymentTypeIdConst.CASH,
                        CashChartAccountId = batch.SourceChartAccountId,
                        OffsetAccountId = offsetAccountId,
                        DocDate = batch.DocDate,
                        CurrencyId = batch.CurrencyId,
                        Amount = batch.TotalAmount,
                        ExchangeRate = 1m,
                        Comment = $"{batch.PaymentKind} payroll payment {batch.DocNumber}",
                        StatusId = DocumentStatusIdConst.DRAFT
                    }, ct);
                    if (!createResult.IsSuccess)
                        return Result.Failure(createResult.Error);

                    batch.CashOperationId = createResult.Value;
                    await _command.UpdateAsync(batch, ct);
                }
                operationResult = await _cashOperationService.ConfirmAsync(batch.CashOperationId.Value, ct);
            }

            if (!operationResult.IsSuccess)
                return Result.Failure(operationResult.Error);

            batch.StatusId = DocumentStatusIdConst.POSTED;
            batch.PostedAt = DateTime.Now;
            batch.PostedByUserId = _userContext.Id;
            await _command.UpdateAsync(batch, ct);
            _auditLogService.SetNewValues(await GetRequiredDtoInternalAsync(id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayPaymentBatch, id.ToString(), AuditLogOperationTypeConst.Update, "Confirmed");
            return Result.Success();
        }, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            var periodId = await GetPaymentPeriodIdAsync(id, ct);
            if (!periodId.HasValue)
                return Result.Failure(PayrollErrors.NotFound("PaymentBatch", id, _userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.SALARY, -periodId.Value, ct);
            var batch = await GetAggregateAsync(id, ct);
            if (batch is null)
                return Result.Failure(PayrollErrors.NotFound("PaymentBatch", id, _userContext.LanguageId));
            if (batch.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();
            if (batch.Period.Status != PayrollPeriodStatusConst.Open)
                return Result.Failure(PayrollErrors.PeriodClosed(batch.PeriodId, _userContext.LanguageId));

            _auditLogService.SetOldValues(await GetRequiredDtoInternalAsync(id, ct));
            Result operationResult = Result.Success();
            if (batch.BankOperationId.HasValue)
                operationResult = await _bankOperationService.CancelAsync(batch.BankOperationId.Value, ct);
            else if (batch.CashOperationId.HasValue)
                operationResult = await _cashOperationService.CancelAsync(batch.CashOperationId.Value, ct);
            if (!operationResult.IsSuccess)
                return Result.Failure(operationResult.Error);

            batch.StatusId = DocumentStatusIdConst.CANCELLED;
            batch.CancelledAt = DateTime.Now;
            batch.CancelledByUserId = _userContext.Id;
            await _command.UpdateAsync(batch, ct);
            _auditLogService.SetNewValues(await GetRequiredDtoInternalAsync(id, ct));
            await _auditLogService.CreateAsync(AuditLogTableConst.PayPaymentBatch, id.ToString(), AuditLogOperationTypeConst.Update, "Cancelled");
            return Result.Success();
        }, ct);

    public Task<Result<PayrollAdvanceSuggestionDto>> GetAdvanceSuggestionAsync(long periodId, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAdvanceSuggestionAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<PayrollAdvanceSuggestionDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            var period = await _periodQuery.GetAsync(_queryBuilder.For<PayPeriod>().Where(x => x.Id == periodId).Build(), ct);
            if (period is null)
                return Result.Failure<PayrollAdvanceSuggestionDto>(PayrollErrors.NotFound("Period", periodId, _userContext.LanguageId));

            var empQuery = _queryBuilder.For<PayEmployment>()
                .Where(x =>
                    x.OrganizationId == organizationId &&
                    x.StateId == StateIdConst.ACTIVE &&
                    x.StartDate <= period.EndDate &&
                    (!x.EndDate.HasValue || x.EndDate.Value >= period.StartDate))
                .As(x => new
                {
                    x.Id,
                    x.EmployeeId,
                    EmployeeNumber = x.Employee.EmployeeNumber,
                    EmployeeName = x.Employee.LastName + " " + x.Employee.FirstName,
                    x.MonthlySalary,
                    x.EmploymentRate,
                    x.AdvanceMethod,
                    x.AdvanceValue,
                    x.StartDate
                })
                .Build();
            var employments = await _employmentQuery.GetAllAsync(empQuery, ct);
            var latest = employments
                .GroupBy(x => x.EmployeeId)
                .Select(g => g.OrderByDescending(y => y.StartDate).ThenByDescending(y => y.Id).First())
                .ToList();
            var employeeIds = latest.Select(x => x.EmployeeId).ToList();

            var correctionByEmployee = new Dictionary<long, decimal>();
            var correctionDocIds = await _payrollDocQuery.GetAllAsync(
                _queryBuilder.For<PayPayrollDoc>()
                    .Where(x =>
                        x.PeriodId == periodId &&
                        x.DocumentKind == PayrollDocumentKindConst.Correction &&
                        x.CorrectionPayoutMode == PayrollCorrectionPayoutModeConst.WithAdvance &&
                        x.StateId == StateIdConst.ACTIVE &&
                        x.StatusId == DocumentStatusIdConst.POSTED)
                    .As(x => x.Id)
                    .Build(), ct);
            if (correctionDocIds.Count > 0 && employeeIds.Count > 0)
            {
                var lines = await GetOutstandingLinesAsync(correctionDocIds, 0, employeeIds, ct);
                correctionByEmployee = lines.ToDictionary(kv => kv.Key, kv => PayrollPaymentAllocator.TotalOutstanding(kv.Value));
            }

            var resultLines = latest
                .Select(x =>
                {
                    var baseAdvance = PayrollAdvanceCalculator.Compute(x.AdvanceMethod, x.AdvanceValue, x.MonthlySalary, x.EmploymentRate);
                    var correction = correctionByEmployee.GetValueOrDefault(x.EmployeeId);
                    return new PayrollAdvanceSuggestionLineDto
                    {
                        EmployeeId = x.EmployeeId,
                        EmployeeNumber = x.EmployeeNumber,
                        EmployeeName = x.EmployeeName,
                        AdvanceMethod = x.AdvanceMethod,
                        AdvanceValue = x.AdvanceValue,
                        BaseAdvance = baseAdvance,
                        CorrectionAmount = correction,
                        Suggested = baseAdvance + correction
                    };
                })
                .Where(x => x.Suggested > 0m)
                .OrderBy(x => x.EmployeeName)
                .ToList();

            return Result.Success(new PayrollAdvanceSuggestionDto { PeriodId = periodId, Lines = resultLines });
        });

    private async Task<Result> ValidateSourceAsync(PayrollPaymentCreateDto dto, int organizationId, CancellationToken ct)
    {
        var validSource = dto.SourceType switch
        {
            PayrollPaymentSourceConst.Bank => dto.BankAccountId.HasValue && !dto.CashBoxId.HasValue,
            PayrollPaymentSourceConst.Cash => dto.CashBoxId.HasValue && !dto.BankAccountId.HasValue,
            _ => false
        };
        if (!validSource)
            return Result.Failure(PayrollErrors.PaymentSourceInvalid(_userContext.LanguageId));

        if (dto.SourceType == PayrollPaymentSourceConst.Bank &&
            !await _bankAccountQuery.AnyAsync(x =>
                x.Id == dto.BankAccountId!.Value &&
                x.OrganizationId == organizationId &&
                x.CurrencyId == dto.CurrencyId &&
                x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(PayrollErrors.ReferencedRecordNotFound("BankAccount", dto.BankAccountId!.Value, _userContext.LanguageId));

        if (dto.SourceType == PayrollPaymentSourceConst.Cash &&
            !await _cashBoxQuery.AnyAsync(x =>
                x.Id == dto.CashBoxId!.Value &&
                x.OrganizationId == organizationId &&
                x.CurrencyId == dto.CurrencyId &&
                x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(PayrollErrors.ReferencedRecordNotFound("CashBox", dto.CashBoxId!.Value, _userContext.LanguageId));

        if (!await _accountQuery.AnyAsync(x =>
                x.Id == dto.SourceChartAccountId &&
                x.OrganizationId == organizationId &&
                x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(PayrollErrors.ReferencedRecordNotFound("ChartAccount", dto.SourceChartAccountId, _userContext.LanguageId));

        if (!await _accountQuery.AnyAsync(x =>
                x.Id == dto.OffsetAccountId &&
                x.OrganizationId == organizationId &&
                x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(PayrollErrors.ReferencedRecordNotFound("ChartAccount", dto.OffsetAccountId, _userContext.LanguageId));

        if (!await _currencyQuery.AnyAsync(x =>
                x.Id == dto.CurrencyId &&
                x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(PayrollErrors.ReferencedRecordNotFound("Currency", dto.CurrencyId, _userContext.LanguageId));

        return Result.Success();
    }

    // Payout mode decides which posted documents one final payment covers:
    // a regular run is paid together with its WITH_SALARY corrections; a SEPARATE
    // (or WITH_ADVANCE) correction is paid on its own.
    private async Task<List<long>> GetPayableDocIdsAsync(long periodId, PayPayrollDoc targetDoc, CancellationToken ct)
    {
        if (targetDoc.DocumentKind != PayrollDocumentKindConst.Regular)
            return [targetDoc.Id];

        var correctionQuery = _queryBuilder.For<PayPayrollDoc>()
            .Where(x =>
                x.PeriodId == periodId &&
                x.DocumentKind == PayrollDocumentKindConst.Correction &&
                x.CorrectionOfDocId == targetDoc.Id &&
                x.CorrectionPayoutMode == PayrollCorrectionPayoutModeConst.WithSalary &&
                x.StateId == StateIdConst.ACTIVE &&
                x.StatusId == DocumentStatusIdConst.POSTED)
            .As(x => x.Id)
            .Build();
        var corrections = await _payrollDocQuery.GetAllAsync(correctionQuery, ct);
        return new List<long> { targetDoc.Id }.Concat(corrections).ToList();
    }

    // Per-employee ordered payroll lines with their remaining outstanding across the
    // covered documents (regular first, then WITH_SALARY corrections).
    private async Task<Dictionary<long, IReadOnlyList<PayrollPaymentAllocationTarget>>> GetOutstandingLinesAsync(
        List<long> docIds,
        long primaryDocId,
        List<long> employeeIds,
        CancellationToken ct)
    {
        var lineQuery = _queryBuilder.For<PayPayrollLine>()
            .Where(x => docIds.Contains(x.PayrollDocId) && employeeIds.Contains(x.EmployeeId))
            .As(x => new { x.Id, x.EmployeeId, x.PayrollDocId, x.PayableAmount })
            .Build();
        var lines = await _payrollLineQuery.GetAllAsync(lineQuery, ct);

        var lineIds = lines.Select(x => x.Id).ToList();
        var paidByLine = await GetPaidByLineAsync(lineIds, ct);

        return lines
            .GroupBy(x => x.EmployeeId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<PayrollPaymentAllocationTarget>)g
                    .OrderByDescending(l => l.PayrollDocId == primaryDocId)
                    .ThenBy(l => l.PayrollDocId)
                    .Select(l => new PayrollPaymentAllocationTarget(
                        l.Id,
                        Math.Max(0m, decimal.Round(l.PayableAmount - paidByLine.GetValueOrDefault(l.Id), 2, MidpointRounding.AwayFromZero))))
                    .ToList());
    }

    private async Task<Dictionary<long, decimal>> GetOutstandingByLineAsync(List<long> lineIds, CancellationToken ct)
    {
        if (lineIds.Count == 0)
            return new Dictionary<long, decimal>();

        var payableQuery = _queryBuilder.For<PayPayrollLine>()
            .Where(x => lineIds.Contains(x.Id))
            .As(x => new { x.Id, x.PayableAmount })
            .Build();
        var payable = (await _payrollLineQuery.GetAllAsync(payableQuery, ct))
            .ToDictionary(x => x.Id, x => x.PayableAmount);
        var paidByLine = await GetPaidByLineAsync(lineIds, ct);

        return lineIds.ToDictionary(
            id => id,
            id => Math.Max(0m, decimal.Round(payable.GetValueOrDefault(id) - paidByLine.GetValueOrDefault(id), 2, MidpointRounding.AwayFromZero)));
    }

    private async Task<Dictionary<long, decimal>> GetPaidByLineAsync(List<long> lineIds, CancellationToken ct)
    {
        if (lineIds.Count == 0)
            return new Dictionary<long, decimal>();

        // Any posted payment that targets a specific payroll line settles it — a final
        // payment, or an advance that settles a WITH_ADVANCE correction. Prepayment
        // advances carry a null PayrollLineId and are excluded here by construction.
        var paymentQuery = _queryBuilder.For<PayPaymentLine>()
            .Where(x =>
                x.PayrollLineId != null &&
                lineIds.Contains(x.PayrollLineId.Value) &&
                x.PaymentBatch.StateId == StateIdConst.ACTIVE &&
                x.PaymentBatch.StatusId == DocumentStatusIdConst.POSTED)
            .As(x => new { LineId = x.PayrollLineId!.Value, x.Amount })
            .Build();
        var rows = await _paymentLineQuery.GetAllAsync(paymentQuery, ct);
        return rows.GroupBy(x => x.LineId).ToDictionary(g => g.Key, g => g.Sum(y => y.Amount));
    }

    private async Task<PayPaymentBatch?> GetAggregateAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayPaymentBatch>().Where(x => x.Id == id).Build();
        query.AddIncludes(x => x.Include(b => b.Period));
        query.AddIncludes(x => x.Include(b => b.Lines));
        return await _query.GetAsync(query, ct);
    }

    private async Task<long?> GetPaymentPeriodIdAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayPaymentBatch>()
            .Where(x => x.Id == id)
            .As(x => (long?)x.PeriodId)
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<PayrollPaymentDto?> GetDtoInternalAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PayPaymentBatch>()
            .Where(x => x.Id == id)
            .As(x => new PayrollPaymentDto
            {
                Id = x.Id,
                OrganizationId = x.OrganizationId,
                DocNumber = x.DocNumber,
                DocDate = x.DocDate,
                PeriodId = x.PeriodId,
                PeriodName = x.Period.PeriodYear + "-" + x.Period.PeriodMonth,
                PayrollDocId = x.PayrollDocId,
                PaymentKind = x.PaymentKind,
                SourceType = x.SourceType,
                BankAccountId = x.BankAccountId,
                CashBoxId = x.CashBoxId,
                SourceChartAccountId = x.SourceChartAccountId,
                OffsetAccountId = x.OffsetAccountId,
                CurrencyId = x.CurrencyId,
                TotalAmount = x.TotalAmount,
                StatusId = x.StatusId,
                StatusName = x.Status.Name,
                BankOperationId = x.BankOperationId,
                CashOperationId = x.CashOperationId,
                Note = x.Note,
                CreatedDate = x.CreatedDate,
                PostedAt = x.PostedAt,
                CancelledAt = x.CancelledAt,
                Lines = x.Lines.OrderBy(line => line.Employee.LastName).ThenBy(line => line.Employee.FirstName)
                    .Select(line => new PayrollPaymentLineDto
                    {
                        Id = line.Id,
                        EmployeeId = line.EmployeeId,
                        EmployeeNumber = line.Employee.EmployeeNumber,
                        EmployeeName = line.Employee.LastName + " " + line.Employee.FirstName,
                        PayrollLineId = line.PayrollLineId,
                        Amount = line.Amount,
                        Note = line.Note
                    }).ToList()
            })
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<PayrollPaymentDto> GetRequiredDtoInternalAsync(long id, CancellationToken ct) =>
        await GetDtoInternalAsync(id, ct)
        ?? throw new InvalidOperationException("Payroll payment audit snapshot is unavailable.");
}
