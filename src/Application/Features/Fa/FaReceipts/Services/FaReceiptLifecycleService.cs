using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
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
    private readonly IQueryRepository<FaReceiptDoc> _query;
    private readonly IFaReceiptCommandRepository _command;
    private readonly IFaAssetCommandRepository _faAssetCommand;
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
        IQueryRepository<FaReceiptDoc> query,
        IFaReceiptCommandRepository command,
        IFaAssetCommandRepository faAssetCommand,
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
        _query = query;
        _command = command;
        _faAssetCommand = faAssetCommand;
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
                return Result.Success();

            if (doc.StatusId != DocumentStatusIdConst.DRAFT && doc.StatusId != DocumentStatusIdConst.PENDING)
                return Result.Failure(FaReceiptErrors.CannotConfirmInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var periodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, doc.DocDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var businessValidation = ValidateForConfirm(doc);
            if (!businessValidation.IsSuccess)
                return businessValidation;

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto is not null)
                _auditLogService.SetOldValues(oldDocDto);

            var now = DateTime.Now;
            foreach (var receiptAsset in doc.Lines.SelectMany(x => x.Assets))
            {
                var asset = receiptAsset.FaAsset;
                if (asset is null)
                {
                    asset = new FaAsset
                    {
                        OrganizationId = doc.OrganizationId,
                        StateId = StateIdConst.ACTIVE,
                        InventoryNumber = receiptAsset.InventoryNumber,
                        Name = receiptAsset.Name,
                        FaGroupId = receiptAsset.FaGroupId,
                        OkofId = receiptAsset.OkofId,
                        DepreciationMethodId = receiptAsset.DepreciationMethodId,
                        UsefulLifeMonths = receiptAsset.UsefulLifeMonths,
                        InitialCost = receiptAsset.InitialCost,
                        SalvageValue = receiptAsset.SalvageValue,
                        CommissioningDate = NormalizeDateTime(receiptAsset.CommissioningDate ?? doc.DocDate),
                        DeprStartDate = NormalizeDateTime(receiptAsset.DeprStartDate ?? receiptAsset.CommissioningDate ?? doc.DocDate),
                        PlannedUnitsTotal = receiptAsset.PlannedUnitsTotal,
                        DepartmentId = receiptAsset.DepartmentId,
                        ResponsibleUserId = receiptAsset.ResponsibleUserId,
                        AssetAccountId = receiptAsset.AssetAccountId,
                        AccumulatedDepreciationAccountId = receiptAsset.AccumulatedDepreciationAccountId,
                        DepreciationExpenseAccountId = receiptAsset.DepreciationExpenseAccountId,
                        StatusId = FaAssetStatusIdConst.ACTIVE,
                        CreatedDate = now,
                        UpdatedDate = now
                    };

                    await _faAssetCommand.CreateAsync(asset, ct);
                    receiptAsset.FaAsset = asset;
                    receiptAsset.FaAssetId = asset.Id;
                }
                else
                {
                    asset.StateId = StateIdConst.ACTIVE;
                    asset.InventoryNumber = receiptAsset.InventoryNumber;
                    asset.Name = receiptAsset.Name;
                    asset.FaGroupId = receiptAsset.FaGroupId;
                    asset.OkofId = receiptAsset.OkofId;
                    asset.DepreciationMethodId = receiptAsset.DepreciationMethodId;
                    asset.UsefulLifeMonths = receiptAsset.UsefulLifeMonths;
                    asset.InitialCost = receiptAsset.InitialCost;
                    asset.SalvageValue = receiptAsset.SalvageValue;
                    asset.CommissioningDate = NormalizeDateTime(receiptAsset.CommissioningDate ?? doc.DocDate);
                    asset.DeprStartDate = NormalizeDateTime(receiptAsset.DeprStartDate ?? receiptAsset.CommissioningDate ?? doc.DocDate);
                    asset.PlannedUnitsTotal = receiptAsset.PlannedUnitsTotal;
                    asset.DepartmentId = receiptAsset.DepartmentId;
                    asset.ResponsibleUserId = receiptAsset.ResponsibleUserId;
                    asset.AssetAccountId = receiptAsset.AssetAccountId;
                    asset.AccumulatedDepreciationAccountId = receiptAsset.AccumulatedDepreciationAccountId;
                    asset.DepreciationExpenseAccountId = receiptAsset.DepreciationExpenseAccountId;
                    asset.StatusId = FaAssetStatusIdConst.ACTIVE;
                    asset.UpdatedDate = now;

                    await _faAssetCommand.UpdateAsync(asset, ct);
                }
            }

            // FA-P4: бухгалтерские проводки прихода ОС.\r\n            var postingBatch = await CreatePostingBatchAsync(doc, PostingBatchStatusConst.POSTED, "Fixed asset receipt confirmed", ct);
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
                await _auditLogService.CreateAsync(AuditLogTableConst.FaReceiptDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Confirmed");
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

            if (doc.StatusId != DocumentStatusIdConst.DRAFT &&
                doc.StatusId != DocumentStatusIdConst.PENDING &&
                doc.StatusId != DocumentStatusIdConst.POSTED)
            {
                return Result.Failure(FaReceiptErrors.CannotCancelInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));
            }

            var periodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, doc.DocDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
            {
                var reversalPeriodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, DateTime.Now, ct);
                if (!reversalPeriodValidation.IsSuccess)
                    return reversalPeriodValidation;
            }


            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto is not null)
                _auditLogService.SetOldValues(oldDocDto);

            var now = DateTime.Now;
            if (doc.StatusId == DocumentStatusIdConst.POSTED)
            {
                // FA-P4: сторнируем бухгалтерские проводки прихода ОС (reversal-проводки).
                var activePostingBatch = await GetActivePostingBatchAsync(id, ct);
                if (activePostingBatch is not null)
                {
                    var reversalBatch = await CreatePostingBatchAsync(doc, PostingBatchStatusConst.REVERSAL, "Fixed asset receipt cancelled", ct);

                    var accountingReverse = await ReverseAccountingEntriesAsync(id, reversalBatch.Id, ct);
                    if (!accountingReverse.IsSuccess)
                        return accountingReverse;

                    activePostingBatch.Status = PostingBatchStatusConst.REVERSED;
                    activePostingBatch.ReversedAt = now;
                    activePostingBatch.ReversedByUserId = _userContext.Id;
                    await _postingBatchCommand.UpdateAsync(activePostingBatch, ct);
                }

                foreach (var asset in doc.Lines.SelectMany(x => x.Assets).Select(x => x.FaAsset).Where(x => x is not null))
                {
                    asset!.StateId = StateIdConst.PASSIVE;
                    asset.StatusId = FaAssetStatusIdConst.NOT_COMMISSIONED;
                    asset.CommissioningDate = null;
                    asset.DeprStartDate = null;
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
                await _auditLogService.CreateAsync(AuditLogTableConst.FaReceiptDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Cancelled");
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
                return Result.Failure(FaReceiptErrors.AssetLinesRequired(line.Name, _userContext.LanguageId));

            if (line.Quantity != decimal.Truncate(line.Quantity) || line.Quantity != line.Assets.Count)
            {
                return Result.Failure(
                    FaReceiptErrors.LineQuantityMismatch(line.Name, line.Quantity, line.Assets.Count, _userContext.LanguageId));
            }
        }

        return Result.Success();
    }

    private async Task<FaReceiptDoc?> GetAggregateAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaReceiptDoc>().Where(x => x.Id == id).Build();
        query.AddIncludes(x => x.Include(d => d.Lines).ThenInclude(l => l.Assets).ThenInclude(a => a.FaAsset));
        return await _query.GetAsync(query, ct);
    }

    private async Task<FaReceiptDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaReceiptDoc>().Where(x => x.Id == id).As<FaReceiptDto>().Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<PostingBatch> CreatePostingBatchAsync(FaReceiptDoc doc, string status, string comment, CancellationToken ct)
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

    private async Task<PostingBatch?> GetActivePostingBatchAsync(long faReceiptDocId, CancellationToken ct)
    {
        var query = _queryBuilder.For<PostingBatch>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.FARECEIPT &&
                        x.DocumentId == faReceiptDocId &&
                        x.Status == PostingBatchStatusConst.POSTED)
            .Build();

        return await _postingBatchQuery.GetAsync(query, ct);
    }

    private async Task<Result> ReverseAccountingEntriesAsync(long faReceiptDocId, long reversalBatchId, CancellationToken ct)
    {
        var query = _queryBuilder.For<AccountingRegisterEntry>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.FARECEIPT &&
                        x.DocumentId == faReceiptDocId &&
                        x.ReversalEntryId == null)
            .Build();
        query.AddIncludes(b => b.Include(x => x.RegisterEntrySubkontos));

        var entries = await _accountingRegisterQuery.GetAllAsync(query, ct);
        if (entries.Count == 0)
            return Result.Success();

        var now = DateTime.Now;
        var reversalEntries = entries.Select(entry => new AccountingRegisterEntry
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

        await _accountingRegisterCommand.CreateAsync(reversalEntries, ct);
        return Result.Success();
    }

    private static string ReverseSubkontoSide(string side) =>
        side == SubkontoSideConst.DEBIT ? SubkontoSideConst.CREDIT :
        side == SubkontoSideConst.CREDIT ? SubkontoSideConst.DEBIT :
        side;

    private static DateTime NormalizeDateTime(DateTime value) =>
        DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
}
