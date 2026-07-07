using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.Register.AccountingRegisterEntries;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;
using System.Globalization;

namespace Application.Features.FaDepreciations;

public class FaDepreciationRunService : BaseService, IFaDepreciationRunService
{
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IDocNumberGenerator _docNumberGenerator;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAccountingPeriodValidator _periodValidator;
    private readonly IAccountingDispatcher _dispatcher;
    private readonly IQueryRepository<FaDepreciationRun> _query;
    private readonly IQueryRepository<FaAsset> _faAssetQuery;
    private readonly IQueryRepository<FaDepreciationRunLine> _runLineQuery;
    private readonly IFaDepreciationRunCommandRepository _command;
    private readonly IQueryRepository<PostingBatch> _postingBatchQuery;
    private readonly ICommandRepository<PostingBatch> _postingBatchCommand;
    private readonly IQueryRepository<AccountingRegisterEntry> _accountingRegisterQuery;
    private readonly ICommandRepository<AccountingRegisterEntry> _accountingRegisterCommand;

    public FaDepreciationRunService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IDocNumberGenerator docNumberGenerator,
        IDocumentPostingLock postingLock,
        IAccountingPeriodValidator periodValidator,
        IAccountingDispatcher dispatcher,
        IQueryRepository<FaDepreciationRun> query,
        IQueryRepository<FaAsset> faAssetQuery,
        IQueryRepository<FaDepreciationRunLine> runLineQuery,
        IFaDepreciationRunCommandRepository command,
        IQueryRepository<PostingBatch> postingBatchQuery,
        ICommandRepository<PostingBatch> postingBatchCommand,
        IQueryRepository<AccountingRegisterEntry> accountingRegisterQuery,
        ICommandRepository<AccountingRegisterEntry> accountingRegisterCommand,
        ILogger<FaDepreciationRunService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _unitOfWork = unitOfWork;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _docNumberGenerator = docNumberGenerator;
        _postingLock = postingLock;
        _periodValidator = periodValidator;
        _dispatcher = dispatcher;
        _query = query;
        _faAssetQuery = faAssetQuery;
        _runLineQuery = runLineQuery;
        _command = command;
        _postingBatchQuery = postingBatchQuery;
        _postingBatchCommand = postingBatchCommand;
        _accountingRegisterQuery = accountingRegisterQuery;
        _accountingRegisterCommand = accountingRegisterCommand;
    }

    public Task<Result<PagedResponse<FaDepreciationRunListDto>>> GetAllAsync(FaDepreciationRunListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<FaDepreciationRun, FaDepreciationRunListDto, FaDepreciationRunListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<FaDepreciationRunDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var item = await GetByIdInternalAsync(id, ct);
            return item is null
                ? Result.Failure<FaDepreciationRunDto>(FaDepreciationErrors.NotFound(id, _userContext.LanguageId))
                : Result.Success(item);
        });

    public Task<Result<long>> RunAsync(string period, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(RunAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            if (!TryParsePeriod(period, out var periodMonth))
                return Result.Failure<long>(FaDepreciationErrors.InvalidPeriod(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            await _postingLock.AcquireAsync(DocumentTypeIdConst.FADEPRECIATION, periodMonth.Ticks, ct);

            var periodValidation = await _periodValidator.EnsureOpenAsync(organizationId, periodMonth, ct);
            if (!periodValidation.IsSuccess)
                return Result.Failure<long>(periodValidation.Error);

            if (await HasActiveRunAsync(organizationId, periodMonth, ct))
                return Result.Failure<long>(FaDepreciationErrors.AlreadyRun(periodMonth, _userContext.LanguageId));

            var linesResult = await BuildRunLinesAsync(organizationId, periodMonth, ct);
            if (!linesResult.IsSuccess)
                return Result.Failure<long>(linesResult.Error);

            var now = DateTime.Now;
            var doc = new FaDepreciationRun
            {
                OrganizationId = organizationId,
                StateId = StateIdConst.ACTIVE,
                DocNumber = await _docNumberGenerator.GenerateAsync(organizationId, "FAD", periodMonth, ct),
                PeriodMonth = periodMonth.Date,
                StatusId = DocumentStatusIdConst.POSTED,
                Note = $"Monthly depreciation for {periodMonth:yyyy-MM}",
                CreatedDate = now,
                CreatedByUserId = _userContext.Id,
                UpdatedDate = now,
                UpdatedByUserId = _userContext.Id,
                PostedAt = now,
                PostedByUserId = _userContext.Id,
                Lines = linesResult.Value
            };

            await _command.CreateAsync(doc, ct);
            await SaveChangesAsync(ct);

            var postingBatch = CreatePostingBatch(doc, PostingBatchStatusConst.POSTED, "Fixed asset depreciation run", now);
            await _postingBatchCommand.CreateAsync(postingBatch, ct);
            await SaveChangesAsync(ct);

            var postingResult = await _dispatcher.ProcessAsync(doc, ct, postingBatch.Id);
            if (!postingResult.IsSuccess)
                return Result.Failure<long>(postingResult.Error);

            doc.UpdatedDate = DateTime.Now;
            await _command.UpdateAsync(doc, ct);
            await SaveChangesAsync(ct);

            var dto = await GetByIdInternalAsync(doc.Id, ct);
            if (dto is not null)
            {
                _auditLogService.SetNewValues(dto);
                await _auditLogService.CreateAsync(AuditLogTableConst.FaDepreciationRun, doc.Id.ToString(), AuditLogOperationTypeConst.Create);
            }

            return Result.Success(doc.Id);
        }, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.FADEPRECIATION, id, ct);

            var doc = await GetAggregateAsync(id, ct);
            if (doc is null)
                return Result.Failure(FaDepreciationErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();

            if (doc.StatusId != DocumentStatusIdConst.POSTED)
                return Result.Failure(FaDepreciationErrors.CannotCancelInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var periodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, doc.PeriodMonth, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var reversalPeriodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, DateTime.Now, ct);
            if (!reversalPeriodValidation.IsSuccess)
                return reversalPeriodValidation;

            var oldDto = await GetByIdInternalAsync(id, ct);
            if (oldDto is not null)
                _auditLogService.SetOldValues(oldDto);

            var activePostingBatch = await GetActivePostingBatchAsync(id, ct);
            if (activePostingBatch == null)
                return Result.Failure(FaDepreciationErrors.MissingPostingBatch(id, _userContext.LanguageId));

            var now = DateTime.Now;
            var reversalBatch = CreatePostingBatch(doc, PostingBatchStatusConst.REVERSAL, "Fixed asset depreciation cancelled", now);
            await _postingBatchCommand.CreateAsync(reversalBatch, ct);
            await SaveChangesAsync(ct);

            var reverseResult = await ReverseAccountingEntriesAsync(id, reversalBatch.Id, ct);
            if (!reverseResult.IsSuccess)
                return reverseResult;

            activePostingBatch.Status = PostingBatchStatusConst.REVERSED;
            activePostingBatch.ReversedAt = now;
            activePostingBatch.ReversedByUserId = _userContext.Id;
            await _postingBatchCommand.UpdateAsync(activePostingBatch, ct);

            doc.StatusId = DocumentStatusIdConst.CANCELLED;
            doc.CancelledAt = now;
            doc.CancelledByUserId = _userContext.Id;
            doc.UpdatedDate = now;
            doc.UpdatedByUserId = _userContext.Id;
            await _command.UpdateAsync(doc, ct);
            await SaveChangesAsync(ct);

            var newDto = await GetByIdInternalAsync(id, ct);
            if (newDto is not null)
            {
                _auditLogService.SetNewValues(newDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.FaDepreciationRun, id.ToString(), AuditLogOperationTypeConst.Update, "Cancelled");
            }

            return Result.Success();
        }, ct);

    private async Task<Result<List<FaDepreciationRunLine>>> BuildRunLinesAsync(int organizationId, DateTime periodMonth, CancellationToken ct)
    {
        var periodEnd = periodMonth.AddMonths(1).AddDays(-1);

        var assetQuery = _queryBuilder.For<FaAsset>()
            .Where(x => x.OrganizationId == organizationId &&
                        x.StateId == StateIdConst.ACTIVE &&
                        x.StatusId == FaAssetStatusIdConst.ACTIVE &&
                        x.DeprStartDate.HasValue &&
                        x.DeprStartDate.Value <= periodEnd)
            .Build();
        assetQuery.AddIncludes(x => x.Include(a => a.DepreciationMethod));

        var assets = await _faAssetQuery.GetAllAsync(assetQuery, ct);
        if (assets.Count == 0)
            return Result.Failure<List<FaDepreciationRunLine>>(FaDepreciationErrors.NoDepreciationLines(periodMonth, _userContext.LanguageId));

        var assetIds = assets.Select(x => x.Id).ToList();
        var previousLineQuery = _queryBuilder.For<FaDepreciationRunLine>()
            .Where(x => assetIds.Contains(x.FaAssetId) &&
                        x.DepreciationRun.OrganizationId == organizationId &&
                        x.DepreciationRun.StatusId == DocumentStatusIdConst.POSTED &&
                        x.DepreciationRun.PeriodMonth < periodMonth)
            .Build();
        var previousLines = await _runLineQuery.GetAllAsync(previousLineQuery, ct);

        var totalsByAsset = previousLines
            .GroupBy(x => x.FaAssetId)
            .ToDictionary(x => x.Key, x => x.Sum(y => y.Amount));

        var result = new List<FaDepreciationRunLine>();

        foreach (var asset in assets)
        {
            var startMonth = StartOfMonth(asset.DeprStartDate!.Value);
            if (periodMonth < startMonth)
                continue;

            var elapsedMonths = MonthsBetween(startMonth, periodMonth) + 1;
            if (elapsedMonths <= 0 || elapsedMonths > asset.UsefulLifeMonths)
                continue;

            var previousAmount = totalsByAsset.GetValueOrDefault(asset.Id);
            var amount = CalculateAmount(asset, elapsedMonths, previousAmount);
            if (amount <= 0m)
                continue;

            result.Add(new FaDepreciationRunLine
            {
                FaAssetId = asset.Id,
                Amount = amount,
                Note = asset.DepreciationMethod.Code == FaDepreciationMethodCodeConst.UNITS_OF_PRODUCTION
                    ? "Units-of-production fallback: equal monthly amount"
                    : null
            });
        }

        if (result.Count == 0)
            return Result.Failure<List<FaDepreciationRunLine>>(FaDepreciationErrors.NoDepreciationLines(periodMonth, _userContext.LanguageId));

        return Result.Success(result);
    }

    private static decimal CalculateAmount(FaAsset asset, int elapsedMonths, decimal previousAmount)
    {
        var depreciableBase = Math.Max(0m, asset.InitialCost - asset.SalvageValue);
        var remainingDepreciable = Math.Max(0m, depreciableBase - previousAmount);
        if (remainingDepreciable <= 0m)
            return 0m;

        var isFinalMonth = elapsedMonths >= asset.UsefulLifeMonths;

        return asset.DepreciationMethod.Code switch
        {
            FaDepreciationMethodCodeConst.DECLINING_BALANCE => CalculateDecliningBalanceAmount(asset, remainingDepreciable, isFinalMonth),
            FaDepreciationMethodCodeConst.UNITS_OF_PRODUCTION => CalculateLinearLikeAmount(asset.UsefulLifeMonths, depreciableBase, previousAmount, isFinalMonth),
            _ => CalculateLinearLikeAmount(asset.UsefulLifeMonths, depreciableBase, previousAmount, isFinalMonth)
        };
    }

    private static decimal CalculateDecliningBalanceAmount(FaAsset asset, decimal remainingDepreciable, bool isFinalMonth)
    {
        if (isFinalMonth)
            return RoundAmount(remainingDepreciable);

        var monthlyRate = 2m / asset.UsefulLifeMonths;
        var remainingBookValue = asset.InitialCost - (asset.InitialCost - asset.SalvageValue - remainingDepreciable);
        var candidate = RoundAmount(remainingBookValue * monthlyRate);
        if (candidate <= 0m)
            candidate = RoundAmount(remainingDepreciable);

        return Math.Min(candidate, RoundAmount(remainingDepreciable));
    }

    private static decimal CalculateLinearLikeAmount(int usefulLifeMonths, decimal depreciableBase, decimal previousAmount, bool isFinalMonth)
    {
        if (isFinalMonth)
            return RoundAmount(depreciableBase - previousAmount);

        var monthly = RoundAmount(depreciableBase / usefulLifeMonths);
        var remaining = RoundAmount(depreciableBase - previousAmount);
        return Math.Min(monthly, remaining);
    }

    private async Task<bool> HasActiveRunAsync(int organizationId, DateTime periodMonth, CancellationToken ct)
        => await _query.AnyAsync(x => x.OrganizationId == organizationId &&
                                      x.PeriodMonth == periodMonth &&
                                      x.StatusId != DocumentStatusIdConst.CANCELLED, ct);

    private async Task<FaDepreciationRun?> GetAggregateAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaDepreciationRun>().Where(x => x.Id == id).Build();
        query.AddIncludes(x => x.Include(d => d.Lines).ThenInclude(l => l.FaAsset).ThenInclude(a => a.DepreciationMethod));
        return await _query.GetAsync(query, ct);
    }

    private async Task<FaDepreciationRunDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaDepreciationRun>().Where(x => x.Id == id).As<FaDepreciationRunDto>().Build();
        return await _query.GetAsync(query, ct);
    }

    private PostingBatch CreatePostingBatch(FaDepreciationRun doc, string status, string comment, DateTime now) =>
        new()
        {
            OrganizationId = doc.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.FADEPRECIATION,
            DocumentId = doc.Id,
            Status = status,
            PostedByUserId = _userContext.Id,
            PostedAt = now,
            Comment = comment
        };

    private async Task<PostingBatch?> GetActivePostingBatchAsync(long depreciationRunId, CancellationToken ct)
    {
        var query = _queryBuilder.For<PostingBatch>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.FADEPRECIATION &&
                        x.DocumentId == depreciationRunId &&
                        x.Status == PostingBatchStatusConst.POSTED)
            .Build();

        return await _postingBatchQuery.GetAsync(query, ct);
    }

    private async Task<Result> ReverseAccountingEntriesAsync(long depreciationRunId, long reversalBatchId, CancellationToken ct)
    {
        var query = _queryBuilder.For<AccountingRegisterEntry>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.FADEPRECIATION &&
                        x.DocumentId == depreciationRunId &&
                        x.ReversalEntryId == null)
            .Build();
        query.AddIncludes(x => x.Include(e => e.RegisterEntrySubkontos));

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
                Side = subkonto.Side == SubkontoSideConst.DEBIT ? SubkontoSideConst.CREDIT : SubkontoSideConst.DEBIT,
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

    private static bool TryParsePeriod(string period, out DateTime periodMonth) =>
        DateTime.TryParseExact(period, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out periodMonth);

    private static DateTime StartOfMonth(DateTime value) => new(value.Year, value.Month, 1);

    private static int MonthsBetween(DateTime from, DateTime to) =>
        (to.Year - from.Year) * 12 + to.Month - from.Month;

    private static decimal RoundAmount(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private Task SaveChangesAsync(CancellationToken ct) => _unitOfWork.SaveChangesAsync(ct);
}
