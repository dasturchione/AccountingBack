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

namespace Application.Features.FaCommissionings;

public class FaCommissioningLifecycleService :
    BaseService,
    IFaCommissioningLifecycleService
{
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAccountingPeriodValidator _periodValidator;
    private readonly IAuditLogService _auditLogService;
    private readonly IAccountingDispatcher _dispatcher;
    private readonly IFaDocumentAccountValidator _accountValidator;
    private readonly IQueryRepository<FaCommissioningDoc> _query;
    private readonly IFaCommissioningCommandRepository _command;
    private readonly IFaAssetCommandRepository _assetCommand;
    private readonly ICommandRepository<FaAssetAccounting> _assetAccountingCommand;
    private readonly IQueryRepository<FaMovementDocLine> _movementLineQuery;
    private readonly IQueryRepository<FaDepreciationRunLine> _depreciationLineQuery;
    private readonly IQueryRepository<FaRevaluationDocLine> _revaluationLineQuery;
    private readonly IQueryRepository<FaDisposalDocLine> _disposalLineQuery;
    private readonly IQueryRepository<PostingBatch> _postingBatchQuery;
    private readonly ICommandRepository<PostingBatch> _postingBatchCommand;
    private readonly IQueryRepository<AccountingRegisterEntry> _accountingRegisterQuery;
    private readonly ICommandRepository<AccountingRegisterEntry> _accountingRegisterCommand;

    public FaCommissioningLifecycleService(
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IQueryBuilder queryBuilder,
        IDocumentPostingLock postingLock,
        IAccountingPeriodValidator periodValidator,
        IAuditLogService auditLogService,
        IAccountingDispatcher dispatcher,
        IFaDocumentAccountValidator accountValidator,
        IQueryRepository<FaCommissioningDoc> query,
        IFaCommissioningCommandRepository command,
        IFaAssetCommandRepository assetCommand,
        ICommandRepository<FaAssetAccounting> assetAccountingCommand,
        IQueryRepository<FaMovementDocLine> movementLineQuery,
        IQueryRepository<FaDepreciationRunLine> depreciationLineQuery,
        IQueryRepository<FaRevaluationDocLine> revaluationLineQuery,
        IQueryRepository<FaDisposalDocLine> disposalLineQuery,
        IQueryRepository<PostingBatch> postingBatchQuery,
        ICommandRepository<PostingBatch> postingBatchCommand,
        IQueryRepository<AccountingRegisterEntry> accountingRegisterQuery,
        ICommandRepository<AccountingRegisterEntry> accountingRegisterCommand,
        ILogger<FaCommissioningLifecycleService> logger)
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
        _assetCommand = assetCommand;
        _assetAccountingCommand = assetAccountingCommand;
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
                return Result.Failure(
                    CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(
                DocumentTypeIdConst.FACOMMISSIONING,
                id,
                ct);

            var document = await GetAggregateAsync(id, ct);
            if (document is null)
            {
                return Result.Failure(
                    FaCommissioningErrors.NotFound(id, _userContext.LanguageId));
            }

            if (document.StatusId == DocumentStatusIdConst.POSTED)
            {
                var existingBatch = await GetActivePostingBatchAsync(id, ct);
                return existingBatch is not null
                    ? Result.Success()
                    : Result.Failure(
                        FaCommissioningErrors.MissingPostingBatch(
                            id,
                            _userContext.LanguageId));
            }

            if (document.StatusId is not (
                    DocumentStatusIdConst.DRAFT or
                    DocumentStatusIdConst.PENDING))
            {
                return Result.Failure(
                    FaCommissioningErrors.CannotConfirm(
                        id,
                        document.StatusId,
                        _userContext.LanguageId));
            }

            var periodValidation = await _periodValidator.EnsureOpenAsync(
                document.OrganizationId,
                document.DocDate,
                ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var businessValidation = ValidateForConfirm(document);
            if (!businessValidation.IsSuccess)
                return businessValidation;

            var accountValidation = await ValidateAccountsAsync(document, ct);
            if (!accountValidation.IsSuccess)
                return accountValidation;

            var oldDto = await GetByIdInternalAsync(id, ct);
            if (oldDto is not null)
                _auditLogService.SetOldValues(oldDto);

            var now = DateTime.Now;
            foreach (var line in document.Lines)
            {
                var asset = line.FaAsset;
                var accounting = asset.FaAssetAccounting!;

                asset.StatusId = FaAssetStatusIdConst.ACTIVE;
                asset.DepartmentId = line.DepartmentId;
                asset.ResponsibleUserId = line.ResponsibleUserId;
                asset.UpdatedDate = now;

                accounting.SalvageValue = line.SalvageValue;
                accounting.DepreciationMethodId = line.DepreciationMethodId;
                accounting.UsefulLifeMonths = line.UsefulLifeMonths;
                accounting.DeprStartDate = line.DeprStartDate;
                accounting.PlannedUnitsTotal = line.PlannedUnitsTotal;
                accounting.AccumulatedDepreciationAccountId =
                    line.AccumulatedDepreciationAccountId;
                accounting.DepreciationExpenseAccountId =
                    line.DepreciationExpenseAccountId;
                accounting.UpdatedDate = now;

                await _assetCommand.UpdateAsync(asset, ct);
                await _assetAccountingCommand.UpdateAsync(accounting, ct);
            }

            await _unitOfWork.SaveChangesAsync(ct);

            var postingBatch = CreatePostingBatch(
                document,
                PostingBatchStatusConst.POSTED,
                "Fixed asset commissioning confirmed",
                now);
            await _postingBatchCommand.CreateAsync(postingBatch, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var postingResult = await _dispatcher.ProcessAsync(
                document,
                ct,
                postingBatch.Id);
            if (!postingResult.IsSuccess)
                return postingResult;

            document.StatusId = DocumentStatusIdConst.POSTED;
            document.PostedAt ??= now;
            document.PostedByUserId ??= _userContext.Id;
            document.UpdatedDate = now;
            document.UpdatedByUserId = _userContext.Id;

            await _command.UpdateAsync(document, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var newDto = await GetByIdInternalAsync(id, ct);
            if (newDto is not null)
            {
                _auditLogService.SetNewValues(newDto);
                await _auditLogService.CreateAsync(
                    AuditLogTableConst.FaCommissioningDoc,
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
                return Result.Failure(
                    CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(
                DocumentTypeIdConst.FACOMMISSIONING,
                id,
                ct);

            var document = await GetAggregateAsync(id, ct);
            if (document is null)
            {
                return Result.Failure(
                    FaCommissioningErrors.NotFound(id, _userContext.LanguageId));
            }

            if (document.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();

            if (document.StatusId is not (
                    DocumentStatusIdConst.DRAFT or
                    DocumentStatusIdConst.PENDING or
                    DocumentStatusIdConst.POSTED))
            {
                return Result.Failure(
                    FaCommissioningErrors.CannotCancel(
                        id,
                        document.StatusId,
                        _userContext.LanguageId));
            }

            var periodValidation = await _periodValidator.EnsureOpenAsync(
                document.OrganizationId,
                document.DocDate,
                ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            if (document.StatusId == DocumentStatusIdConst.POSTED)
            {
                var dependencyValidation = await EnsureNoPostedDependenciesAsync(
                    document,
                    ct);
                if (!dependencyValidation.IsSuccess)
                    return dependencyValidation;

                var reversalPeriodValidation =
                    await _periodValidator.EnsureOpenAsync(
                        document.OrganizationId,
                        DateTime.Now,
                        ct);
                if (!reversalPeriodValidation.IsSuccess)
                    return reversalPeriodValidation;
            }

            var oldDto = await GetByIdInternalAsync(id, ct);
            if (oldDto is not null)
                _auditLogService.SetOldValues(oldDto);

            var now = DateTime.Now;
            if (document.StatusId == DocumentStatusIdConst.POSTED)
            {
                var activePostingBatch = await GetActivePostingBatchAsync(id, ct);
                if (activePostingBatch is null)
                {
                    return Result.Failure(
                        FaCommissioningErrors.MissingPostingBatch(
                            id,
                            _userContext.LanguageId));
                }

                var reversalBatch = CreatePostingBatch(
                    document,
                    PostingBatchStatusConst.REVERSAL,
                    "Fixed asset commissioning cancelled",
                    now);
                await _postingBatchCommand.CreateAsync(reversalBatch, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                var reverseResult = await ReverseAccountingEntriesAsync(
                    id,
                    reversalBatch.Id,
                    ct);
                if (!reverseResult.IsSuccess)
                    return reverseResult;

                activePostingBatch.Status = PostingBatchStatusConst.REVERSED;
                activePostingBatch.ReversedAt = now;
                activePostingBatch.ReversedByUserId = _userContext.Id;
                await _postingBatchCommand.UpdateAsync(activePostingBatch, ct);

                foreach (var line in document.Lines)
                {
                    var asset = line.FaAsset;
                    var accounting = asset.FaAssetAccounting!;

                    asset.StatusId = FaAssetStatusIdConst.NOT_COMMISSIONED;
                    asset.DepartmentId = null;
                    asset.ResponsibleUserId = null;
                    asset.UpdatedDate = now;

                    accounting.SalvageValue = null;
                    accounting.DepreciationMethodId = null;
                    accounting.UsefulLifeMonths = null;
                    accounting.DeprStartDate = null;
                    accounting.PlannedUnitsTotal = null;
                    accounting.AccumulatedDepreciationAccountId = null;
                    accounting.DepreciationExpenseAccountId = null;
                    accounting.UpdatedDate = now;

                    await _assetCommand.UpdateAsync(asset, ct);
                    await _assetAccountingCommand.UpdateAsync(accounting, ct);
                }
            }

            document.StatusId = DocumentStatusIdConst.CANCELLED;
            document.CancelledAt ??= now;
            document.CancelledByUserId ??= _userContext.Id;
            document.UpdatedDate = now;
            document.UpdatedByUserId = _userContext.Id;

            await _command.UpdateAsync(document, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var newDto = await GetByIdInternalAsync(id, ct);
            if (newDto is not null)
            {
                _auditLogService.SetNewValues(newDto);
                await _auditLogService.CreateAsync(
                    AuditLogTableConst.FaCommissioningDoc,
                    id.ToString(),
                    AuditLogOperationTypeConst.Update,
                    "Cancelled");
            }

            return Result.Success();
        }, ct);

    private Result ValidateForConfirm(FaCommissioningDoc document)
    {
        if (document.Lines.Count == 0)
        {
            return Result.Failure(
                FaCommissioningErrors.LinesRequired(_userContext.LanguageId));
        }

        foreach (var line in document.Lines)
        {
            if (line.FaAsset.StateId != StateIdConst.ACTIVE ||
                line.FaAsset.StatusId != FaAssetStatusIdConst.NOT_COMMISSIONED)
            {
                return Result.Failure(
                    FaCommissioningErrors.AssetUnavailable(
                        line.FaAssetId,
                        _userContext.LanguageId));
            }

            if (line.FaAsset.FaAssetAccounting is null)
            {
                return Result.Failure(
                    FaCommissioningErrors.AccountingNotFound(
                        line.FaAssetId,
                        _userContext.LanguageId));
            }

            if (line.SalvageValue >
                line.FaAsset.FaAssetAccounting.InitialCost)
            {
                return Result.Failure(
                    FaCommissioningErrors.SalvageValueTooHigh(
                        line.FaAssetId,
                        _userContext.LanguageId));
            }
        }

        return Result.Success();
    }

    private Task<Result> ValidateAccountsAsync(
        FaCommissioningDoc document,
        CancellationToken ct)
    {
        var requirements = document.Lines
            .SelectMany(line => new[]
            {
                new FaDocumentAccountRequirement(
                    line.CapitalInvestmentAccountId,
                    FaDocumentAccountRoleCodeConst.CapitalInvestment),
                new FaDocumentAccountRequirement(
                    line.FaAsset.FaAssetAccounting?.AssetAccountId,
                    FaDocumentAccountRoleCodeConst.FixedAsset),
                new FaDocumentAccountRequirement(
                    line.AccumulatedDepreciationAccountId,
                    FaDocumentAccountRoleCodeConst.AccumulatedDepreciation),
                new FaDocumentAccountRequirement(
                    line.DepreciationExpenseAccountId,
                    FaDocumentAccountRoleCodeConst.DepreciationExpense)
            })
            .ToList();

        return _accountValidator.ValidateAsync(
            document.OrganizationId,
            DocumentTypeIdConst.FACOMMISSIONING,
            requirements,
            ct);
    }

    private async Task<Result> EnsureNoPostedDependenciesAsync(
        FaCommissioningDoc document,
        CancellationToken ct)
    {
        var assetIds = document.Lines
            .Select(line => line.FaAssetId)
            .Distinct()
            .ToList();

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

        return hasMovement || hasDepreciation || hasRevaluation || hasDisposal
            ? Result.Failure(
                FaCommissioningErrors.PostedDependenciesExist(
                    _userContext.LanguageId))
            : Result.Success();
    }

    private async Task<FaCommissioningDoc?> GetAggregateAsync(
        long id,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<FaCommissioningDoc>()
            .Where(document => document.Id == id)
            .Build();
        query.AddIncludes(include =>
            include.Include(document => document.Lines)
                .ThenInclude(line => line.FaAsset)
                .ThenInclude(asset => asset.FaAssetAccounting));
        return await _query.GetAsync(query, ct);
    }

    private async Task<FaCommissioningDto?> GetByIdInternalAsync(
        long id,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<FaCommissioningDoc>()
            .Where(document => document.Id == id)
            .As<FaCommissioningDto>()
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private PostingBatch CreatePostingBatch(
        FaCommissioningDoc document,
        string status,
        string comment,
        DateTime now) =>
        new()
        {
            OrganizationId = document.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.FACOMMISSIONING,
            DocumentId = document.Id,
            Status = status,
            PostedByUserId = _userContext.Id,
            PostedAt = now,
            Comment = comment
        };

    private async Task<PostingBatch?> GetActivePostingBatchAsync(
        long documentId,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<PostingBatch>()
            .Where(batch =>
                batch.DocumentTypeId == DocumentTypeIdConst.FACOMMISSIONING &&
                batch.DocumentId == documentId &&
                batch.Status == PostingBatchStatusConst.POSTED)
            .Build();
        return await _postingBatchQuery.GetAsync(query, ct);
    }

    private async Task<Result> ReverseAccountingEntriesAsync(
        long documentId,
        long reversalBatchId,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<AccountingRegisterEntry>()
            .Where(entry =>
                entry.DocumentTypeId == DocumentTypeIdConst.FACOMMISSIONING &&
                entry.DocumentId == documentId &&
                entry.ReversalEntryId == null)
            .Build();
        query.AddIncludes(include =>
            include.Include(entry => entry.RegisterEntrySubkontos));

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
            RegisterEntrySubkontos = entry.RegisterEntrySubkontos.Select(
                subkonto => new RegisterEntrySubkonto
                {
                    Side = subkonto.Side == SubkontoSideConst.DEBIT
                        ? SubkontoSideConst.CREDIT
                        : SubkontoSideConst.DEBIT,
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

