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

namespace Application.Features.Rnt.RentalAccruals;

public sealed class RentalAccrualLifecycleService : BaseService, IRentalAccrualLifecycleService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAccountingPeriodValidator _periodValidator;
    private readonly IAccountingDispatcher _dispatcher;
    private readonly IAuditLogService _auditLogService;
    private readonly IQueryRepository<RentalAccrualDoc> _query;
    private readonly ICommandRepository<RentalAccrualDoc> _command;
    private readonly IQueryRepository<ChartAccount> _accountQuery;
    private readonly IQueryRepository<PostingBatch> _batchQuery;
    private readonly ICommandRepository<PostingBatch> _batchCommand;
    private readonly IQueryRepository<AccountingRegisterEntry> _entryQuery;
    private readonly ICommandRepository<AccountingRegisterEntry> _entryCommand;

    public RentalAccrualLifecycleService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IDocumentPostingLock postingLock,
        IAccountingPeriodValidator periodValidator,
        IAccountingDispatcher dispatcher,
        IAuditLogService auditLogService,
        IQueryRepository<RentalAccrualDoc> query,
        ICommandRepository<RentalAccrualDoc> command,
        IQueryRepository<ChartAccount> accountQuery,
        IQueryRepository<PostingBatch> batchQuery,
        ICommandRepository<PostingBatch> batchCommand,
        IQueryRepository<AccountingRegisterEntry> entryQuery,
        ICommandRepository<AccountingRegisterEntry> entryCommand,
        ILogger<RentalAccrualLifecycleService> logger,
        IUnitOfWork unitOfWork) : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _postingLock = postingLock;
        _periodValidator = periodValidator;
        _dispatcher = dispatcher;
        _auditLogService = auditLogService;
        _query = query;
        _command = command;
        _accountQuery = accountQuery;
        _batchQuery = batchQuery;
        _batchCommand = batchCommand;
        _entryQuery = entryQuery;
        _entryCommand = entryCommand;
    }

    public Task<Result> PostAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(PostAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.RENTAL_ACCRUAL, id, ct);
            var document = await GetDocumentAsync(id, ct);
            if (document is null)
                return Result.Failure(RentalAccrualErrors.NotFound(id, _userContext.LanguageId));
            if (document.StatusId == DocumentStatusIdConst.POSTED)
                return await GetActiveBatchAsync(id, ct) is not null
                    ? Result.Success()
                    : Result.Failure(RentalAccrualErrors.MissingPostingBatch(id, _userContext.LanguageId));
            if (document.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(RentalAccrualErrors.InvalidStatus(id, document.StatusId, _userContext.LanguageId));

            var period = await _periodValidator.EnsureOpenAsync(document.OrganizationId, document.DocDate, ct);
            if (!period.IsSuccess)
                return period;
            var validation = await ValidateAsync(document, ct);
            if (!validation.IsSuccess)
                return validation;
            if (await GetActiveBatchAsync(id, ct) is not null ||
                await _entryQuery.AnyAsync(x => x.DocumentTypeId == DocumentTypeIdConst.RENTAL_ACCRUAL && x.DocumentId == id && x.ReversalEntryId == null, ct))
                return Result.Failure(RentalAccrualErrors.EffectsAlreadyExist(id, _userContext.LanguageId));

            var old = await GetDtoAsync(id, ct);
            if (old is not null)
                _auditLogService.SetOldValues(old);

            var batch = await CreateBatchAsync(document, PostingBatchStatusConst.POSTED, "Rental accrual posted", ct);
            var posting = await _dispatcher.ProcessAsync(document, ct, batch.Id);
            if (!posting.IsSuccess)
                return Result.Failure(posting.Error);

            document.StatusId = DocumentStatusIdConst.POSTED;
            document.PostedAt = DateTime.Now;
            document.PostedByUserId = _userContext.Id;
            document.UpdatedDate = DateTime.Now;
            document.UpdatedByUserId = _userContext.Id;
            await _command.UpdateAsync(document, ct);

            var updated = await GetDtoAsync(id, ct);
            if (updated is not null)
            {
                _auditLogService.SetNewValues(updated);
                await _auditLogService.CreateAsync(AuditLogTableConst.RentalAccrual, id.ToString(), AuditLogOperationTypeConst.Update, "Posted");
            }
            return Result.Success();
        }, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.RENTAL_ACCRUAL, id, ct);
            var document = await GetDocumentAsync(id, ct);
            if (document is null)
                return Result.Failure(RentalAccrualErrors.NotFound(id, _userContext.LanguageId));
            if (document.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();
            if (document.StatusId is not (DocumentStatusIdConst.DRAFT or DocumentStatusIdConst.POSTED))
                return Result.Failure(RentalAccrualErrors.InvalidStatus(id, document.StatusId, _userContext.LanguageId));

            var old = await GetDtoAsync(id, ct);
            if (old is not null)
                _auditLogService.SetOldValues(old);

            if (document.StatusId == DocumentStatusIdConst.POSTED)
            {
                var originalPeriod = await _periodValidator.EnsureOpenAsync(document.OrganizationId, document.DocDate, ct);
                if (!originalPeriod.IsSuccess)
                    return originalPeriod;
                var reversalPeriod = await _periodValidator.EnsureOpenAsync(document.OrganizationId, DateTime.Now, ct);
                if (!reversalPeriod.IsSuccess)
                    return reversalPeriod;

                var activeBatch = await GetActiveBatchAsync(id, ct);
                if (activeBatch is null)
                    return Result.Failure(RentalAccrualErrors.MissingPostingBatch(id, _userContext.LanguageId));
                var reversalBatch = await CreateBatchAsync(document, PostingBatchStatusConst.REVERSAL, "Rental accrual cancelled", ct);
                var reversal = await ReverseEntriesAsync(document.Id, reversalBatch.Id, ct);
                if (!reversal.IsSuccess)
                    return reversal;

                activeBatch.Status = PostingBatchStatusConst.REVERSED;
                activeBatch.ReversedAt = DateTime.Now;
                activeBatch.ReversedByUserId = _userContext.Id;
                await _batchCommand.UpdateAsync(activeBatch, ct);
            }

            document.StatusId = DocumentStatusIdConst.CANCELLED;
            document.CancelledAt = DateTime.Now;
            document.CancelledByUserId = _userContext.Id;
            document.UpdatedDate = DateTime.Now;
            document.UpdatedByUserId = _userContext.Id;
            await _command.UpdateAsync(document, ct);

            var updated = await GetDtoAsync(id, ct);
            if (updated is not null)
            {
                _auditLogService.SetNewValues(updated);
                await _auditLogService.CreateAsync(AuditLogTableConst.RentalAccrual, id.ToString(), AuditLogOperationTypeConst.Update, "Cancelled");
            }
            return Result.Success();
        }, ct);

    private async Task<Result> ValidateAsync(RentalAccrualDoc document, CancellationToken ct)
    {
        if (document.ExchangeRate <= 0m || document.CurrencyId != document.Contract.CurrencyId ||
            document.Items.Any(x => x.ContractObject.ContractId != document.ContractId))
            return Result.Failure(RentalAccrualErrors.InvalidAmounts(document.Id, _userContext.LanguageId));

        foreach (var item in document.Items)
        {
            var expected = RentalAccrualCalculator.Calculate(item.ContractAmount, item.TaxBaseAmount, item.TaxRate);
            if (item.PeriodTo.Date < item.PeriodFrom.Date ||
                item.TaxAmount != expected.TaxAmount ||
                item.PayableAmount != expected.PayableAmount ||
                item.Amount != expected.Amount)
                return Result.Failure(RentalAccrualErrors.InvalidAmounts(document.Id, _userContext.LanguageId));
        }

        if (document.ContractAmount != document.Items.Sum(x => x.ContractAmount) ||
            document.TaxBaseAmount != document.Items.Sum(x => x.TaxBaseAmount) ||
            document.TaxAmount != document.Items.Sum(x => x.TaxAmount) ||
            document.PayableAmount != document.Items.Sum(x => x.PayableAmount) ||
            document.Amount != document.Items.Sum(x => x.Amount))
            return Result.Failure(RentalAccrualErrors.InvalidAmounts(document.Id, _userContext.LanguageId));

        if (!document.LessorPayableAccountId.HasValue || !document.TaxPayableAccountId.HasValue ||
            document.Items.Count == 0 || document.Items.Any(x => !x.ExpenseAccountId.HasValue))
            return Result.Failure(RentalAccrualErrors.MissingAccounts(document.Id, _userContext.LanguageId));

        var lessorAccountId = document.LessorPayableAccountId.Value;
        var taxAccountId = document.TaxPayableAccountId.Value;
        if (lessorAccountId == taxAccountId ||
            document.Items.Any(x => x.ExpenseAccountId == lessorAccountId || x.ExpenseAccountId == taxAccountId))
            return Result.Failure(RentalAccrualErrors.InvalidAccounts(_userContext.LanguageId));

        var accountIds = document.Items.Select(x => x.ExpenseAccountId!.Value)
            .Append(lessorAccountId)
            .Append(taxAccountId)
            .Distinct()
            .ToArray();
        var query = _queryBuilder.For<ChartAccount>()
            .Where(x => accountIds.Contains(x.Id) && x.OrganizationId == document.OrganizationId && x.StateId == StateIdConst.ACTIVE)
            .As(x => x.Id)
            .Build();
        return (await _accountQuery.GetAllAsync(query, ct)).Count == accountIds.Length
            ? Result.Success()
            : Result.Failure(RentalAccrualErrors.InvalidAccounts(_userContext.LanguageId));
    }

    private async Task<RentalAccrualDoc?> GetDocumentAsync(long id, CancellationToken ct)
    {
        var organizationId = _userContext.OrganizationId!.Value;
        var query = _queryBuilder.For<RentalAccrualDoc>()
            .Where(x => x.Id == id && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
            .Build();
        query.AddIncludes(x => x.Include(d => d.Contract));
        query.AddIncludes(x => x.Include(d => d.Items).ThenInclude(i => i.ContractObject));
        return await _query.GetAsync(query, ct);
    }

    private async Task<RentalAccrualDocDto?> GetDtoAsync(long id, CancellationToken ct)
    {
        var organizationId = _userContext.OrganizationId!.Value;
        var query = _queryBuilder.For<RentalAccrualDoc>()
            .Where(x => x.Id == id && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
            .As<RentalAccrualDocDto>()
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<PostingBatch?> GetActiveBatchAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PostingBatch>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.RENTAL_ACCRUAL && x.DocumentId == id && x.Status == PostingBatchStatusConst.POSTED)
            .Build();
        return await _batchQuery.GetAsync(query, ct);
    }

    private async Task<PostingBatch> CreateBatchAsync(RentalAccrualDoc document, string status, string comment, CancellationToken ct)
    {
        var batch = new PostingBatch
        {
            OrganizationId = document.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.RENTAL_ACCRUAL,
            DocumentId = document.Id,
            Status = status,
            PostedByUserId = _userContext.Id,
            PostedAt = DateTime.Now,
            Comment = comment
        };
        await _batchCommand.CreateAsync(batch, ct);
        return batch;
    }

    private async Task<Result> ReverseEntriesAsync(long id, long reversalBatchId, CancellationToken ct)
    {
        var query = _queryBuilder.For<AccountingRegisterEntry>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.RENTAL_ACCRUAL && x.DocumentId == id && x.ReversalEntryId == null)
            .Build();
        query.AddIncludes(x => x.Include(e => e.RegisterEntrySubkontos));
        var originals = await _entryQuery.GetAllAsync(query, ct);
        if (originals.Count == 0)
            return Result.Failure(RentalAccrualErrors.MissingAccountingEntries(id, _userContext.LanguageId));

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
        await _entryCommand.CreateAsync(reversals, ct);
        return Result.Success();
    }
}
