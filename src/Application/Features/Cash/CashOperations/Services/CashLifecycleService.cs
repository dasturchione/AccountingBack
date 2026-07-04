using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.CounterpartyRegisterBalances;
using Application.Features.Register.AccountingRegisterEntries;
using Application.Features.MoneyRegisterBalances;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.CashOperations;

public class CashLifecycleService : BaseService, ICashLifecycleService
{
    private static readonly HashSet<string> SupportedPostingAliases =
    [
        AliasConst.Supplier,
        AliasConst.SupplierAdvance,
        AliasConst.Customer,
        AliasConst.CustomerAdvance,
        AliasConst.CashInTransit
    ];

    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAccountingPeriodValidator _periodValidator;
    private readonly IAuditLogService _auditLogService;
    private readonly IAccountingDispatcher _dispatcher;
    private readonly ICashMoneyRegisterService _moneyRegisterService;
    private readonly ICashCounterpartyRegisterService _counterpartyRegisterService;
    private readonly IQueryRepository<CashOperation> _query;
    private readonly ICommandRepository<CashOperation> _command;
    private readonly IQueryRepository<PostingBatch> _postingBatchQuery;
    private readonly ICommandRepository<PostingBatch> _postingBatchCommand;
    private readonly IQueryRepository<AccountingRegisterEntry> _accountingRegisterQuery;
    private readonly ICommandRepository<AccountingRegisterEntry> _accountingRegisterCommand;
    private readonly IQueryRepository<MoneyRegisterBalance> _moneyRegisterQuery;
    private readonly IQueryRepository<CounterpartyRegisterBalance> _counterpartyRegisterQuery;

    public CashLifecycleService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IDocumentPostingLock postingLock,
        IAccountingPeriodValidator periodValidator,
        IAuditLogService auditLogService,
        IAccountingDispatcher dispatcher,
        ICashMoneyRegisterService moneyRegisterService,
        ICashCounterpartyRegisterService counterpartyRegisterService,
        IQueryRepository<CashOperation> query,
        ICommandRepository<CashOperation> command,
        IQueryRepository<PostingBatch> postingBatchQuery,
        ICommandRepository<PostingBatch> postingBatchCommand,
        IQueryRepository<AccountingRegisterEntry> accountingRegisterQuery,
        ICommandRepository<AccountingRegisterEntry> accountingRegisterCommand,
        IQueryRepository<MoneyRegisterBalance> moneyRegisterQuery,
        IQueryRepository<CounterpartyRegisterBalance> counterpartyRegisterQuery,
        ILogger<CashLifecycleService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _postingLock = postingLock;
        _periodValidator = periodValidator;
        _auditLogService = auditLogService;
        _dispatcher = dispatcher;
        _moneyRegisterService = moneyRegisterService;
        _counterpartyRegisterService = counterpartyRegisterService;
        _query = query;
        _command = command;
        _postingBatchQuery = postingBatchQuery;
        _postingBatchCommand = postingBatchCommand;
        _accountingRegisterQuery = accountingRegisterQuery;
        _accountingRegisterCommand = accountingRegisterCommand;
        _moneyRegisterQuery = moneyRegisterQuery;
        _counterpartyRegisterQuery = counterpartyRegisterQuery;
    }

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ConfirmAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.CASHOPERATION, id, ct);

            var cashOperation = await GetCashOperationForLifecycleAsync(id, ct);
            if (cashOperation == null)
                return Result.Failure(CashOperationErrors.NotFound(id, _userContext.LanguageId));

            if (cashOperation.OrganizationId != _userContext.OrganizationId.Value)
                return Result.Failure(CashOperationErrors.OrganizationMismatch(id, _userContext.LanguageId));

            if (cashOperation.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Failure(CashOperationErrors.AlreadyCancelled(id, _userContext.LanguageId));

            if (cashOperation.StatusId == DocumentStatusIdConst.POSTED)
            {
                return await GetActivePostingBatchAsync(id, ct) is not null
                    ? Result.Success()
                    : Result.Failure(CashOperationErrors.MissingPostingBatch(id, _userContext.LanguageId));
            }

            if (cashOperation.StatusId != DocumentStatusIdConst.DRAFT && cashOperation.StatusId != DocumentStatusIdConst.PENDING)
                return Result.Failure(CashOperationErrors.CannotConfirmInCurrentStatus(id, cashOperation.StatusId, _userContext.LanguageId));

            var periodValidation = await _periodValidator.EnsureOpenAsync(cashOperation.OrganizationId, cashOperation.DocDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var validation = await ValidateForConfirmAsync(cashOperation, ct);
            if (!validation.IsSuccess)
                return validation;

            if (await GetActivePostingBatchAsync(id, ct) is not null || await HasBusinessEffectsAsync(id, ct))
                return Result.Failure(CashOperationErrors.BusinessEffectsAlreadyExist(id, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            var postingBatch = await CreatePostingBatchAsync(cashOperation, PostingBatchStatusConst.POSTED, "Cash operation confirmed", ct);

            var accountingDispatch = await _dispatcher.ProcessAsync(cashOperation, ct, postingBatch.Id);
            if (!accountingDispatch.IsSuccess)
                return Result.Failure(accountingDispatch.Error);

            var moneyDispatch = await _moneyRegisterService.PostAsync(cashOperation, postingBatch.Id, ct);
            if (!moneyDispatch.IsSuccess)
                return Result.Failure(moneyDispatch.Error);

            var counterpartyDispatch = await _counterpartyRegisterService.PostAsync(cashOperation, postingBatch.Id, ct);
            if (!counterpartyDispatch.IsSuccess)
                return Result.Failure(counterpartyDispatch.Error);

            cashOperation.StatusId = DocumentStatusIdConst.POSTED;
            cashOperation.PostedAt ??= DateTime.Now;
            cashOperation.PostedByUserId = _userContext.Id;
            await _command.UpdateAsync(cashOperation, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.CashOperation, id.ToString(), AuditLogOperationTypeConst.Update, "Confirmed");
            }

            return Result.Success();
        }, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.CASHOPERATION, id, ct);

            var cashOperation = await GetCashOperationForLifecycleAsync(id, ct);
            if (cashOperation == null)
                return Result.Failure(CashOperationErrors.NotFound(id, _userContext.LanguageId));

            if (cashOperation.OrganizationId != _userContext.OrganizationId.Value)
                return Result.Failure(CashOperationErrors.OrganizationMismatch(id, _userContext.LanguageId));

            if (cashOperation.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();

            if (cashOperation.StatusId != DocumentStatusIdConst.DRAFT &&
                cashOperation.StatusId != DocumentStatusIdConst.PENDING &&
                cashOperation.StatusId != DocumentStatusIdConst.POSTED)
                return Result.Failure(CashOperationErrors.CannotCancelInCurrentStatus(id, cashOperation.StatusId, _userContext.LanguageId));

            var periodValidation = await _periodValidator.EnsureOpenAsync(cashOperation.OrganizationId, cashOperation.DocDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var reversalPeriodValidation = await _periodValidator.EnsureOpenAsync(cashOperation.OrganizationId, DateTime.Now, ct);
            if (!reversalPeriodValidation.IsSuccess)
                return reversalPeriodValidation;

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            if (cashOperation.StatusId == DocumentStatusIdConst.POSTED)
            {
                var activePostingBatch = await GetActivePostingBatchAsync(id, ct);
                if (activePostingBatch == null)
                    return Result.Failure(CashOperationErrors.MissingPostingBatch(id, _userContext.LanguageId));

                var reversalBatch = await CreatePostingBatchAsync(cashOperation, PostingBatchStatusConst.REVERSAL, "Cash operation cancelled", ct);

                var accountingReverse = await ReverseAccountingEntriesAsync(id, reversalBatch.Id, ct);
                if (!accountingReverse.IsSuccess)
                    return accountingReverse;

                var moneyReverse = await _moneyRegisterService.ReverseAsync(cashOperation, reversalBatch.Id, ct);
                if (!moneyReverse.IsSuccess)
                    return moneyReverse;

                var counterpartyReverse = await _counterpartyRegisterService.ReverseAsync(cashOperation, reversalBatch.Id, ct);
                if (!counterpartyReverse.IsSuccess)
                    return counterpartyReverse;

                if (cashOperation.CounterpartyId is not null && counterpartyReverse.Value.Count == 0)
                    return Result.Failure(CashOperationErrors.MissingCounterpartyRegisterEntries(id, _userContext.LanguageId));

                if (moneyReverse.Value.Count == 0)
                    return Result.Failure(CashOperationErrors.MissingMoneyRegisterEntries(id, _userContext.LanguageId));

                activePostingBatch.Status = PostingBatchStatusConst.REVERSED;
                activePostingBatch.ReversedAt = DateTime.Now;
                activePostingBatch.ReversedByUserId = _userContext.Id;
                await _postingBatchCommand.UpdateAsync(activePostingBatch, ct);
            }

            cashOperation.StatusId = DocumentStatusIdConst.CANCELLED;
            cashOperation.CancelledAt ??= DateTime.Now;
            cashOperation.CancelledByUserId = _userContext.Id;
            await _command.UpdateAsync(cashOperation, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.CashOperation, id.ToString(), AuditLogOperationTypeConst.Update, "Cancelled");
            }

            return Result.Success();
        }, ct);

    private async Task<CashOperationDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<CashOperation>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
            .As<CashOperationDto>()
            .Build();

        return await _query.GetAsync(query, ct);
    }

    private async Task<CashOperation?> GetCashOperationForLifecycleAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<CashOperation>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
            .Build();
        query.AddIncludes(x => x.Include(d => d.CashBox));
        query.AddIncludes(x => x.Include(d => d.DestinationCashBox));
        query.AddIncludes(x => x.Include(d => d.Counterparty));
        query.AddIncludes(x => x.Include(d => d.PaymentPurpose)
            .ThenInclude(purpose => purpose.Alias));
        return await _query.GetAsync(query, ct);
    }

    private async Task<Result> ValidateForConfirmAsync(CashOperation cashOperation, CancellationToken ct)
    {
        if (cashOperation.Amount <= 0m)
            return Result.Failure(CashOperationErrors.InvalidAmount(cashOperation.Id, _userContext.LanguageId));

        if (cashOperation.OperationTypeId is not (OperationTypeIdConst.IN or OperationTypeIdConst.OUT or OperationTypeIdConst.TRANSFER))
            return Result.Failure(CashOperationErrors.InvalidOperationType(cashOperation.OperationTypeId, _userContext.LanguageId));

        if (cashOperation.PaymentPurposeId <= 0 || cashOperation.PaymentPurpose == null)
            return Result.Failure(CashOperationErrors.InvalidPaymentPurpose(cashOperation.Id, _userContext.LanguageId));

        if (cashOperation.CashBox.OrganizationId != cashOperation.OrganizationId ||
            cashOperation.CashBox.StateId != StateIdConst.ACTIVE)
            return Result.Failure(CashOperationErrors.InvalidCashBoxMismatch(cashOperation.Id, _userContext.LanguageId));

        if (cashOperation.OperationTypeId == OperationTypeIdConst.TRANSFER)
        {
            if (cashOperation.DestinationCashBoxId is null || cashOperation.DestinationCashBoxId == cashOperation.CashBoxId)
                return Result.Failure(CashOperationErrors.InvalidCashBoxMismatch(cashOperation.Id, _userContext.LanguageId));

            if (cashOperation.DestinationCashBox is null ||
                cashOperation.DestinationCashBox.OrganizationId != cashOperation.OrganizationId ||
                cashOperation.DestinationCashBox.StateId != StateIdConst.ACTIVE)
                return Result.Failure(CashOperationErrors.InvalidCashBoxMismatch(cashOperation.Id, _userContext.LanguageId));
        }

        if (cashOperation.CounterpartyId.HasValue &&
            (cashOperation.Counterparty == null ||
             cashOperation.Counterparty.OrganizationId != cashOperation.OrganizationId ||
             cashOperation.Counterparty.StateId != StateIdConst.ACTIVE))
        {
            return Result.Failure(CashOperationErrors.OrganizationMismatch(cashOperation.Id, _userContext.LanguageId));
        }

        if (cashOperation.PaymentPurpose.OperationTypeId != cashOperation.OperationTypeId)
            return Result.Failure(CashOperationErrors.InvalidPaymentPurpose(cashOperation.Id, _userContext.LanguageId));

        if (cashOperation.PaymentPurpose.RequiresCounterparty && !cashOperation.CounterpartyId.HasValue)
            return Result.Failure(Error.Business("CashOperation.CounterpartyRequired", "Selected payment purpose requires a counterparty."));

        if (cashOperation.OperationTypeId != OperationTypeIdConst.TRANSFER &&
            !SupportedPostingAliases.Contains(cashOperation.PaymentPurpose.Alias.Code))
        {
            return Result.Failure(Error.Business("CashOperation.UnsupportedPaymentPurpose", $"Payment purpose alias '{cashOperation.PaymentPurpose.Alias.Code}' is not supported by current posting configuration."));
        }

        if (cashOperation.OperationTypeId == OperationTypeIdConst.OUT || cashOperation.OperationTypeId == OperationTypeIdConst.TRANSFER)
        {
            var sourceBalance = await _moneyRegisterService.GetCashBoxBalanceAsync(cashOperation.CashBoxId, cashOperation.DocDate, ct);
            if (sourceBalance < cashOperation.Amount)
                return Result.Failure(CashOperationErrors.InsufficientBalance(cashOperation.CashBoxId, cashOperation.Amount, _userContext.LanguageId));
        }

        return Result.Success();
    }

    private async Task<PostingBatch?> GetActivePostingBatchAsync(long cashOperationId, CancellationToken ct)
    {
        var query = _queryBuilder.For<PostingBatch>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.CASHOPERATION &&
                        x.DocumentId == cashOperationId &&
                        x.Status == PostingBatchStatusConst.POSTED)
            .Build();

        return await _postingBatchQuery.GetAsync(query, ct);
    }

    private async Task<bool> HasBusinessEffectsAsync(long cashOperationId, CancellationToken ct)
    {
        if (await _accountingRegisterQuery.AnyAsync(x =>
                x.DocumentTypeId == DocumentTypeIdConst.CASHOPERATION &&
                x.DocumentId == cashOperationId &&
                x.ReversalEntryId == null, ct))
            return true;

        if (await _moneyRegisterQuery.AnyAsync(x =>
                x.DocumentTypeId == DocumentTypeIdConst.CASHOPERATION &&
                x.DocumentId == cashOperationId &&
                x.ReversalEntryId == null, ct))
            return true;

        return await _counterpartyRegisterQuery.AnyAsync(x =>
            x.DocumentTypeId == DocumentTypeIdConst.CASHOPERATION &&
            x.DocumentId == cashOperationId &&
            x.ReversalEntryId == null, ct);
    }

    private async Task<PostingBatch> CreatePostingBatchAsync(CashOperation cashOperation, string status, string comment, CancellationToken ct)
    {
        var now = DateTime.Now;
        var batch = new PostingBatch
        {
            OrganizationId = cashOperation.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.CASHOPERATION,
            DocumentId = cashOperation.Id,
            Status = status,
            PostedByUserId = _userContext.Id,
            PostedAt = now,
            Comment = comment
        };

        await _postingBatchCommand.CreateAsync(batch, ct);
        return batch;
    }

    private async Task<Result> ReverseAccountingEntriesAsync(long cashOperationId, long reversalBatchId, CancellationToken ct)
    {
        var query = _queryBuilder.For<AccountingRegisterEntry>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.CASHOPERATION &&
                        x.DocumentId == cashOperationId &&
                        x.ReversalEntryId == null)
            .Build();
        query.AddIncludes(b => b.Include(x => x.RegisterEntrySubkontos));

        var entries = await _accountingRegisterQuery.GetAllAsync(query, ct);
        if (entries.Count == 0)
            return Result.Failure(CashOperationErrors.MissingAccountingRegisterEntries(cashOperationId, _userContext.LanguageId));

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
                Side = ReverseSubkontoSide(subkonto.Side),
                SubkontoTypeId = subkonto.SubkontoTypeId,
                SortOrder = subkonto.SortOrder,
                EntityId = subkonto.EntityId,
                DisplayValue = subkonto.DisplayValue,
                CreatedDate = now
            }).ToList()
        }).ToList();

        await _accountingRegisterCommand.CreateAsync(reversals, ct);
        return Result.Success();
    }

    private static string ReverseSubkontoSide(string side) =>
        side == SubkontoSideConst.DEBIT ? SubkontoSideConst.CREDIT :
        side == SubkontoSideConst.CREDIT ? SubkontoSideConst.DEBIT :
        side;
}
