using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.Register;
using Application.Features.Register.AccountingRegisterEntries;
using Application.Features.BankOperations;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.CashCollections;

public sealed class CashCollectionLifecycleService : BaseService, ICashCollectionLifecycleService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAccountingPeriodValidator _periodValidator;
    private readonly IAccountingDispatcher _accountingDispatcher;
    private readonly ICashCollectionMoneyService _moneyService;
    private readonly IAuditLogService _auditLogService;
    private readonly IQueryRepository<CashCollectionDoc> _query;
    private readonly ICommandRepository<CashCollectionDoc> _command;
    private readonly IQueryRepository<PostingBatch> _batchQuery;
    private readonly ICommandRepository<PostingBatch> _batchCommand;
    private readonly IQueryRepository<AccountingRegisterEntry> _accountingQuery;
    private readonly ICommandRepository<AccountingRegisterEntry> _accountingCommand;
    private readonly IQueryRepository<MoneyRegisterBalance> _moneyQuery;
    private readonly IBankOperationRelatedDocumentService _relatedDocumentService;

    public CashCollectionLifecycleService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IDocumentPostingLock postingLock,
        IAccountingPeriodValidator periodValidator,
        IAccountingDispatcher accountingDispatcher,
        ICashCollectionMoneyService moneyService,
        IAuditLogService auditLogService,
        IQueryRepository<CashCollectionDoc> query,
        ICommandRepository<CashCollectionDoc> command,
        IQueryRepository<PostingBatch> batchQuery,
        ICommandRepository<PostingBatch> batchCommand,
        IQueryRepository<AccountingRegisterEntry> accountingQuery,
        ICommandRepository<AccountingRegisterEntry> accountingCommand,
        IQueryRepository<MoneyRegisterBalance> moneyQuery,
        IBankOperationRelatedDocumentService relatedDocumentService,
        ILogger<CashCollectionLifecycleService> logger,
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
        _relatedDocumentService = relatedDocumentService;
    }

    public Task<Result> SendToBankAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(SendToBankAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.CASHCOLLECTION, id, ct);
            var document = await GetDocumentAsync(id, ct);
            if (document is null)
                return Result.Failure(CashCollectionErrors.NotFound(id, _userContext.LanguageId));
            if (document.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Failure(CashCollectionErrors.AlreadyCancelled(id, _userContext.LanguageId));
            if (document.StatusId == DocumentStatusIdConst.IN_TRANSIT)
                return await GetActiveBatchAsync(id, ct) is not null
                    ? Result.Success()
                    : Result.Failure(CashCollectionErrors.MissingPostingBatch(id, _userContext.LanguageId));
            if (document.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(CashCollectionErrors.InvalidStatus(id, document.StatusId, _userContext.LanguageId));

            var period = await _periodValidator.EnsureOpenAsync(document.OrganizationId, document.DocDate, ct);
            if (!period.IsSuccess)
                return period;

            var validation = ValidateConfiguration(document);
            if (!validation.IsSuccess)
                return validation;

            await _postingLock.AcquireMoneyAsync(
                document.OrganizationId,
                RegisterDefaultsConst.CashBoxBalance,
                document.CashBoxId,
                ct);

            var available = await _moneyService.GetCashBoxBalanceAsync(document.CashBoxId, document.DocDate, ct);
            if (available < document.Amount)
                return Result.Failure(CashCollectionErrors.InsufficientBalance(available, document.Amount, _userContext.LanguageId));

            if (await GetActiveBatchAsync(id, ct) is not null || await HasEffectsAsync(id, ct))
                return Result.Failure(CashCollectionErrors.BusinessEffectsAlreadyExist(id, _userContext.LanguageId));

            var oldDocument = await GetDtoAsync(id, ct);
            if (oldDocument is not null)
                _auditLogService.SetOldValues(oldDocument);

            var batch = await CreateBatchAsync(document, PostingBatchStatusConst.POSTED, "Cash collection sent to bank", ct);
            var accounting = await _accountingDispatcher.ProcessAsync(document, ct, batch.Id);
            if (!accounting.IsSuccess)
                return Result.Failure(accounting.Error);

            var money = await _moneyService.PostAsync(document, batch.Id, ct);
            if (!money.IsSuccess)
                return Result.Failure(money.Error);

            document.StatusId = DocumentStatusIdConst.IN_TRANSIT;
            document.InTransitAt = DateTime.Now;
            document.InTransitByUserId = _userContext.Id;
            await _command.UpdateAsync(document, ct);

            var newDocument = await GetDtoAsync(id, ct);
            if (newDocument is not null)
            {
                _auditLogService.SetNewValues(newDocument);
                await _auditLogService.CreateAsync(
                    AuditLogTableConst.CashCollection,
                    id.ToString(),
                    AuditLogOperationTypeConst.Update,
                    "Sent to bank");
            }
            return Result.Success();
        }, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.CASHCOLLECTION, id, ct);
            var document = await GetDocumentAsync(id, ct);
            if (document is null)
                return Result.Failure(CashCollectionErrors.NotFound(id, _userContext.LanguageId));
            if (document.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();

            var hasActiveBankOperation = await _relatedDocumentService.HasActiveCashCollectionLinkAsync(
                document.Id,
                excludedBankOperationId: null,
                ct);
            var linkValidation = CashCollectionBankLinkPolicy.ValidateCancellation(document, hasActiveBankOperation, _userContext.LanguageId);
            if (!linkValidation.IsSuccess)
                return linkValidation;

            if (document.StatusId == DocumentStatusIdConst.COMPLETED)
                return Result.Failure(CashCollectionErrors.CompletedBankOperationMustBeCancelled(id, _userContext.LanguageId));
            if (document.StatusId is not (DocumentStatusIdConst.DRAFT or DocumentStatusIdConst.IN_TRANSIT))
                return Result.Failure(CashCollectionErrors.InvalidStatus(id, document.StatusId, _userContext.LanguageId));

            var cancelledFromStatusId = document.StatusId;
            if (document.StatusId == DocumentStatusIdConst.IN_TRANSIT)
            {
                var originalPeriod = await _periodValidator.EnsureOpenAsync(document.OrganizationId, document.DocDate, ct);
                if (!originalPeriod.IsSuccess)
                    return originalPeriod;
                var reversalPeriod = await _periodValidator.EnsureOpenAsync(document.OrganizationId, DateTime.Now, ct);
                if (!reversalPeriod.IsSuccess)
                    return reversalPeriod;

                await _postingLock.AcquireMoneyAsync(
                    document.OrganizationId,
                    RegisterDefaultsConst.CashBoxBalance,
                    document.CashBoxId,
                    ct);

                var activeBatch = await GetActiveBatchAsync(id, ct);
                if (activeBatch is null)
                    return Result.Failure(CashCollectionErrors.MissingPostingBatch(id, _userContext.LanguageId));

                var reversalBatch = await CreateBatchAsync(document, PostingBatchStatusConst.REVERSAL, "Cash collection cancelled", ct);
                var accountingReverse = await ReverseAccountingAsync(id, reversalBatch.Id, ct);
                if (!accountingReverse.IsSuccess)
                    return accountingReverse;
                var moneyReverse = await _moneyService.ReverseAsync(document, reversalBatch.Id, ct);
                if (!moneyReverse.IsSuccess)
                    return Result.Failure(moneyReverse.Error);

                activeBatch.Status = PostingBatchStatusConst.REVERSED;
                activeBatch.ReversedAt = DateTime.Now;
                activeBatch.ReversedByUserId = _userContext.Id;
                await _batchCommand.UpdateAsync(activeBatch, ct);
            }

            document.StatusId = DocumentStatusIdConst.CANCELLED;
            document.CancelledFromStatusId = cancelledFromStatusId;
            document.CancelledAt = DateTime.Now;
            document.CancelledByUserId = _userContext.Id;
            await _command.UpdateAsync(document, ct);
            await _auditLogService.CreateAsync(
                AuditLogTableConst.CashCollection,
                id.ToString(),
                AuditLogOperationTypeConst.Update,
                "Cancelled");
            return Result.Success();
        }, ct);

    private Result ValidateConfiguration(CashCollectionDoc document)
    {
        if (document.Amount <= 0m || document.ExchangeRate <= 0m)
            return Result.Failure(CashCollectionErrors.InvalidAmountOrRate(_userContext.LanguageId));
        if (document.CashBox.OrganizationId != document.OrganizationId || document.CashBox.StateId != StateIdConst.ACTIVE)
            return Result.Failure(CashCollectionErrors.CashBoxInvalid(_userContext.LanguageId));
        if (document.BankAccount.OrganizationId != document.OrganizationId || document.BankAccount.StateId != StateIdConst.ACTIVE)
            return Result.Failure(CashCollectionErrors.BankAccountInvalid(_userContext.LanguageId));
        if (document.CashBox.CurrencyId != document.CurrencyId || document.BankAccount.CurrencyId != document.CurrencyId)
            return Result.Failure(CashCollectionErrors.DocumentCurrenciesMismatch(_userContext.LanguageId));
        if (document.CashChartAccount is null || document.CashInTransitAccount is null || document.BankChartAccount is null ||
            document.CashChartAccountId == document.CashInTransitAccountId ||
            document.CashChartAccountId == document.BankChartAccountId ||
            document.CashInTransitAccountId == document.BankChartAccountId)
            return Result.Failure(CashCollectionErrors.AccountsMustDiffer(_userContext.LanguageId));
        if (document.CashChartAccount.OrganizationId != document.OrganizationId ||
            document.CashInTransitAccount.OrganizationId != document.OrganizationId ||
            document.BankChartAccount.OrganizationId != document.OrganizationId ||
            document.CashChartAccount.StateId != StateIdConst.ACTIVE ||
            document.CashInTransitAccount.StateId != StateIdConst.ACTIVE ||
            document.BankChartAccount.StateId != StateIdConst.ACTIVE)
            return Result.Failure(CashCollectionErrors.AccountsInvalid(_userContext.LanguageId));

        return Result.Success();
    }

    private async Task<CashCollectionDoc?> GetDocumentAsync(long id, CancellationToken ct)
    {
        var organizationId = _userContext.OrganizationId!.Value;
        var query = _queryBuilder.For<CashCollectionDoc>()
            .Where(x => x.Id == id && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
            .Build();
        query.AddIncludes(x => x.Include(document => document.CashBox));
        query.AddIncludes(x => x.Include(document => document.BankAccount));
        query.AddIncludes(x => x.Include(document => document.CashChartAccount));
        query.AddIncludes(x => x.Include(document => document.CashInTransitAccount));
        query.AddIncludes(x => x.Include(document => document.BankChartAccount));
        return await _query.GetAsync(query, ct);
    }

    private async Task<CashCollectionDto?> GetDtoAsync(long id, CancellationToken ct)
    {
        var organizationId = _userContext.OrganizationId!.Value;
        var query = _queryBuilder.For<CashCollectionDoc>()
            .Where(x => x.Id == id && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
            .As<CashCollectionDto>()
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<PostingBatch?> GetActiveBatchAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PostingBatch>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.CASHCOLLECTION &&
                        x.DocumentId == id &&
                        x.Status == PostingBatchStatusConst.POSTED)
            .Build();
        return await _batchQuery.GetAsync(query, ct);
    }

    private async Task<bool> HasEffectsAsync(long id, CancellationToken ct) =>
        await _accountingQuery.AnyAsync(x => x.DocumentTypeId == DocumentTypeIdConst.CASHCOLLECTION && x.DocumentId == id && x.ReversalEntryId == null, ct) ||
        await _moneyQuery.AnyAsync(x => x.DocumentTypeId == DocumentTypeIdConst.CASHCOLLECTION && x.DocumentId == id && x.ReversalEntryId == null, ct);

    private async Task<PostingBatch> CreateBatchAsync(CashCollectionDoc document, string status, string comment, CancellationToken ct)
    {
        var batch = new PostingBatch
        {
            OrganizationId = document.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.CASHCOLLECTION,
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
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.CASHCOLLECTION &&
                        x.DocumentId == id &&
                        x.ReversalEntryId == null)
            .Build();
        query.AddIncludes(x => x.Include(entry => entry.RegisterEntrySubkontos));
        var originals = await _accountingQuery.GetAllAsync(query, ct);
        if (originals.Count == 0)
            return Result.Failure(CashCollectionErrors.MissingAccountingEntries(id, _userContext.LanguageId));

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
            RegisterEntrySubkontos = x.RegisterEntrySubkontos.Select(subkonto => new RegisterEntrySubkonto
            {
                Side = subkonto.Side == SubkontoSideConst.DEBIT ? SubkontoSideConst.CREDIT : SubkontoSideConst.DEBIT,
                SubkontoTypeId = subkonto.SubkontoTypeId,
                SortOrder = subkonto.SortOrder,
                EntityId = subkonto.EntityId,
                DisplayValue = subkonto.DisplayValue,
                CreatedDate = now
            }).ToList()
        }).ToList();

        await _accountingCommand.CreateAsync(reversals, ct);
        return Result.Success();
    }
}
