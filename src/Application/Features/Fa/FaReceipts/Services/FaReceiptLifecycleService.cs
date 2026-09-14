using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.Fa;
using Application.Features.FaAssets;
using Application.Features.Register.AccountingRegisterEntries;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.FaReceipts;

public class FaReceiptLifecycleService : BaseService, IFaReceiptLifecycleService
{
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAccountingPeriodValidator _periodValidator;
    private readonly IAuditLogService _auditLogService;
    private readonly IAccountingDispatcher _dispatcher;
    private readonly IFaDocumentAccountValidator _accountValidator;
    private readonly IQueryRepository<FaReceiptDoc> _query;
    private readonly IFaReceiptCommandRepository _command;
    private readonly IFaAssetCommandRepository _faAssetCommand;
    private readonly ICommandRepository<FaAssetAccounting> _assetAccountingCommand;
    private readonly IQueryRepository<FaCommissioningDocLine> _commissioningLineQuery;
    private readonly IQueryRepository<FaMovementDocLine> _movementLineQuery;
    private readonly IQueryRepository<FaDepreciationRunLine> _depreciationLineQuery;
    private readonly IQueryRepository<FaRevaluationDocLine> _revaluationLineQuery;
    private readonly IQueryRepository<FaDisposalDocLine> _disposalLineQuery;
    private readonly IQueryRepository<PostingBatch> _postingBatchQuery;
    private readonly ICommandRepository<PostingBatch> _postingBatchCommand;
    private readonly IQueryRepository<AccountingRegisterEntry> _accountingRegisterQuery;
    private readonly ICommandRepository<AccountingRegisterEntry> _accountingRegisterCommand;

    public FaReceiptLifecycleService(
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IQueryBuilder queryBuilder,
        IDocumentPostingLock postingLock,
        IAccountingPeriodValidator periodValidator,
        IAuditLogService auditLogService,
        IAccountingDispatcher dispatcher,
        IFaDocumentAccountValidator accountValidator,
        IQueryRepository<FaReceiptDoc> query,
        IFaReceiptCommandRepository command,
        IFaAssetCommandRepository faAssetCommand,
        ICommandRepository<FaAssetAccounting> assetAccountingCommand,
        IQueryRepository<FaCommissioningDocLine> commissioningLineQuery,
        IQueryRepository<FaMovementDocLine> movementLineQuery,
        IQueryRepository<FaDepreciationRunLine> depreciationLineQuery,
        IQueryRepository<FaRevaluationDocLine> revaluationLineQuery,
        IQueryRepository<FaDisposalDocLine> disposalLineQuery,
        IQueryRepository<PostingBatch> postingBatchQuery,
        ICommandRepository<PostingBatch> postingBatchCommand,
        IQueryRepository<AccountingRegisterEntry> accountingRegisterQuery,
        ICommandRepository<AccountingRegisterEntry> accountingRegisterCommand,
        ILogger<FaReceiptLifecycleService> logger)
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
        _assetAccountingCommand = assetAccountingCommand;
        _commissioningLineQuery = commissioningLineQuery;
        _movementLineQuery = movementLineQuery;
        _depreciationLineQuery = depreciationLineQuery;
        _revaluationLineQuery = revaluationLineQuery;
        _disposalLineQuery = disposalLineQuery;
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

            await _postingLock.AcquireAsync(DocumentTypeIdConst.FARECEIPT, id, ct);

            var doc = await GetAggregateAsync(id, ct);
            if (doc is null)
                return Result.Failure(FaReceiptErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Failure(FaReceiptErrors.AlreadyCancelled(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
            {
                var existingBatch = await GetActivePostingBatchAsync(id, ct);
                return existingBatch is not null
                    ? Result.Success()
                    : Result.Failure(
                        FaReceiptErrors.MissingPostingBatch(
                            id,
                            _userContext.LanguageId));
            }

            if (doc.StatusId is not (DocumentStatusIdConst.DRAFT or DocumentStatusIdConst.PENDING))
            {
                return Result.Failure(
                    FaReceiptErrors.CannotConfirmInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));
            }

            var periodValidation = await _periodValidator.EnsureOpenAsync(
                doc.OrganizationId,
                doc.DocDate,
                ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var businessValidation = ValidateForConfirm(doc);
            if (!businessValidation.IsSuccess)
                return businessValidation;

            var accountValidation = await ValidateAccountsAsync(doc, ct);
            if (!accountValidation.IsSuccess)
                return accountValidation;

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto is not null)
                _auditLogService.SetOldValues(oldDocDto);

            var now = DateTime.Now;
            var newAssets = new List<(FaReceiptDocAsset ReceiptAsset, FaAsset Asset)>();

            foreach (var receiptAsset in doc.Lines.SelectMany(line => line.Assets))
            {
                if (receiptAsset.FaAssetId.HasValue)
                    continue;

                var asset = new FaAsset
                {
                    OrganizationId = doc.OrganizationId,
                    StateId = StateIdConst.ACTIVE,
                    InventoryNumber = receiptAsset.InventoryNumber,
                    Name = receiptAsset.Name,
                    FaGroupId = receiptAsset.FaGroupId,
                    OkofId = receiptAsset.OkofId,
                    DepartmentId = null,
                    ResponsibleUserId = null,
                    StatusId = FaAssetStatusIdConst.NOT_COMMISSIONED,
                    CreatedDate = now,
                    UpdatedDate = now
                };

                await _faAssetCommand.CreateAsync(asset, ct);
                newAssets.Add((receiptAsset, asset));
            }

            if (newAssets.Count > 0)
            {
                await _unitOfWork.SaveChangesAsync(ct);

                var accountingRows = new List<FaAssetAccounting>();
                foreach (var item in newAssets)
                {
                    item.ReceiptAsset.FaAssetId = item.Asset.Id;
                    item.ReceiptAsset.FaAsset = item.Asset;

                    accountingRows.Add(new FaAssetAccounting
                    {
                        AssetId = item.Asset.Id,
                        InitialCost = item.ReceiptAsset.InitialCost,
                        AssetAccountId = item.ReceiptAsset.AssetAccountId,
                        SalvageValue = null,
                        DepreciationMethodId = null,
                        UsefulLifeMonths = null,
                        DeprStartDate = null,
                        PlannedUnitsTotal = null,
                        AccumulatedDepreciationAccountId = null,
                        DepreciationExpenseAccountId = null,
                        CreatedDate = now,
                        UpdatedDate = now
                    });
                }

                await _assetAccountingCommand.CreateAsync(accountingRows, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }

            var postingBatch = await CreatePostingBatchAsync(
                doc,
                PostingBatchStatusConst.POSTED,
                "Fixed asset receipt confirmed",
                ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var dispatch = await _dispatcher.ProcessAsync(doc, ct, postingBatch.Id);
            if (!dispatch.IsSuccess)
                return Result.Failure(dispatch.Error);

            doc.StatusId = DocumentStatusIdConst.POSTED;
            doc.PostedAt ??= now;
            doc.PostedByUserId ??= _userContext.Id;
            doc.UpdatedDate = now;

            await _command.UpdateAsync(doc, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto is not null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(
                    AuditLogTableConst.FaReceiptDoc,
                    id.ToString(),
                    AuditLogOperationTypeConst.Update,
                    "Confirmed");
            }

            return Result.Success();
        }, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.FARECEIPT, id, ct);

            var doc = await GetAggregateAsync(id, ct);
            if (doc is null)
                return Result.Failure(FaReceiptErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();

            if (doc.StatusId is not (
                    DocumentStatusIdConst.DRAFT or
                    DocumentStatusIdConst.PENDING or
                    DocumentStatusIdConst.POSTED))
            {
                return Result.Failure(
                    FaReceiptErrors.CannotCancelInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));
            }

            var periodValidation = await _periodValidator.EnsureOpenAsync(
                doc.OrganizationId,
                doc.DocDate,
                ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
            {
                var dependencyValidation = await EnsureNoPostedDependenciesAsync(doc, ct);
                if (!dependencyValidation.IsSuccess)
                    return dependencyValidation;

                var reversalPeriodValidation = await _periodValidator.EnsureOpenAsync(
                    doc.OrganizationId,
                    DateTime.Now,
                    ct);
                if (!reversalPeriodValidation.IsSuccess)
                    return reversalPeriodValidation;
            }

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto is not null)
                _auditLogService.SetOldValues(oldDocDto);

            var now = DateTime.Now;
            if (doc.StatusId == DocumentStatusIdConst.POSTED)
            {
                var activePostingBatch = await GetActivePostingBatchAsync(id, ct);
                if (activePostingBatch is null)
                {
                    return Result.Failure(
                        FaReceiptErrors.MissingPostingBatch(
                            id,
                            _userContext.LanguageId));
                }

                var reversalBatch = await CreatePostingBatchAsync(
                    doc,
                    PostingBatchStatusConst.REVERSAL,
                    "Fixed asset receipt cancelled",
                    ct);
                await _unitOfWork.SaveChangesAsync(ct);

                var accountingReverse = await ReverseAccountingEntriesAsync(
                    id,
                    reversalBatch.Id,
                    ct);
                if (!accountingReverse.IsSuccess)
                    return accountingReverse;

                activePostingBatch.Status = PostingBatchStatusConst.REVERSED;
                activePostingBatch.ReversedAt = now;
                activePostingBatch.ReversedByUserId = _userContext.Id;
                await _postingBatchCommand.UpdateAsync(activePostingBatch, ct);

                foreach (var asset in doc.Lines
                             .SelectMany(line => line.Assets)
                             .Select(receiptAsset => receiptAsset.FaAsset)
                             .Where(asset => asset is not null)
                             .DistinctBy(asset => asset!.Id))
                {
                    asset!.StateId = StateIdConst.PASSIVE;
                    asset.StatusId = FaAssetStatusIdConst.NOT_COMMISSIONED;
                    asset.DepartmentId = null;
                    asset.ResponsibleUserId = null;
                    asset.UpdatedDate = now;
                    await _faAssetCommand.UpdateAsync(asset, ct);
                }
            }

            doc.StatusId = DocumentStatusIdConst.CANCELLED;
            doc.CancelledAt ??= now;
            doc.CancelledByUserId ??= _userContext.Id;
            doc.UpdatedDate = now;

            await _command.UpdateAsync(doc, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto is not null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(
                    AuditLogTableConst.FaReceiptDoc,
                    id.ToString(),
                    AuditLogOperationTypeConst.Update,
                    "Cancelled");
            }

            return Result.Success();
        }, ct);

    private Result ValidateForConfirm(FaReceiptDoc doc)
    {
        if (doc.Lines.Count == 0)
            return Result.Failure(FaReceiptErrors.LinesRequired(_userContext.LanguageId));

        foreach (var line in doc.Lines)
        {
            if (line.Assets.Count == 0)
            {
                return Result.Failure(
                    FaReceiptErrors.AssetLinesRequired(line.Name, _userContext.LanguageId));
            }

            if (line.Quantity != line.Assets.Count)
            {
                return Result.Failure(
                    FaReceiptErrors.LineQuantityMismatch(
                        line.Name,
                        line.Quantity,
                        line.Assets.Count,
                        _userContext.LanguageId));
            }

            if (Math.Abs(line.Assets.Sum(asset => asset.InitialCost) - line.Amount) > 0.01m)
            {
                return Result.Failure(
                    FaReceiptErrors.LineAmountMismatch(
                        line.Name,
                        line.Amount,
                        line.Assets.Sum(asset => asset.InitialCost),
                        _userContext.LanguageId));
            }
        }

        return Result.Success();
    }

    private Task<Result> ValidateAccountsAsync(FaReceiptDoc doc, CancellationToken ct)
    {
        var requirements = new List<FaDocumentAccountRequirement>
        {
            new(
                doc.SupplierAccountId,
                FaDocumentAccountRoleCodeConst.SupplierSettlement)
        };

        foreach (var line in doc.Lines)
        {
            requirements.Add(new(
                line.CapitalInvestmentAccountId,
                FaDocumentAccountRoleCodeConst.CapitalInvestment));
            requirements.Add(new(
                line.VatAccountId,
                FaDocumentAccountRoleCodeConst.InputVat,
                line.VatAmount > 0m));
            requirements.AddRange(line.Assets.Select(asset =>
                new FaDocumentAccountRequirement(
                    asset.AssetAccountId,
                    FaDocumentAccountRoleCodeConst.FixedAsset)));
        }

        return _accountValidator.ValidateAsync(
            doc.OrganizationId,
            DocumentTypeIdConst.FARECEIPT,
            requirements,
            ct);
    }

    private async Task<Result> EnsureNoPostedDependenciesAsync(
        FaReceiptDoc doc,
        CancellationToken ct)
    {
        var assetIds = doc.Lines
            .SelectMany(line => line.Assets)
            .Where(asset => asset.FaAssetId.HasValue)
            .Select(asset => asset.FaAssetId!.Value)
            .Distinct()
            .ToList();

        if (assetIds.Count == 0)
            return Result.Success();

        var hasCommissioning = await _commissioningLineQuery.AnyAsync(line =>
            assetIds.Contains(line.FaAssetId) &&
            line.CommissioningDoc.StatusId == DocumentStatusIdConst.POSTED, ct);
        var hasMovement = await _movementLineQuery.AnyAsync(line =>
            assetIds.Contains(line.FaAssetId) &&
            line.MovementDoc.StatusId == DocumentStatusIdConst.POSTED, ct);
        var hasDepreciation = await _depreciationLineQuery.AnyAsync(line =>
            assetIds.Contains(line.FaAssetId) &&
            line.DepreciationRun.StatusId == DocumentStatusIdConst.POSTED, ct);
        var hasRevaluation = await _revaluationLineQuery.AnyAsync(line =>
            assetIds.Contains(line.FaAssetId) &&
            line.RevaluationDoc.StatusId == DocumentStatusIdConst.POSTED, ct);
        var hasDisposal = await _disposalLineQuery.AnyAsync(line =>
            assetIds.Contains(line.FaAssetId) &&
            line.DisposalDoc.StatusId == DocumentStatusIdConst.POSTED, ct);

        if (!hasCommissioning && !hasMovement && !hasDepreciation && !hasRevaluation && !hasDisposal)
            return Result.Success();

        return Result.Failure(FaReceiptErrors.PostedDependenciesExist(_userContext.LanguageId));
    }

    private async Task<FaReceiptDoc?> GetAggregateAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaReceiptDoc>()
            .Where(receipt => receipt.Id == id)
            .Build();
        query.AddIncludes(include =>
            include.Include(receipt => receipt.Lines)
                .ThenInclude(line => line.Assets)
                .ThenInclude(receiptAsset => receiptAsset.FaAsset)
                .ThenInclude(asset => asset!.FaAssetAccounting));
        return await _query.GetAsync(query, ct);
    }

    private async Task<FaReceiptDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaReceiptDoc>()
            .Where(receipt => receipt.Id == id)
            .As<FaReceiptDto>()
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<PostingBatch> CreatePostingBatchAsync(
        FaReceiptDoc doc,
        string status,
        string comment,
        CancellationToken ct)
    {
        var batch = new PostingBatch
        {
            OrganizationId = doc.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.FARECEIPT,
            DocumentId = doc.Id,
            Status = status,
            PostedByUserId = _userContext.Id,
            PostedAt = DateTime.Now,
            Comment = comment
        };

        await _postingBatchCommand.CreateAsync(batch, ct);
        return batch;
    }

    private async Task<PostingBatch?> GetActivePostingBatchAsync(
        long faReceiptDocId,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<PostingBatch>()
            .Where(batch =>
                batch.DocumentTypeId == DocumentTypeIdConst.FARECEIPT &&
                batch.DocumentId == faReceiptDocId &&
                batch.Status == PostingBatchStatusConst.POSTED)
            .Build();

        return await _postingBatchQuery.GetAsync(query, ct);
    }

    private async Task<Result> ReverseAccountingEntriesAsync(
        long faReceiptDocId,
        long reversalBatchId,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<AccountingRegisterEntry>()
            .Where(entry =>
                entry.DocumentTypeId == DocumentTypeIdConst.FARECEIPT &&
                entry.DocumentId == faReceiptDocId &&
                entry.ReversalEntryId == null)
            .Build();
        query.AddIncludes(include =>
            include.Include(entry => entry.RegisterEntrySubkontos));

        var entries = await _accountingRegisterQuery.GetAllAsync(query, ct);
        if (entries.Count == 0)
            return Result.Success();

        var reversalEntries = AccountingRegisterEntryReversalFactory.Create(
            entries,
            reversalBatchId);

        await _accountingRegisterCommand.CreateAsync(reversalEntries, ct);
        return Result.Success();
    }
}
