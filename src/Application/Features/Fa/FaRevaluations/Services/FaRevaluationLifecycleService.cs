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

namespace Application.Features.FaRevaluations;

public class FaRevaluationLifecycleService : BaseService, IFaRevaluationLifecycleService
{
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAccountingPeriodValidator _periodValidator;
    private readonly IAuditLogService _auditLogService;
    private readonly IAccountingDispatcher _dispatcher;
    private readonly IFaDocumentAccountValidator _accountValidator;
    private readonly IQueryRepository<FaRevaluationDoc> _query;
    private readonly IFaRevaluationCommandRepository _command;
    private readonly IFaAssetCommandRepository _faAssetCommand;
    private readonly ICommandRepository<FaAssetAccounting> _faAssetAccountingCommand;
    private readonly IQueryRepository<FaDepreciationRunLine> _depreciationLineQuery;
    private readonly IQueryRepository<PostingBatch> _postingBatchQuery;
    private readonly ICommandRepository<PostingBatch> _postingBatchCommand;
    private readonly IQueryRepository<AccountingRegisterEntry> _accountingRegisterQuery;
    private readonly ICommandRepository<AccountingRegisterEntry> _accountingRegisterCommand;

    public FaRevaluationLifecycleService(
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IQueryBuilder queryBuilder,
        IDocumentPostingLock postingLock,
        IAccountingPeriodValidator periodValidator,
        IAuditLogService auditLogService,
        IAccountingDispatcher dispatcher,
        IFaDocumentAccountValidator accountValidator,
        IQueryRepository<FaRevaluationDoc> query,
        IFaRevaluationCommandRepository command,
        IFaAssetCommandRepository faAssetCommand,
        ICommandRepository<FaAssetAccounting> faAssetAccountingCommand,
        IQueryRepository<FaDepreciationRunLine> depreciationLineQuery,
        IQueryRepository<PostingBatch> postingBatchQuery,
        ICommandRepository<PostingBatch> postingBatchCommand,
        IQueryRepository<AccountingRegisterEntry> accountingRegisterQuery,
        ICommandRepository<AccountingRegisterEntry> accountingRegisterCommand,
        ILogger<FaRevaluationLifecycleService> logger)
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
        _faAssetAccountingCommand = faAssetAccountingCommand;
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

            await _postingLock.AcquireAsync(DocumentTypeIdConst.FAREVALUATION, id, ct);

            var doc = await GetAggregateAsync(id, ct);
            if (doc is null)
                return Result.Failure(FaRevaluationErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Failure(FaRevaluationErrors.CannotConfirmInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
            {
                var batch = await GetActivePostingBatchAsync(id, ct);
                return batch is not null
                    ? Result.Success()
                    : Result.Failure(FaRevaluationErrors.MissingPostingBatch(id, _userContext.LanguageId));
            }

            if (doc.StatusId != DocumentStatusIdConst.DRAFT && doc.StatusId != DocumentStatusIdConst.PENDING)
                return Result.Failure(FaRevaluationErrors.CannotConfirmInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var periodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, doc.RevaluationDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var oldDto = await GetByIdInternalAsync(id, ct);
            if (oldDto is not null)
                _auditLogService.SetOldValues(oldDto);

            foreach (var line in doc.Lines)
            {
                if (line.FaAsset.StatusId == FaAssetStatusIdConst.DISPOSED)
                    return Result.Failure(FaRevaluationErrors.AssetDisposed(line.FaAssetId, _userContext.LanguageId));

                var accounting = line.FaAsset.FaAssetAccounting;
                if (accounting is null)
                    return Result.Failure(FaRevaluationErrors.AssetInactive(line.FaAssetId, _userContext.LanguageId));

                var accumulated = await GetAccumulatedDepreciationAsync(line.FaAssetId, doc.RevaluationDate, ct);
                line.OldValue = Math.Max(0m, accounting.InitialCost - accumulated);
                line.RevaluationAmount = line.NewValue - line.OldValue;
                accounting.InitialCost = accumulated + line.NewValue;
                accounting.UpdatedDate = DateTime.Now;
                line.FaAsset.UpdatedDate = DateTime.Now;
                await _faAssetAccountingCommand.UpdateAsync(accounting, ct);
                await _faAssetCommand.UpdateAsync(line.FaAsset, ct);
            }

            var accountValidation = await ValidateAccountsAsync(doc, ct);
            if (!accountValidation.IsSuccess)
                return accountValidation;

            var now = DateTime.Now;
            var postingBatch = new PostingBatch
            {
                OrganizationId = doc.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.FAREVALUATION,
                DocumentId = doc.Id,
                Status = PostingBatchStatusConst.POSTED,
                PostedByUserId = _userContext.Id,
                PostedAt = now,
                Comment = "Fixed asset revaluation confirmed"
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
                await _auditLogService.CreateAsync(AuditLogTableConst.FaRevaluationDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Confirmed");
            }

            return Result.Success();
        }, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.FAREVALUATION, id, ct);

            var doc = await GetAggregateAsync(id, ct);
            if (doc is null)
                return Result.Failure(FaRevaluationErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();

            if (doc.StatusId != DocumentStatusIdConst.DRAFT &&
                doc.StatusId != DocumentStatusIdConst.PENDING &&
                doc.StatusId != DocumentStatusIdConst.POSTED)
            {
                return Result.Failure(FaRevaluationErrors.CannotCancelInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));
            }

            var periodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, doc.RevaluationDate, ct);
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
                    return Result.Failure(FaRevaluationErrors.MissingPostingBatch(id, _userContext.LanguageId));

                var reversalBatch = new PostingBatch
                {
                    OrganizationId = doc.OrganizationId,
                    DocumentTypeId = DocumentTypeIdConst.FAREVALUATION,
                    DocumentId = doc.Id,
                    Status = PostingBatchStatusConst.REVERSAL,
                    PostedByUserId = _userContext.Id,
                    PostedAt = now,
                    Comment = "Fixed asset revaluation cancelled"
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
                    var accounting = line.FaAsset.FaAssetAccounting;
                    if (accounting is null)
                        return Result.Failure(FaRevaluationErrors.AssetInactive(line.FaAssetId, _userContext.LanguageId));

                    var accumulated = await GetAccumulatedDepreciationAsync(line.FaAssetId, doc.RevaluationDate, ct);
                    accounting.InitialCost = accumulated + line.OldValue;
                    accounting.UpdatedDate = now;
                    line.FaAsset.UpdatedDate = now;
                    await _faAssetAccountingCommand.UpdateAsync(accounting, ct);
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
                await _auditLogService.CreateAsync(AuditLogTableConst.FaRevaluationDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Cancelled");
            }

            return Result.Success();
        }, ct);

    private Task<Result> ValidateAccountsAsync(
        FaRevaluationDoc document,
        CancellationToken ct)
    {
        var requirements = document.Lines
            .Where(line => line.RevaluationAmount != 0m)
            .Select(line => new FaDocumentAccountRequirement(
                line.AssetAccountId,
                FaDocumentAccountRoleCodeConst.FixedAsset))
            .ToList();

        requirements.Add(new FaDocumentAccountRequirement(
            document.RevaluationReserveAccountId,
            FaDocumentAccountRoleCodeConst.RevaluationReserve,
            document.Lines.Any(line => line.RevaluationAmount > 0m)));
        requirements.Add(new FaDocumentAccountRequirement(
            document.RevaluationLossAccountId,
            FaDocumentAccountRoleCodeConst.RevaluationLoss,
            document.Lines.Any(line => line.RevaluationAmount < 0m)));

        return _accountValidator.ValidateAsync(
            document.OrganizationId,
            DocumentTypeIdConst.FAREVALUATION,
            requirements,
            ct);
    }
    private async Task<FaRevaluationDoc?> GetAggregateAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaRevaluationDoc>().Where(x => x.Id == id).Build();
        query.AddIncludes(x => x.Include(d => d.Lines).ThenInclude(l => l.FaAsset).ThenInclude(a => a.FaAssetAccounting));
        return await _query.GetAsync(query, ct);
    }

    private async Task<FaRevaluationDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaRevaluationDoc>().Where(x => x.Id == id).As<FaRevaluationDto>().Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<PostingBatch?> GetActivePostingBatchAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PostingBatch>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.FAREVALUATION &&
                        x.DocumentId == id &&
                        x.Status == PostingBatchStatusConst.POSTED)
            .Build();
        return await _postingBatchQuery.GetAsync(query, ct);
    }

    private async Task<decimal> GetAccumulatedDepreciationAsync(long faAssetId, DateTime revaluationDate, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaDepreciationRunLine>()
            .Where(x => x.FaAssetId == faAssetId &&
                        x.DepreciationRun.StatusId == DocumentStatusIdConst.POSTED &&
                        x.DepreciationRun.PeriodMonth <= revaluationDate)
            .Build();
        var lines = await _depreciationLineQuery.GetAllAsync(query, ct);
        return lines.Sum(x => x.Amount);
    }

    private async Task<Result> ReverseAccountingEntriesAsync(long docId, long reversalBatchId, CancellationToken ct)
    {
        var query = _queryBuilder.For<AccountingRegisterEntry>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.FAREVALUATION &&
                        x.DocumentId == docId &&
                        x.ReversalEntryId == null)
            .Build();
        query.AddIncludes(x => x.Include(e => e.RegisterEntrySubkontos));

        var entries = await _accountingRegisterQuery.GetAllAsync(query, ct);
        if (entries.Count == 0)
            return Result.Success();

        var reversals = AccountingRegisterEntryReversalFactory.Create(
            entries,
            reversalBatchId);

        await _accountingRegisterCommand.CreateAsync(reversals, ct);
        return Result.Success();
    }
}
