using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.BankOperations;
using Application.Features.CashOperations;
using Application.Features.InventoryAdjustments;
using Application.Features.InventoryCounts;
using Application.Features.PurchaseDocs;
using Application.Features.SaleDocs;
using Application.Features.WarehouseTransfers;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Reposting;

public class RepostService : BaseService, IRepostService
{
    private static readonly HashSet<short> SupportedDocumentTypes =
    [
        DocumentTypeIdConst.PURCHASE,
        DocumentTypeIdConst.SALE,
        DocumentTypeIdConst.BANKOPERATION,
        DocumentTypeIdConst.CASHOPERATION,
        DocumentTypeIdConst.WAREHOUSETRANSFER,
        DocumentTypeIdConst.INVENTORYADJUSTMENT,
        DocumentTypeIdConst.INVENTORYCOUNT
    ];

    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IRepostReadRepository _readRepository;
    private readonly IQueryRepository<AccountingPeriod> _periodQuery;
    private readonly IQueryRepository<SaleDoc> _saleDocQuery;
    private readonly IPurchaseDocService _purchaseDocService;
    private readonly ISaleDocService _saleDocService;
    private readonly IBankOperationService _bankOperationService;
    private readonly ICashOperationService _cashOperationService;
    private readonly IWarehouseTransferService _warehouseTransferService;
    private readonly IInventoryAdjustmentService _inventoryAdjustmentService;
    private readonly IInventoryCountService _inventoryCountService;

    public RepostService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IDocumentPostingLock postingLock,
        IRepostReadRepository readRepository,
        IQueryRepository<AccountingPeriod> periodQuery,
        IQueryRepository<SaleDoc> saleDocQuery,
        IPurchaseDocService purchaseDocService,
        ISaleDocService saleDocService,
        IBankOperationService bankOperationService,
        ICashOperationService cashOperationService,
        IWarehouseTransferService warehouseTransferService,
        IInventoryAdjustmentService inventoryAdjustmentService,
        IInventoryCountService inventoryCountService,
        ILogger<RepostService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _postingLock = postingLock;
        _readRepository = readRepository;
        _periodQuery = periodQuery;
        _saleDocQuery = saleDocQuery;
        _purchaseDocService = purchaseDocService;
        _saleDocService = saleDocService;
        _bankOperationService = bankOperationService;
        _cashOperationService = cashOperationService;
        _warehouseTransferService = warehouseTransferService;
        _inventoryAdjustmentService = inventoryAdjustmentService;
        _inventoryCountService = inventoryCountService;
    }

    public Task<Result<RepostDto>> RepostAsync(RepostFilter filter, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(RepostAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<RepostDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var validation = await ValidateAsync(filter, _userContext.OrganizationId.Value, ct);
            if (!validation.IsSuccess)
                return Result.Failure<RepostDto>(validation.Error);

            var candidates = await _readRepository.GetCandidatesAsync(new RepostReadRequest
            {
                OrganizationId = _userContext.OrganizationId.Value,
                DateFrom = validation.Value.DateFrom,
                DateTo = validation.Value.DateTo,
                DocumentType = filter.DocumentType,
                DocumentId = filter.DocumentId
            }, ct);

            if (filter.DocumentId.HasValue && filter.DocumentType.HasValue && candidates.Count == 0)
                return Result.Failure<RepostDto>(RepostErrors.InvalidDocument(filter.DocumentType.Value, filter.DocumentId.Value, _userContext.LanguageId));

            var processed = new List<RepostDocumentDto>(candidates.Count);

            foreach (var candidate in candidates)
            {
                var acquired = await _postingLock.TryAcquireAsync(candidate.DocumentType, candidate.DocumentId, ct);
                if (!acquired)
                    return Result.Failure<RepostDto>(RepostErrors.AlreadyReposting(candidate.DocumentType, candidate.DocumentId, _userContext.LanguageId));

                var result = await RepostDocumentAsync(candidate, ct);
                if (!result.IsSuccess)
                    return Result.Failure<RepostDto>(result.Error);

                processed.Add(new RepostDocumentDto
                {
                    DocumentType = candidate.DocumentType,
                    DocumentId = candidate.DocumentId,
                    PostingDate = candidate.DocDate
                });
            }

            return Result.Success(new RepostDto
            {
                ProcessedCount = processed.Count,
                Documents = processed
            });
        }, ct);

    private async Task<Result<RepostValidationState>> ValidateAsync(RepostFilter filter, int organizationId, CancellationToken ct)
    {
        if (filter.DateFrom.HasValue && filter.DateTo.HasValue && filter.DateFrom.Value.Date > filter.DateTo.Value.Date)
            return Result.Failure<RepostValidationState>(RepostErrors.InvalidDateRange(_userContext.LanguageId));

        if (filter.DocumentId.HasValue && !filter.DocumentType.HasValue)
            return Result.Failure<RepostValidationState>(RepostErrors.DocumentTypeRequired(_userContext.LanguageId));

        if (filter.DocumentType.HasValue && !SupportedDocumentTypes.Contains(filter.DocumentType.Value))
            return Result.Failure<RepostValidationState>(RepostErrors.UnsupportedDocumentType(filter.DocumentType.Value, _userContext.LanguageId));

        DateTime? dateFrom = filter.DateFrom?.Date;
        DateTime? dateTo = filter.DateTo?.Date.AddDays(1).AddTicks(-1);

        if (filter.PeriodId.HasValue)
        {
            var periodQuery = _queryBuilder.For<AccountingPeriod>()
                .Where(x => x.Id == filter.PeriodId.Value && x.OrganizationId == organizationId)
                .Build();
            var period = await _periodQuery.GetAsync(periodQuery, ct);

            if (period is null)
                return Result.Failure<RepostValidationState>(RepostErrors.PeriodNotFound(filter.PeriodId.Value, _userContext.LanguageId));

            if (period.IsClosed)
                return Result.Failure<RepostValidationState>(RepostErrors.ClosedPeriod(period.Id, _userContext.LanguageId));

            var periodStart = period.StartDate.ToDateTime(TimeOnly.MinValue);
            var periodEnd = period.EndDate.ToDateTime(TimeOnly.MaxValue);

            dateFrom ??= periodStart;
            dateTo ??= periodEnd;

            if (dateFrom.Value < periodStart || dateTo.Value > periodEnd)
                return Result.Failure<RepostValidationState>(RepostErrors.DateRangeOutsidePeriod(period.Id, _userContext.LanguageId));
        }
        else if (dateFrom.HasValue || dateTo.HasValue)
        {
            var closedPeriodQuery = _queryBuilder.For<AccountingPeriod>()
                .Where(x => x.OrganizationId == organizationId &&
                            x.IsClosed &&
                            (!dateFrom.HasValue || x.EndDate.ToDateTime(TimeOnly.MaxValue) >= dateFrom.Value) &&
                            (!dateTo.HasValue || x.StartDate.ToDateTime(TimeOnly.MinValue) <= dateTo.Value))
                .Build();

            var closedPeriod = await _periodQuery.GetAsync(closedPeriodQuery, ct);
            if (closedPeriod is not null)
                return Result.Failure<RepostValidationState>(RepostErrors.ClosedPeriod(closedPeriod.Id, _userContext.LanguageId));
        }

        return new RepostValidationState
        {
            DateFrom = dateFrom,
            DateTo = dateTo
        };
    }

    private async Task<Result> RepostDocumentAsync(RepostCandidate candidate, CancellationToken ct)
    {
        return candidate.DocumentType switch
        {
            DocumentTypeIdConst.PURCHASE => await RepostAsync(candidate.DocumentId, _purchaseDocService.CancelAsync, _purchaseDocService.ConfirmAsync, ct),
            DocumentTypeIdConst.BANKOPERATION => await RepostAsync(candidate.DocumentId, _bankOperationService.CancelAsync, _bankOperationService.ConfirmAsync, ct),
            DocumentTypeIdConst.CASHOPERATION => await RepostAsync(candidate.DocumentId, _cashOperationService.CancelAsync, _cashOperationService.ConfirmAsync, ct),
            DocumentTypeIdConst.WAREHOUSETRANSFER => await RepostAsync(candidate.DocumentId, _warehouseTransferService.CancelAsync, _warehouseTransferService.ConfirmAsync, ct),
            DocumentTypeIdConst.INVENTORYADJUSTMENT => await RepostAsync(candidate.DocumentId, _inventoryAdjustmentService.CancelAsync, _inventoryAdjustmentService.ConfirmAsync, ct),
            DocumentTypeIdConst.INVENTORYCOUNT => await RepostAsync(candidate.DocumentId, _inventoryCountService.CancelAsync, _inventoryCountService.ConfirmAsync, ct),
            DocumentTypeIdConst.SALE => await RepostSaleAsync(candidate.DocumentId, ct),
            _ => Result.Failure(RepostErrors.UnsupportedDocumentType(candidate.DocumentType, _userContext.LanguageId))
        };
    }

    private static async Task<Result> RepostAsync(
        long documentId,
        Func<long, CancellationToken, Task<Result>> cancel,
        Func<long, CancellationToken, Task<Result>> confirm,
        CancellationToken ct)
    {
        var cancelResult = await cancel(documentId, ct);
        if (!cancelResult.IsSuccess)
            return cancelResult;

        return await confirm(documentId, ct);
    }

    private async Task<Result> RepostSaleAsync(long documentId, CancellationToken ct)
    {
        var cancelResult = await _saleDocService.CancelAsync(documentId, ct);
        if (!cancelResult.IsSuccess)
            return cancelResult;

        var confirmDto = await BuildSaleConfirmDtoAsync(documentId, ct);
        if (!confirmDto.IsSuccess)
            return Result.Failure(confirmDto.Error);

        return await _saleDocService.ConfirmAsync(documentId, confirmDto.Value, ct);
    }

    private async Task<Result<SaleDocConfirmDto>> BuildSaleConfirmDtoAsync(long documentId, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<SaleDocConfirmDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var query = _queryBuilder.For<SaleDoc>()
            .Where(x => x.Id == documentId && x.OrganizationId == _userContext.OrganizationId.Value)
            .Build();
        query.AddIncludes(b => b.Include(x => x.SaleDocProducts));

        var doc = await _saleDocQuery.GetAsync(query, ct);
        if (doc is null)
            return Result.Failure<SaleDocConfirmDto>(RepostErrors.InvalidDocument(DocumentTypeIdConst.SALE, documentId, _userContext.LanguageId));

        return new SaleDocConfirmDto
        {
            Lines = doc.SaleDocProducts
                .OrderBy(x => x.Id)
                .Select(x => new SaleDocConfirmLineDto
                {
                    Id = x.Id,
                    UnitPrice = x.UnitPrice,
                    CostPrice = x.CostPrice
                })
                .ToList()
        };
    }

    private sealed class RepostValidationState
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
