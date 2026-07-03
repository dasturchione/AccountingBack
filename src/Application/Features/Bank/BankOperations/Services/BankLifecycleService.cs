using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.CounterpartyRegisterBalances;
using Application.Features.MoneyRegisterBalances;
using Application.Features.Register.AccountingRegisterEntries;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.BankOperations;

public class BankLifecycleService : BaseService, IBankLifecycleService
{
    private static readonly HashSet<string> SupportedPostingAliases =
    [
        AliasConst.Supplier,
        AliasConst.SupplierAdvance,
        AliasConst.Customer,
        AliasConst.CustomerAdvance
    ];

    private readonly IUserContext _userContext;
    private readonly IPermissionChecker _permissionChecker;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAccountingPeriodValidator _periodValidator;
    private readonly IAuditLogService _auditLogService;
    private readonly IAccountingDispatcher _dispatcher;
    private readonly IBankMoneyRegisterService _moneyRegisterService;
    private readonly IBankCounterpartyRegisterService _counterpartyRegisterService;
    private readonly IQueryRepository<BankOperation> _query;
    private readonly ICommandRepository<BankOperation> _command;
    private readonly IQueryRepository<PostingBatch> _postingBatchQuery;
    private readonly ICommandRepository<PostingBatch> _postingBatchCommand;
    private readonly IQueryRepository<AccountingRegisterEntry> _accountingRegisterQuery;
    private readonly ICommandRepository<AccountingRegisterEntry> _accountingRegisterCommand;
    private readonly IQueryRepository<MoneyRegisterBalance> _moneyRegisterQuery;
    private readonly IQueryRepository<CounterpartyRegisterBalance> _counterpartyRegisterQuery;

    public BankLifecycleService(
        IUserContext userContext,
        IPermissionChecker permissionChecker,
        IQueryBuilder queryBuilder,
        IDocumentPostingLock postingLock,
        IAccountingPeriodValidator periodValidator,
        IAuditLogService auditLogService,
        IAccountingDispatcher dispatcher,
        IBankMoneyRegisterService moneyRegisterService,
        IBankCounterpartyRegisterService counterpartyRegisterService,
        IQueryRepository<BankOperation> query,
        ICommandRepository<BankOperation> command,
        IQueryRepository<PostingBatch> postingBatchQuery,
        ICommandRepository<PostingBatch> postingBatchCommand,
        IQueryRepository<AccountingRegisterEntry> accountingRegisterQuery,
        ICommandRepository<AccountingRegisterEntry> accountingRegisterCommand,
        IQueryRepository<MoneyRegisterBalance> moneyRegisterQuery,
        IQueryRepository<CounterpartyRegisterBalance> counterpartyRegisterQuery,
        ILogger<BankLifecycleService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _permissionChecker = permissionChecker;
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

            var permission = await EnsurePermissionAsync(PermissionCodeConst.ConfirmBankOperation, ct);
            if (!permission.IsSuccess)
                return permission;

            await _postingLock.AcquireAsync(DocumentTypeIdConst.BANKOPERATION, id, ct);

            var bankOperation = await GetBankOperationForLifecycleAsync(id, ct);
            if (bankOperation == null)
                return Result.Failure(BankOperationErrors.NotFound(id, _userContext.LanguageId));

            if (bankOperation.OrganizationId != _userContext.OrganizationId.Value)
                return Result.Failure(BankOperationErrors.OrganizationMismatch(id, _userContext.LanguageId));

            if (bankOperation.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Failure(BankOperationErrors.AlreadyCancelled(id, _userContext.LanguageId));

            if (bankOperation.StatusId == DocumentStatusIdConst.POSTED)
            {
                return await GetActivePostingBatchAsync(id, ct) is not null
                    ? Result.Success()
                    : Result.Failure(BankOperationErrors.MissingPostingBatch(id, _userContext.LanguageId));
            }

            if (bankOperation.StatusId != DocumentStatusIdConst.DRAFT && bankOperation.StatusId != DocumentStatusIdConst.PENDING)
                return Result.Failure(BankOperationErrors.CannotConfirmInCurrentStatus(id, bankOperation.StatusId, _userContext.LanguageId));

            var periodValidation = await _periodValidator.EnsureOpenAsync(bankOperation.OrganizationId, bankOperation.DocDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var validation = await ValidateForConfirmAsync(bankOperation, ct);
            if (!validation.IsSuccess)
                return validation;

            if (await GetActivePostingBatchAsync(id, ct) is not null || await HasBusinessEffectsAsync(id, ct))
                return Result.Failure(BankOperationErrors.BusinessEffectsAlreadyExist(id, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            var postingBatch = await CreatePostingBatchAsync(bankOperation, PostingBatchStatusConst.POSTED, "Bank operation confirmed", ct);

            var accountingDispatch = await _dispatcher.ProcessAsync(bankOperation, ct, postingBatch.Id);
            if (!accountingDispatch.IsSuccess)
                return Result.Failure(accountingDispatch.Error);

            var moneyDispatch = await _moneyRegisterService.PostAsync(bankOperation, postingBatch.Id, ct);
            if (!moneyDispatch.IsSuccess)
                return Result.Failure(moneyDispatch.Error);

            var counterpartyDispatch = await _counterpartyRegisterService.PostAsync(bankOperation, postingBatch.Id, ct);
            if (!counterpartyDispatch.IsSuccess)
                return Result.Failure(counterpartyDispatch.Error);

            bankOperation.StatusId = DocumentStatusIdConst.POSTED;
            bankOperation.PostedAt ??= DateTime.Now;
            bankOperation.PostedByUserId = _userContext.Id;
            await _command.UpdateAsync(bankOperation, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.BankOperation, id.ToString(), AuditLogOperationTypeConst.Update, "Confirmed");
            }

            return Result.Success();
        }, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var permission = await EnsurePermissionAsync(PermissionCodeConst.CancelBankOperation, ct);
            if (!permission.IsSuccess)
                return permission;

            await _postingLock.AcquireAsync(DocumentTypeIdConst.BANKOPERATION, id, ct);

            var bankOperation = await GetBankOperationForLifecycleAsync(id, ct);
            if (bankOperation == null)
                return Result.Failure(BankOperationErrors.NotFound(id, _userContext.LanguageId));

            if (bankOperation.OrganizationId != _userContext.OrganizationId.Value)
                return Result.Failure(BankOperationErrors.OrganizationMismatch(id, _userContext.LanguageId));

            if (bankOperation.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();

            if (bankOperation.StatusId != DocumentStatusIdConst.DRAFT &&
                bankOperation.StatusId != DocumentStatusIdConst.PENDING &&
                bankOperation.StatusId != DocumentStatusIdConst.POSTED)
                return Result.Failure(BankOperationErrors.CannotCancelInCurrentStatus(id, bankOperation.StatusId, _userContext.LanguageId));

            var periodValidation = await _periodValidator.EnsureOpenAsync(bankOperation.OrganizationId, bankOperation.DocDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var reversalPeriodValidation = await _periodValidator.EnsureOpenAsync(bankOperation.OrganizationId, DateTime.Now, ct);
            if (!reversalPeriodValidation.IsSuccess)
                return reversalPeriodValidation;

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            if (bankOperation.StatusId == DocumentStatusIdConst.POSTED)
            {
                var activePostingBatch = await GetActivePostingBatchAsync(id, ct);
                if (activePostingBatch == null)
                    return Result.Failure(BankOperationErrors.MissingPostingBatch(id, _userContext.LanguageId));

                var reversalBatch = await CreatePostingBatchAsync(bankOperation, PostingBatchStatusConst.REVERSAL, "Bank operation cancelled", ct);

                var accountingReverse = await ReverseAccountingEntriesAsync(id, reversalBatch.Id, ct);
                if (!accountingReverse.IsSuccess)
                    return accountingReverse;

                var moneyReverse = await _moneyRegisterService.ReverseAsync(bankOperation, reversalBatch.Id, ct);
                if (!moneyReverse.IsSuccess)
                    return moneyReverse;

                if (moneyReverse.Value.Count == 0)
                    return Result.Failure(BankOperationErrors.MissingMoneyRegisterEntries(id, _userContext.LanguageId));

                var counterpartyReverse = await _counterpartyRegisterService.ReverseAsync(bankOperation, reversalBatch.Id, ct);
                if (!counterpartyReverse.IsSuccess)
                    return counterpartyReverse;

                if (bankOperation.CounterpartyId is not null && counterpartyReverse.Value.Count == 0)
                    return Result.Failure(BankOperationErrors.MissingCounterpartyRegisterEntries(id, _userContext.LanguageId));

                activePostingBatch.Status = PostingBatchStatusConst.REVERSED;
                activePostingBatch.ReversedAt = DateTime.Now;
                activePostingBatch.ReversedByUserId = _userContext.Id;
                await _postingBatchCommand.UpdateAsync(activePostingBatch, ct);
            }

            bankOperation.StatusId = DocumentStatusIdConst.CANCELLED;
            bankOperation.CancelledAt ??= DateTime.Now;
            bankOperation.CancelledByUserId = _userContext.Id;
            await _command.UpdateAsync(bankOperation, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.BankOperation, id.ToString(), AuditLogOperationTypeConst.Update, "Cancelled");
            }

            return Result.Success();
        }, ct);

    private async Task<Result> EnsurePermissionAsync(string permissionCode, CancellationToken ct)
    {
        if (_userContext.HasGlobalAccess)
            return Result.Success();

        if (_userContext.RoleId is null)
            return Result.Failure(CommonErrors.Forbidden(_userContext.LanguageId));

        return await _permissionChecker.HasPermissionAsync(_userContext.RoleId.Value, permissionCode, ct)
            ? Result.Success()
            : Result.Failure(CommonErrors.Forbidden(_userContext.LanguageId));
    }

    private async Task<BankOperationDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<BankOperation>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
            .As<BankOperationDto>()
            .Build();

        return await _query.GetAsync(query, ct);
    }

    private async Task<BankOperation?> GetBankOperationForLifecycleAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<BankOperation>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
            .Build();

        query.AddIncludes(x => 
        {
            x.Include(d => d.Contract);
            x.Include(d => d.BankAccount);
            x.Include(d => d.Counterparty);
            x.Include(d => d.CounterpartyBankAccount);
            x.Include(d => d.BankOperationLines)
                .ThenInclude(line => line.PaymentPurpose)
                .ThenInclude(purpose => purpose.Alias);
        });

        return await _query.GetAsync(query, ct);
    }

    private async Task<Result> ValidateForConfirmAsync(BankOperation bankOperation, CancellationToken ct)
    {
        if (bankOperation.Amount <= 0m)
            return Result.Failure(BankOperationErrors.InvalidAmount(bankOperation.Id, _userContext.LanguageId));

        if (bankOperation.OperationTypeId is not (OperationTypeIdConst.IN or OperationTypeIdConst.OUT))
            return Result.Failure(BankOperationErrors.InvalidOperationType(bankOperation.OperationTypeId, _userContext.LanguageId));

        if (bankOperation.BankOperationLines.Count != 1)
            return Result.Failure(BankOperationErrors.InvalidLineConfiguration(bankOperation.Id, _userContext.LanguageId));

        var line = bankOperation.BankOperationLines.Single();
        if (line.Amount != bankOperation.Amount || line.PaymentPurposeId <= 0)
            return Result.Failure(BankOperationErrors.InvalidLineConfiguration(bankOperation.Id, _userContext.LanguageId));

        if (bankOperation.BankAccount.OrganizationId != bankOperation.OrganizationId ||
            bankOperation.BankAccount.StateId != StateIdConst.ACTIVE)
            return Result.Failure(BankOperationErrors.InvalidOrganizationReference("BankAccount", _userContext.LanguageId));

        if (bankOperation.BankAccount.CurrencyId != bankOperation.CurrencyId)
            return Result.Failure(BankOperationErrors.InvalidCurrencyMismatch(_userContext.LanguageId));

        if (bankOperation.CounterpartyId.HasValue)
        {
            if (bankOperation.Counterparty == null ||
                bankOperation.Counterparty.OrganizationId != bankOperation.OrganizationId ||
                bankOperation.Counterparty.StateId != StateIdConst.ACTIVE)
                return Result.Failure(BankOperationErrors.InvalidOrganizationReference("Counterparty", _userContext.LanguageId));
        }

        if (bankOperation.CounterpartyBankAccountId.HasValue)
        {
            if (bankOperation.CounterpartyBankAccount == null ||
                bankOperation.CounterpartyBankAccount.OrganizationId != bankOperation.OrganizationId ||
                bankOperation.CounterpartyBankAccount.StateId != StateIdConst.ACTIVE)
                return Result.Failure(BankOperationErrors.InvalidOrganizationReference("CounterpartyBankAccount", _userContext.LanguageId));

            if (bankOperation.CounterpartyId.HasValue &&
                bankOperation.CounterpartyBankAccount.CounterpartyId != bankOperation.CounterpartyId.Value)
                return Result.Failure(BankOperationErrors.InvalidOrganizationReference("CounterpartyBankAccount", _userContext.LanguageId));
        }

        if (bankOperation.ContractId.HasValue)
        {
            if (bankOperation.Contract == null ||
                bankOperation.Contract.OrganizationId != bankOperation.OrganizationId ||
                bankOperation.Contract.StateId != StateIdConst.ACTIVE)
                return Result.Failure(BankOperationErrors.InvalidOrganizationReference("Contract", _userContext.LanguageId));

            if (bankOperation.CounterpartyId.HasValue &&
                bankOperation.Contract.CounterpartyId != bankOperation.CounterpartyId.Value)
                return Result.Failure(BankOperationErrors.InvalidOrganizationReference("Contract", _userContext.LanguageId));
        }

        if (line.PaymentPurpose.OperationTypeId != bankOperation.OperationTypeId)
            return Result.Failure(BankOperationErrors.InvalidLineConfiguration(bankOperation.Id, _userContext.LanguageId));

        if (line.PaymentPurpose.RequiresCounterparty && !bankOperation.CounterpartyId.HasValue)
            return Result.Failure(Error.Business("BankOperation.CounterpartyRequired", "Selected payment purpose requires a counterparty."));

        if (!SupportedPostingAliases.Contains(line.PaymentPurpose.Alias.Code))
            return Result.Failure(Error.Business("BankOperation.UnsupportedPaymentPurpose", $"Payment purpose alias '{line.PaymentPurpose.Alias.Code}' is not supported by current posting configuration."));

        if (bankOperation.OperationTypeId == OperationTypeIdConst.OUT)
        {
            var balance = await _moneyRegisterService.GetBankAccountBalanceAsync(bankOperation.BankAccountId, bankOperation.DocDate, ct);
            if (balance < bankOperation.Amount)
            {
                return Result.Failure(Error.Business(
                    "BankOperation.InsufficientBalance",
                    $"Bank account {bankOperation.BankAccountId} has insufficient balance for amount {bankOperation.Amount}."));
            }
        }

        return Result.Success();
    }

    private async Task<PostingBatch?> GetActivePostingBatchAsync(long bankOperationId, CancellationToken ct)
    {
        var query = _queryBuilder.For<PostingBatch>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.BANKOPERATION &&
                        x.DocumentId == bankOperationId &&
                        x.Status == PostingBatchStatusConst.POSTED)
            .Build();

        return await _postingBatchQuery.GetAsync(query, ct);
    }

    private async Task<bool> HasBusinessEffectsAsync(long bankOperationId, CancellationToken ct)
    {
        if (await _accountingRegisterQuery.AnyAsync(x =>
                x.DocumentTypeId == DocumentTypeIdConst.BANKOPERATION &&
                x.DocumentId == bankOperationId &&
                x.ReversalEntryId == null, ct))
            return true;

        if (await _moneyRegisterQuery.AnyAsync(x =>
                x.DocumentTypeId == DocumentTypeIdConst.BANKOPERATION &&
                x.DocumentId == bankOperationId &&
                x.ReversalEntryId == null, ct))
            return true;

        return await _counterpartyRegisterQuery.AnyAsync(x =>
            x.DocumentTypeId == DocumentTypeIdConst.BANKOPERATION &&
            x.DocumentId == bankOperationId &&
            x.ReversalEntryId == null, ct);
    }

    private async Task<PostingBatch> CreatePostingBatchAsync(BankOperation bankOperation, string status, string comment, CancellationToken ct)
    {
        var now = DateTime.Now;
        var batch = new PostingBatch
        {
            OrganizationId = bankOperation.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.BANKOPERATION,
            DocumentId = bankOperation.Id,
            Status = status,
            PostedByUserId = _userContext.Id,
            PostedAt = now,
            Comment = comment
        };

        await _postingBatchCommand.CreateAsync(batch, ct);
        return batch;
    }

    private async Task<Result> ReverseAccountingEntriesAsync(long bankOperationId, long reversalBatchId, CancellationToken ct)
    {
        var query = _queryBuilder.For<AccountingRegisterEntry>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.BANKOPERATION &&
                        x.DocumentId == bankOperationId &&
                        x.ReversalEntryId == null)
            .Build();
        query.AddIncludes(x => x.Include(entry => entry.RegisterEntrySubkontos));

        var entries = await _accountingRegisterQuery.GetAllAsync(query, ct);
        if (entries.Count == 0)
            return Result.Failure(BankOperationErrors.MissingAccountingRegisterEntries(bankOperationId, _userContext.LanguageId));

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
