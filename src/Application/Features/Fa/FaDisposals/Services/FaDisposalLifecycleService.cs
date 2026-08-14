using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.Fa;
using Application.Features.FaAssets;
using Application.Features.Register.AccountingRegisterEntries;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.FaDisposals;

public class FaDisposalLifecycleService : BaseService, IFaDisposalLifecycleService
{
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAccountingPeriodValidator _periodValidator;
    private readonly IAuditLogService _auditLogService;
    private readonly IAccountingDispatcher _dispatcher;
    private readonly IFaDocumentAccountValidator _accountValidator;
    private readonly IQueryRepository<FaDisposalDoc> _query;
    private readonly IFaDisposalCommandRepository _command;
    private readonly IFaAssetCommandRepository _faAssetCommand;
    private readonly IQueryRepository<FaDepreciationRunLine> _depreciationLineQuery;
    private readonly IQueryRepository<PostingBatch> _postingBatchQuery;
    private readonly ICommandRepository<PostingBatch> _postingBatchCommand;
    private readonly IQueryRepository<AccountingRegisterEntry> _accountingRegisterQuery;
    private readonly ICommandRepository<AccountingRegisterEntry> _accountingRegisterCommand;

    public FaDisposalLifecycleService(
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IQueryBuilder queryBuilder,
        IDocumentPostingLock postingLock,
        IAccountingPeriodValidator periodValidator,
        IAuditLogService auditLogService,
        IAccountingDispatcher dispatcher,
        IFaDocumentAccountValidator accountValidator,
        IQueryRepository<FaDisposalDoc> query,
        IFaDisposalCommandRepository command,
        IFaAssetCommandRepository faAssetCommand,
        IQueryRepository<FaDepreciationRunLine> depreciationLineQuery,
        IQueryRepository<PostingBatch> postingBatchQuery,
        ICommandRepository<PostingBatch> postingBatchCommand,
        IQueryRepository<AccountingRegisterEntry> accountingRegisterQuery,
        ICommandRepository<AccountingRegisterEntry> accountingRegisterCommand,
        ILogger<FaDisposalLifecycleService> logger)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _unitOfWork = unitOfWork;
        _queryBuilder = queryBuilder;
        _postingLock = postingLock;
        _periodValidator = periodValidator;
        _auditLogService = auditLogService;
        _dispatcher = dispatcher;
        _accountValidator = accountValidator;
        _query = query;
        _command = command;
        _faAssetCommand = faAssetCommand;
        _depreciationLineQuery = depreciationLineQuery;
        _postingBatchQuery = postingBatchQuery;
        _postingBatchCommand = postingBatchCommand;
        _accountingRegisterQuery = accountingRegisterQuery;
        _accountingRegisterCommand = accountingRegisterCommand;
    }

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ConfirmAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.FADISPOSAL, id, ct);

            var doc = await GetAggregateAsync(id, ct);
            if (doc is null)
                return Result.Failure(FaDisposalErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Failure(FaDisposalErrors.CannotConfirmInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
            {
                var batch = await GetActivePostingBatchAsync(id, ct);
                return batch is not null
                    ? Result.Success()
                    : Result.Failure(FaDisposalErrors.MissingPostingBatch(id, _userContext.LanguageId));
            }

            if (doc.StatusId != DocumentStatusIdConst.DRAFT && doc.StatusId != DocumentStatusIdConst.PENDING)
                return Result.Failure(FaDisposalErrors.CannotConfirmInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var periodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, doc.DisposalDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var oldDto = await GetByIdInternalAsync(id, ct);
            if (oldDto is not null)
                _auditLogService.SetOldValues(oldDto);

            foreach (var line in doc.Lines)
            {
                if (line.FaAsset.StatusId == FaAssetStatusIdConst.DISPOSED)
                    return Result.Failure(FaDisposalErrors.AssetAlreadyDisposed(line.FaAssetId, _userContext.LanguageId));

                var accounting = line.FaAsset.FaAssetAccounting;
                if (accounting is null)
                    return Result.Failure(FaDisposalErrors.AssetInactive(line.FaAssetId, _userContext.LanguageId));

                var accumulated = await GetAccumulatedDepreciationAsync(line.FaAssetId, doc.DisposalDate, ct);
                line.BookValue = Math.Max(0m, accounting.InitialCost - accumulated);
                line.GainLoss = line.SaleAmount - line.BookValue;

                line.FaAsset.StatusId = FaAssetStatusIdConst.DISPOSED;
                line.FaAsset.UpdatedDate = DateTime.Now;
                await _faAssetCommand.UpdateAsync(line.FaAsset, ct);
            }

            var accountValidation = await ValidateAccountsAsync(doc, ct);
            if (!accountValidation.IsSuccess)
                return accountValidation;

            var now = DateTime.Now;
            var postingBatch = new PostingBatch
            {
                OrganizationId = doc.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.FADISPOSAL,
                DocumentId = doc.Id,
                Status = PostingBatchStatusConst.POSTED,
                PostedByUserId = _userContext.Id,
                PostedAt = now,
                Comment = "Fixed asset disposal confirmed"
            };

            await _postingBatchCommand.CreateAsync(postingBatch, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var postingResult = await _dispatcher.ProcessAsync(doc, ct, postingBatch.Id);
            if (!postingResult.IsSuccess)
                return postingResult;

            doc.StatusId = DocumentStatusIdConst.POSTED;
            doc.PostedAt = now;
            doc.PostedByUserId = _userContext.Id;
            doc.UpdatedDate = now;
            doc.UpdatedByUserId = _userContext.Id;
            await _command.UpdateAsync(doc, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var newDto = await GetByIdInternalAsync(id, ct);
            if (newDto is not null)
            {
                _auditLogService.SetNewValues(newDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.FaDisposalDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Confirmed");
            }

            return Result.Success();
        }, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.FADISPOSAL, id, ct);

            var doc = await GetAggregateAsync(id, ct);
            if (doc is null)
                return Result.Failure(FaDisposalErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();

            if (doc.StatusId != DocumentStatusIdConst.DRAFT &&
                doc.StatusId != DocumentStatusIdConst.PENDING &&
                doc.StatusId != DocumentStatusIdConst.POSTED)
            {
                return Result.Failure(FaDisposalErrors.CannotCancelInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));
            }

            var periodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, doc.DisposalDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
            {
                var reversalPeriodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, DateTime.Now, ct);
                if (!reversalPeriodValidation.IsSuccess)
                    return reversalPeriodValidation;
            }

            var oldDto = await GetByIdInternalAsync(id, ct);
            if (oldDto is not null)
                _auditLogService.SetOldValues(oldDto);

            var now = DateTime.Now;
            if (doc.StatusId == DocumentStatusIdConst.POSTED)
            {
                var activePostingBatch = await GetActivePostingBatchAsync(id, ct);
                if (activePostingBatch == null)
                    return Result.Failure(FaDisposalErrors.MissingPostingBatch(id, _userContext.LanguageId));

                var reversalBatch = new PostingBatch
                {
                    OrganizationId = doc.OrganizationId,
                    DocumentTypeId = DocumentTypeIdConst.FADISPOSAL,
                    DocumentId = doc.Id,
                    Status = PostingBatchStatusConst.REVERSAL,
                    PostedByUserId = _userContext.Id,
                    PostedAt = now,
                    Comment = "Fixed asset disposal cancelled"
                };

                await _postingBatchCommand.CreateAsync(reversalBatch, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                var reverseResult = await ReverseAccountingEntriesAsync(id, reversalBatch.Id, ct);
                if (!reverseResult.IsSuccess)
                    return reverseResult;

                activePostingBatch.Status = PostingBatchStatusConst.REVERSED;
                activePostingBatch.ReversedAt = now;
                activePostingBatch.ReversedByUserId = _userContext.Id;
                await _postingBatchCommand.UpdateAsync(activePostingBatch, ct);

                foreach (var line in doc.Lines)
                {
                    line.FaAsset.StatusId = FaAssetStatusIdConst.ACTIVE;
                    line.FaAsset.UpdatedDate = now;
                    await _faAssetCommand.UpdateAsync(line.FaAsset, ct);
                }
            }

            doc.StatusId = DocumentStatusIdConst.CANCELLED;
            doc.CancelledAt = now;
            doc.CancelledByUserId = _userContext.Id;
            doc.UpdatedDate = now;
            doc.UpdatedByUserId = _userContext.Id;
            await _command.UpdateAsync(doc, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var newDto = await GetByIdInternalAsync(id, ct);
            if (newDto is not null)
            {
                _auditLogService.SetNewValues(newDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.FaDisposalDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Cancelled");
            }

            return Result.Success();
        }, ct);

    private Task<Result> ValidateAccountsAsync(
        FaDisposalDoc document,
        CancellationToken ct)
    {
        var requirements = document.Lines
            .SelectMany(line => new[]
            {
                new FaDocumentAccountRequirement(
                    line.AssetAccountId,
                    FaDocumentAccountRoleCodeConst.FixedAsset),
                new FaDocumentAccountRequirement(
                    line.AccumulatedDepreciationAccountId,
                    FaDocumentAccountRoleCodeConst.AccumulatedDepreciation,
                    line.FaAsset.FaAssetAccounting!.InitialCost > line.BookValue)
            })
            .ToList();

        requirements.Add(new FaDocumentAccountRequirement(
            document.DisposalAccountId,
            FaDocumentAccountRoleCodeConst.Disposal,
            document.Lines.Any(line =>
                line.BookValue != 0m ||
                line.SaleAmount != 0m ||
                line.GainLoss != 0m)));
        requirements.Add(new FaDocumentAccountRequirement(
            document.CustomerAccountId,
            FaDocumentAccountRoleCodeConst.CustomerSettlement,
            document.Lines.Any(line => line.SaleAmount != 0m)));
        requirements.Add(new FaDocumentAccountRequirement(
            document.GainAccountId,
            FaDocumentAccountRoleCodeConst.DisposalGain,
            document.Lines.Any(line => line.GainLoss > 0m)));
        requirements.Add(new FaDocumentAccountRequirement(
            document.LossAccountId,
            FaDocumentAccountRoleCodeConst.DisposalLoss,
            document.Lines.Any(line => line.GainLoss < 0m)));
        requirements.Add(new FaDocumentAccountRequirement(
            document.VatAccountId,
            FaDocumentAccountRoleCodeConst.InputVat,
            false));

        return _accountValidator.ValidateAsync(
            document.OrganizationId,
            DocumentTypeIdConst.FADISPOSAL,
            requirements,
            ct);
    }
    private async Task<FaDisposalDoc?> GetAggregateAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaDisposalDoc>().Where(x => x.Id == id).Build();
        query.AddIncludes(x => x.Include(d => d.Lines).ThenInclude(l => l.FaAsset).ThenInclude(a => a.FaAssetAccounting));
        return await _query.GetAsync(query, ct);
    }

    private async Task<FaDisposalDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaDisposalDoc>().Where(x => x.Id == id).As<FaDisposalDto>().Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<PostingBatch?> GetActivePostingBatchAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PostingBatch>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.FADISPOSAL &&
                        x.DocumentId == id &&
                        x.Status == PostingBatchStatusConst.POSTED)
            .Build();
        return await _postingBatchQuery.GetAsync(query, ct);
    }

    private async Task<decimal> GetAccumulatedDepreciationAsync(long faAssetId, DateTime disposalDate, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaDepreciationRunLine>()
            .Where(x => x.FaAssetId == faAssetId &&
                        x.DepreciationRun.StatusId == DocumentStatusIdConst.POSTED &&
                        x.DepreciationRun.PeriodMonth <= disposalDate)
            .Build();
        var lines = await _depreciationLineQuery.GetAllAsync(query, ct);
        return lines.Sum(x => x.Amount);
    }

    private async Task<Result> ReverseAccountingEntriesAsync(long docId, long reversalBatchId, CancellationToken ct)
    {
        var query = _queryBuilder.For<AccountingRegisterEntry>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.FADISPOSAL &&
                        x.DocumentId == docId &&
                        x.ReversalEntryId == null)
            .Build();
        query.AddIncludes(x => x.Include(e => e.RegisterEntrySubkontos));

        var entries = await _accountingRegisterQuery.GetAllAsync(query, ct);
        if (entries.Count == 0)
            return Result.Success();

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

        await _accountingRegisterCommand.CreateAsync(reversals, ct);
        return Result.Success();
    }
}
