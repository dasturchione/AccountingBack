using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.Register;
using Application.Features.Register.AccountingRegisterEntries;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.CashFiscalTransfers;

public sealed class CashFiscalTransferLifecycleService : BaseService, ICashFiscalTransferLifecycleService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAccountingPeriodValidator _periodValidator;
    private readonly IAccountingDispatcher _accountingDispatcher;
    private readonly ICashFiscalTransferMoneyService _moneyService;
    private readonly IAuditLogService _auditLogService;
    private readonly IQueryRepository<CashFiscalTransferDoc> _query;
    private readonly ICommandRepository<CashFiscalTransferDoc> _command;
    private readonly IQueryRepository<PostingBatch> _batchQuery;
    private readonly ICommandRepository<PostingBatch> _batchCommand;
    private readonly IQueryRepository<AccountingRegisterEntry> _accountingQuery;
    private readonly ICommandRepository<AccountingRegisterEntry> _accountingCommand;
    private readonly IQueryRepository<MoneyRegisterBalance> _moneyQuery;

    public CashFiscalTransferLifecycleService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IDocumentPostingLock postingLock,
        IAccountingPeriodValidator periodValidator,
        IAccountingDispatcher accountingDispatcher,
        ICashFiscalTransferMoneyService moneyService,
        IAuditLogService auditLogService,
        IQueryRepository<CashFiscalTransferDoc> query,
        ICommandRepository<CashFiscalTransferDoc> command,
        IQueryRepository<PostingBatch> batchQuery,
        ICommandRepository<PostingBatch> batchCommand,
        IQueryRepository<AccountingRegisterEntry> accountingQuery,
        ICommandRepository<AccountingRegisterEntry> accountingCommand,
        IQueryRepository<MoneyRegisterBalance> moneyQuery,
        ILogger<CashFiscalTransferLifecycleService> logger,
        IUnitOfWork unitOfWork) : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _postingLock = postingLock;
        _periodValidator = periodValidator;
        _accountingDispatcher = accountingDispatcher;
        _moneyService = moneyService;
        _auditLogService = auditLogService;
        _query = query;
        _command = command;
        _batchQuery = batchQuery;
        _batchCommand = batchCommand;
        _accountingQuery = accountingQuery;
        _accountingCommand = accountingCommand;
        _moneyQuery = moneyQuery;
    }

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ConfirmAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.CASHFISCALTRANSFER, id, ct);
            var document = await GetDocumentAsync(id, ct);
            if (document is null) return Result.Failure(CashFiscalTransferErrors.NotFound(id, _userContext.LanguageId));
            if (document.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Failure(CashFiscalTransferErrors.AlreadyCancelled(id, _userContext.LanguageId));
            if (document.StatusId == DocumentStatusIdConst.POSTED)
                return await GetActiveBatchAsync(id, ct) is not null
                    ? Result.Success()
                    : Result.Failure(CashFiscalTransferErrors.MissingPostingBatch(id, _userContext.LanguageId));
            if (document.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(CashFiscalTransferErrors.InvalidStatus(id, document.StatusId, _userContext.LanguageId));

            var period = await _periodValidator.EnsureOpenAsync(document.OrganizationId, document.DocDate, ct);
            if (!period.IsSuccess) return period;

            var validation = ValidateConfiguration(document);
            if (!validation.IsSuccess) return validation;

            var sourceType = document.DirectionId == MovementDirectionIdConst.OUT
                ? RegisterDefaultsConst.FiscalCashRegister
                : RegisterDefaultsConst.CashBoxBalance;
            var sourceId = document.DirectionId == MovementDirectionIdConst.OUT
                ? document.FiscalCashRegisterId
                : document.CashBoxId;
            await _postingLock.AcquireMoneyAsync(document.OrganizationId, sourceType, sourceId, ct);

            var available = document.DirectionId == MovementDirectionIdConst.OUT
                ? await _moneyService.GetFiscalBalanceAsync(document.FiscalCashRegisterId, document.CurrencyId, document.DocDate, ct)
                : await _moneyService.GetCashBoxBalanceAsync(document.CashBoxId, document.DocDate, ct);
            if (available < document.Amount)
                return Result.Failure(CashFiscalTransferErrors.InsufficientBalance(sourceType, available, document.Amount, _userContext.LanguageId));

            if (await GetActiveBatchAsync(id, ct) is not null || await HasEffectsAsync(id, ct))
                return Result.Failure(CashFiscalTransferErrors.BusinessEffectsAlreadyExist(id, _userContext.LanguageId));

            var batch = await CreateBatchAsync(document, PostingBatchStatusConst.POSTED, "Cash fiscal transfer confirmed", ct);
            var accounting = await _accountingDispatcher.ProcessAsync(document, ct, batch.Id);
            if (!accounting.IsSuccess) return Result.Failure(accounting.Error);
            var money = await _moneyService.PostAsync(document, batch.Id, ct);
            if (!money.IsSuccess) return Result.Failure(money.Error);

            document.StatusId = DocumentStatusIdConst.POSTED;
            document.PostedAt = DateTime.Now;
            document.PostedByUserId = _userContext.Id;
            await _command.UpdateAsync(document, ct);
            await _auditLogService.CreateAsync(AuditLogTableConst.CashFiscalTransfer, id.ToString(), AuditLogOperationTypeConst.Update, "Confirmed");
            return Result.Success();
        }, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.CASHFISCALTRANSFER, id, ct);
            var document = await GetDocumentAsync(id, ct);
            if (document is null) return Result.Failure(CashFiscalTransferErrors.NotFound(id, _userContext.LanguageId));
            if (document.StatusId == DocumentStatusIdConst.CANCELLED) return Result.Success();
            if (document.StatusId is not (DocumentStatusIdConst.DRAFT or DocumentStatusIdConst.POSTED))
                return Result.Failure(CashFiscalTransferErrors.InvalidStatus(id, document.StatusId, _userContext.LanguageId));

            if (document.StatusId == DocumentStatusIdConst.POSTED)
            {
                var originalPeriod = await _periodValidator.EnsureOpenAsync(document.OrganizationId, document.DocDate, ct);
                if (!originalPeriod.IsSuccess) return originalPeriod;
                var reversalPeriod = await _periodValidator.EnsureOpenAsync(document.OrganizationId, DateTime.Now, ct);
                if (!reversalPeriod.IsSuccess) return reversalPeriod;

                await AcquireBothMoneyLocksAsync(document, ct);
                var activeBatch = await GetActiveBatchAsync(id, ct);
                if (activeBatch is null) return Result.Failure(CashFiscalTransferErrors.MissingPostingBatch(id, _userContext.LanguageId));
                var reversalBatch = await CreateBatchAsync(document, PostingBatchStatusConst.REVERSAL, "Cash fiscal transfer cancelled", ct);
                var accountingReverse = await ReverseAccountingAsync(document.Id, reversalBatch.Id, ct);
                if (!accountingReverse.IsSuccess) return accountingReverse;
                var moneyReverse = await _moneyService.ReverseAsync(document, reversalBatch.Id, ct);
                if (!moneyReverse.IsSuccess) return Result.Failure(moneyReverse.Error);

                activeBatch.Status = PostingBatchStatusConst.REVERSED;
                activeBatch.ReversedAt = DateTime.Now;
                activeBatch.ReversedByUserId = _userContext.Id;
                await _batchCommand.UpdateAsync(activeBatch, ct);
            }

            document.StatusId = DocumentStatusIdConst.CANCELLED;
            document.CancelledAt = DateTime.Now;
            document.CancelledByUserId = _userContext.Id;
            await _command.UpdateAsync(document, ct);
            await _auditLogService.CreateAsync(AuditLogTableConst.CashFiscalTransfer, id.ToString(), AuditLogOperationTypeConst.Update, "Cancelled");
            return Result.Success();
        }, ct);

    private Result ValidateConfiguration(CashFiscalTransferDoc document)
    {
        if (!MovementDirectionIdConst.IsValid(document.DirectionId))
            return Result.Failure(CashFiscalTransferErrors.InvalidDirection(_userContext.LanguageId));
        if (document.Amount <= 0m || document.ExchangeRate <= 0m)
            return Result.Failure(CashFiscalTransferErrors.InvalidAmountOrRate(_userContext.LanguageId));
        if (document.FiscalCashRegister.OrganizationId != document.OrganizationId || document.FiscalCashRegister.StateId != StateIdConst.ACTIVE)
            return Result.Failure(CashFiscalTransferErrors.FiscalRegisterInvalid(_userContext.LanguageId));
        if (document.CashBox.OrganizationId != document.OrganizationId || document.CashBox.StateId != StateIdConst.ACTIVE || !document.CashBox.IsMain)
            return Result.Failure(CashFiscalTransferErrors.MainCashBoxInvalid(_userContext.LanguageId));
        if (document.CashBox.CurrencyId != document.CurrencyId)
            return Result.Failure(CashFiscalTransferErrors.CurrencyMismatch(_userContext.LanguageId));
        if (document.FiscalCashAccount is null || document.CashBoxAccount is null || document.FiscalCashAccountId == document.CashBoxAccountId)
            return Result.Failure(CashFiscalTransferErrors.AccountsMustDiffer(_userContext.LanguageId));
        if (document.FiscalCashAccount.OrganizationId != document.OrganizationId || document.CashBoxAccount.OrganizationId != document.OrganizationId ||
            document.FiscalCashAccount.StateId != StateIdConst.ACTIVE || document.CashBoxAccount.StateId != StateIdConst.ACTIVE)
            return Result.Failure(CashFiscalTransferErrors.AccountsInvalid(_userContext.LanguageId));
        return Result.Success();
    }

    private async Task<CashFiscalTransferDoc?> GetDocumentAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<CashFiscalTransferDoc>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId && x.StateId == StateIdConst.ACTIVE)
            .Build();
        query.AddIncludes(x => x.Include(d => d.FiscalCashRegister));
        query.AddIncludes(x => x.Include(d => d.CashBox));
        query.AddIncludes(x => x.Include(d => d.FiscalCashAccount));
        query.AddIncludes(x => x.Include(d => d.CashBoxAccount));
        return await _query.GetAsync(query, ct);
    }

    private async Task<PostingBatch?> GetActiveBatchAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PostingBatch>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.CASHFISCALTRANSFER && x.DocumentId == id && x.Status == PostingBatchStatusConst.POSTED)
            .Build();
        return await _batchQuery.GetAsync(query, ct);
    }

    private async Task<bool> HasEffectsAsync(long id, CancellationToken ct) =>
        await _accountingQuery.AnyAsync(x => x.DocumentTypeId == DocumentTypeIdConst.CASHFISCALTRANSFER && x.DocumentId == id && x.ReversalEntryId == null, ct) ||
        await _moneyQuery.AnyAsync(x => x.DocumentTypeId == DocumentTypeIdConst.CASHFISCALTRANSFER && x.DocumentId == id && x.ReversalEntryId == null, ct);

    private async Task<PostingBatch> CreateBatchAsync(CashFiscalTransferDoc document, string status, string comment, CancellationToken ct)
    {
        var batch = new PostingBatch
        {
            OrganizationId = document.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.CASHFISCALTRANSFER,
            DocumentId = document.Id,
            Status = status,
            PostedByUserId = _userContext.Id,
            PostedAt = DateTime.Now,
            Comment = comment
        };
        await _batchCommand.CreateAsync(batch, ct);
        return batch;
    }

    private async Task<Result> ReverseAccountingAsync(long id, long reversalBatchId, CancellationToken ct)
    {
        var query = _queryBuilder.For<AccountingRegisterEntry>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.CASHFISCALTRANSFER && x.DocumentId == id && x.ReversalEntryId == null)
            .Build();
        query.AddIncludes(x => x.Include(e => e.RegisterEntrySubkontos));
        var originals = await _accountingQuery.GetAllAsync(query, ct);
        if (originals.Count == 0) return Result.Failure(CashFiscalTransferErrors.MissingAccountingEntries(id, _userContext.LanguageId));

        var now = DateTime.Now;
        var reversals = originals.Select(x => new AccountingRegisterEntry
        {
            OrganizationId = x.OrganizationId,
            DocumentTypeId = x.DocumentTypeId,
            DocumentId = x.DocumentId,
            DebitAccountId = x.CreditAccountId,
            CreditAccountId = x.DebitAccountId,
            CurrencyId = x.CurrencyId,
            Amount = x.Amount,
            DocDate = now,
            CreatedDate = now,
            OperationTypeId = x.OperationTypeId,
            DebitQuantity = x.CreditQuantity,
            CreditQuantity = x.DebitQuantity,
            Content = $"Reversal: {x.Content}",
            JournalNumber = x.JournalNumber,
            PostingBatchId = reversalBatchId,
            SourceLineId = x.SourceLineId,
            ReversalEntryId = x.Id,
            RegisterEntrySubkontos = x.RegisterEntrySubkontos.Select(s => new RegisterEntrySubkonto
            {
                Side = s.Side == SubkontoSideConst.DEBIT ? SubkontoSideConst.CREDIT : SubkontoSideConst.DEBIT,
                SubkontoTypeId = s.SubkontoTypeId,
                SortOrder = s.SortOrder,
                EntityId = s.EntityId,
                DisplayValue = s.DisplayValue,
                CreatedDate = now
            }).ToList()
        }).ToList();
        await _accountingCommand.CreateAsync(reversals, ct);
        return Result.Success();
    }

    private async Task AcquireBothMoneyLocksAsync(CashFiscalTransferDoc document, CancellationToken ct)
    {
        await _postingLock.AcquireMoneyAsync(document.OrganizationId, RegisterDefaultsConst.CashBoxBalance, document.CashBoxId, ct);
        await _postingLock.AcquireMoneyAsync(document.OrganizationId, RegisterDefaultsConst.FiscalCashRegister, document.FiscalCashRegisterId, ct);
    }
}
