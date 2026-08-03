using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.Contracts;
using Application.Features.CounterpartyCards;
using Application.Features.PurchaseDocTables;
using Application.Features.Warehouses;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.PurchaseDocs;

public class PurchaseDocService : BaseService, IPurchaseDocService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IPurchaseLifecycleService _purchaseLifecycleService;
    private readonly IAuditLogService _auditLogService;
    private readonly IQueryRepository<PurchaseDoc> _query;
    private readonly IQueryRepository<VatRate> _vatRateQuery;
    private readonly ICommandRepository<PurchaseDoc> _command;
    private readonly IQueryRepository<Contract> _contractQuery;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
    private readonly IQueryRepository<Warehouse> _warehouseQuery;
    private readonly IQueryRepository<Currency> _currencyQuery;
    private readonly IQueryRepository<Unit> _unitQuery;
    private readonly IQueryRepository<Product> _productQuery;
    private readonly ICommandRepository<ProductTable> _productTableCommand;
    private readonly IQueryRepository<PurchaseDocTable> _purchaseDocTableQuery;
    private readonly ICommandRepository<PurchaseDocProduct> _productLineCommand;
    private readonly ICommandRepository<PurchaseDocTable> _tableLineCommand;
    private readonly IDocumentNumberService _documentNumberService;

    public PurchaseDocService(IUserContext userContext,
                              IQueryBuilder queryBuilder,
                              IPurchaseLifecycleService purchaseLifecycleService,
                              IAuditLogService auditLogService,
                              IDocumentNumberService documentNumberService,
                              IQueryRepository<PurchaseDoc> query,
                              IQueryRepository<VatRate> vatRateQuery,
                              IQueryRepository<Contract> contractQuery,
                              IQueryRepository<CounterpartyCard> counterpartyQuery,
                              IQueryRepository<Warehouse> warehouseQuery,
                              IQueryRepository<Currency> currencyQuery,
                              IQueryRepository<Unit> unitQuery,
                              IQueryRepository<Product> productQuery,
                              ICommandRepository<ProductTable> productTableCommand,
                              IQueryRepository<PurchaseDocTable> purchaseDocTableQuery,
                              ICommandRepository<PurchaseDoc> command,
                              ICommandRepository<PurchaseDocProduct> productLineCommand,
                              ICommandRepository<PurchaseDocTable> tableLineCommand,
                              ILogger<PurchaseDocService> logger,
                              IUnitOfWork unitOfWork)
            : base(logger, unitOfWork)
    {
        _query = query;
        _command = command;
        _productLineCommand = productLineCommand;
        _tableLineCommand = tableLineCommand;
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _purchaseLifecycleService = purchaseLifecycleService;
        _auditLogService = auditLogService;
        _vatRateQuery = vatRateQuery;
        _contractQuery = contractQuery;
        _counterpartyQuery = counterpartyQuery;
        _warehouseQuery = warehouseQuery;
        _currencyQuery = currencyQuery;
        _unitQuery = unitQuery;
        _productQuery = productQuery;
        _productTableCommand = productTableCommand;
        _purchaseDocTableQuery = purchaseDocTableQuery;
        _documentNumberService = documentNumberService;
    }

    public Task<Result<PagedResponse<PurchaseDocListDto>>> GetAllAsync(PurchaseDocListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<PurchaseDoc, PurchaseDocListDto, PurchaseDocListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<PurchaseDocDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<PurchaseDocDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<PurchaseDoc>()
                .Where(p => p.Id == id && p.OrganizationId == _userContext.OrganizationId.Value)
                .As<PurchaseDocDto>()
                .Build();
            var entity = await _query.GetAsync(query, ct);

            if (entity == null)
                return Result.Failure<PurchaseDocDto>(PurchaseDocErrors.NotFound(id, _userContext.LanguageId));

            return Result.Success(entity);
        });

    public Task<Result<long>> CreateAsync(PurchaseDocCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var headerValidation = await ValidateHeaderReferencesAsync(dto, ct);
            if (!headerValidation.IsSuccess)
                return Result.Failure<long>(headerValidation.Error);

            var allLinesResult = await BuildAllLinesAsync(_userContext.OrganizationId.Value, dto.Lines, ct);
            if (!allLinesResult.IsSuccess)
                return Result.Failure<long>(allLinesResult.Error);

            var allLines = allLinesResult.Value;
            var documentNumberResult = await _documentNumberService.GetNextAsync(
                _userContext.OrganizationId.Value,
                DocumentTypeIdConst.PURCHASE,
                dto.DocDate,
                ct);
            if (!documentNumberResult.IsSuccess)
                return Result.Failure<long>(documentNumberResult.Error);

            var doc = new PurchaseDoc
            {
                OrganizationId = _userContext.OrganizationId.Value,
                DocNumber = documentNumberResult.Value.DocumentNumber,
                DocDate = dto.DocDate,
                CurrencyId = dto.CurrencyId,
                ExchangeRate = dto.ExchangeRate == 0 ? 1m : dto.ExchangeRate,
                PurchaseDocProducts = allLines,
                TotalAmount = allLines.Sum(l => l.Amount),
                VatAmount = allLines.Sum(l => l.VatAmount),
                FinalAmount = allLines.Sum(l => l.TotalAmount),
                StatusId = DocumentStatusIdConst.DRAFT,
                Comment = dto.Comment,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now,
                WarehouseId = dto.WarehouseId,
                CounterpartyId = dto.CounterpartyId,
                ContractId = dto.ContractId,
                SupplierAccountId = dto.SupplierAccountId,
            };

            await _command.CreateAsync(doc, ct);

            if (dto.ProcessingMode == PurchaseProcessingMode.Immediate)
            {
                var confirmResult = await _purchaseLifecycleService.ConfirmAsync(doc.Id, ct);
                if (!confirmResult.IsSuccess)
                    return Result.Failure<long>(confirmResult.Error);
            }

            var docDto = await GetByIdInternalAsync(doc.Id, ct);
            if (docDto != null)
            {
                _auditLogService.SetNewValues(docDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.PurchaseDoc, doc.Id.ToString(), AuditLogOperationTypeConst.Create);
            }

            return Result.Success(doc.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, PurchaseDocUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<PurchaseDoc>()
                .Where(p => p.Id == id && p.OrganizationId == _userContext.OrganizationId.Value)
                .Build();
            var doc = await _query.GetAsync(query, ct);

            if (doc == null)
                return Result.Failure(PurchaseDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(PurchaseDocErrors.CannotUpdateInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            var headerValidation = await ValidateHeaderReferencesAsync(dto, ct);
            if (!headerValidation.IsSuccess)
                return Result.Failure(headerValidation.Error);

            var allLinesResult = await BuildAllLinesAsync(_userContext.OrganizationId.Value, dto.Lines, ct);
            if (!allLinesResult.IsSuccess)
                return Result.Failure(allLinesResult.Error);

            var newLines = allLinesResult.Value;
            var existingTableLinks = await GetPurchaseTableLinksAsync(id, ct);
            var oldPurchaseDocLineIds = existingTableLinks.Select(x => x.OwnerId).Distinct().ToList();
            var oldProductTableIds = existingTableLinks.Select(x => x.ProductTableId).Distinct().ToList();

            // Eski qatorlarni o'chirib, yangilarini yozamiz
            if (oldPurchaseDocLineIds.Count > 0)
                await _tableLineCommand.DeleteAsync(l => oldPurchaseDocLineIds.Contains(l.OwnerId), ct);
            await _productLineCommand.DeleteAsync(l => l.OwnerId == id, ct);
            if (oldProductTableIds.Count > 0)
                await _productTableCommand.DeleteAsync(x => oldProductTableIds.Contains(x.Id), ct);

            foreach (var line in newLines)
                line.OwnerId = id;

            await _productLineCommand.CreateAsync(newLines, ct);

            doc.OrganizationId = _userContext.OrganizationId.Value;
            doc.DocDate = DateTime.SpecifyKind(dto.DocDate, DateTimeKind.Unspecified);
            doc.CounterpartyId = dto.CounterpartyId;
            doc.WarehouseId = dto.WarehouseId;
            doc.CurrencyId = dto.CurrencyId;
            doc.ExchangeRate = dto.ExchangeRate == 0 ? 1m : dto.ExchangeRate;
            doc.ContractId = dto.ContractId;
            doc.SupplierAccountId = dto.SupplierAccountId;
            doc.TotalAmount = newLines.Sum(l => l.Amount);
            doc.VatAmount = newLines.Sum(l => l.VatAmount);
            doc.FinalAmount = newLines.Sum(l => l.TotalAmount);
            doc.Comment = dto.Comment;
            // State is lifecycle-managed; draft updates must not overwrite it from request payload.

            await _command.UpdateAsync(doc, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.PurchaseDoc, id.ToString(), AuditLogOperationTypeConst.Update, dto.Comment);
            }

            return Result.Success();
        }, ct);

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        _purchaseLifecycleService.ConfirmAsync(id, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        _purchaseLifecycleService.CancelAsync(id, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<PurchaseDoc>()
                .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
                .Build();
            var doc = await _query.GetAsync(query, ct);

            if (doc == null)
                return Result.Failure(PurchaseDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(PurchaseDocErrors.CannotDeleteInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            var productTableIds = (await GetPurchaseTableLinksAsync(id, ct))
                .Select(x => x.ProductTableId)
                .Distinct()
                .ToList();

            // Avval barcha qatorlarni o'chiramiz, keyin hujjatni
            await _tableLineCommand.DeleteAsync(l => l.Owner.OwnerId == id, ct);
            await _productLineCommand.DeleteAsync(l => l.OwnerId == id, ct);
            if (productTableIds.Count > 0)
                await _productTableCommand.DeleteAsync(x => productTableIds.Contains(x.Id), ct);

            doc.StateId = StateIdConst.PASSIVE;
            await _command.UpdateAsync(doc, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.PurchaseDoc, id.ToString(), AuditLogOperationTypeConst.Delete);
            }

            return Result.Success();
        }, ct);

    private async Task<PurchaseDocDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<PurchaseDoc>()
            .Where(p => p.Id == id && p.OrganizationId == _userContext.OrganizationId.Value)
            .As<PurchaseDocDto>()
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<List<PurchaseTableLink>> GetPurchaseTableLinksAsync(long purchaseDocId, CancellationToken ct)
    {
        var query = _queryBuilder.For<PurchaseDocTable>()
            .Where(x => x.Owner.OwnerId == purchaseDocId)
            .As(x => new PurchaseTableLink(x.OwnerId, x.ProductTableId))
            .Build();

        return await _purchaseDocTableQuery.GetAllAsync(query, ct);
    }

    private async Task<Result> ValidateHeaderReferencesAsync(PurchaseDocBaseDto dto, CancellationToken ct)
    {
        var counterpartyExists = await _counterpartyQuery.AnyAsync(x => x.Id == dto.CounterpartyId, ct);
        if (!counterpartyExists)
            return Result.Failure(CounterpartyCardErrors.NotFound(dto.CounterpartyId, _userContext.LanguageId));

        var warehouseExists = await _warehouseQuery.AnyAsync(x => x.Id == dto.WarehouseId, ct);
        if (!warehouseExists)
            return Result.Failure(WarehouseErrors.NotFound(dto.WarehouseId, _userContext.LanguageId));

        var currencyExists = await _currencyQuery.AnyAsync(x => x.Id == dto.CurrencyId, ct);
        if (!currencyExists)
            return Result.Failure(PurchaseDocErrors.CurrencyNotFound(dto.CurrencyId, _userContext.LanguageId));

        if (dto.ContractId.HasValue)
        {
            var contractExists = await _contractQuery.AnyAsync(x => x.Id == dto.ContractId.Value, ct);
            if (!contractExists)
                return Result.Failure(ContractErrors.NotFound(dto.ContractId.Value, _userContext.LanguageId));
        }

        return Result.Success();
    }

    private async Task<Result<List<PurchaseDocProduct>>> BuildAllLinesAsync(
        int organizationId,
        List<PurchaseDocLineDto> productLineDtos,
        CancellationToken ct)
    {
        var allLines = new List<PurchaseDocProduct>();

        if (productLineDtos.Count > 0)
        {
            var productResult = await BuildProductLinesAsync(organizationId, productLineDtos, ct);
            if (!productResult.IsSuccess)
                return Result.Failure<List<PurchaseDocProduct>>(productResult.Error);

            allLines.AddRange(productResult.Value);
        }

        return allLines;
    }

    private async Task<Result<List<PurchaseDocProduct>>> BuildProductLinesAsync(
        int organizationId, List<PurchaseDocLineDto> lineDtos, CancellationToken ct)
    {
        var lines = new List<PurchaseDocProduct>();
        var markingNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var productIds = lineDtos.Select(x => x.ProductId).Distinct().ToList();
        var unitIds = lineDtos.Select(x => x.UnitId).Distinct().ToList();
        var vatRateIds = lineDtos.Where(x => x.VatRateId.HasValue).Select(x => x.VatRateId!.Value).Distinct().ToList();

        var productsQuery = _queryBuilder.For<Product>()
            .Where(x => productIds.Contains(x.Id))
            .Build();
        var products = await _productQuery.GetAllAsync(productsQuery, ct);
        var productById = products.ToDictionary(x => x.Id);

        var unitsQuery = _queryBuilder.For<Unit>()
            .Where(x => unitIds.Contains(x.Id))
            .Build();
        var units = await _unitQuery.GetAllAsync(unitsQuery, ct);
        var unitIdsFound = units.Select(x => x.Id).ToHashSet();

        var vatRateById = new Dictionary<short, VatRate>();
        if (vatRateIds.Count > 0)
        {
            var vatRatesQuery = _queryBuilder.For<VatRate>()
                .Where(x => vatRateIds.Contains(x.Id))
                .Build();
            var vatRates = await _vatRateQuery.GetAllAsync(vatRatesQuery, ct);
            vatRateById = vatRates.ToDictionary(x => x.Id);
        }

        foreach (var dto in lineDtos)
        {
            if (!productById.TryGetValue(dto.ProductId, out var product))
                return Result.Failure<List<PurchaseDocProduct>>(
                    PurchaseDocErrors.ProductNotFound(dto.ProductId, _userContext.LanguageId));

            if (!unitIdsFound.Contains(dto.UnitId))
                return Result.Failure<List<PurchaseDocProduct>>(
                    PurchaseDocErrors.UnitNotFound(dto.UnitId, _userContext.LanguageId));

            if (dto.Quantity <= 0)
                return Result.Failure<List<PurchaseDocProduct>>(
                    PurchaseDocTableErrors.InvalidProductQuantity(dto.ProductId, dto.Quantity, _userContext.LanguageId));

            if (dto.UnitPrice < 0)
                return Result.Failure<List<PurchaseDocProduct>>(
                    PurchaseDocTableErrors.InvalidProductUnitPrice(dto.ProductId, dto.UnitPrice, _userContext.LanguageId));

            if (product.IsService || !product.IsPieceTracked)
            {
                if (dto.Items.Count > 0)
                    return Result.Failure<List<PurchaseDocProduct>>(
                        PurchaseDocErrors.ServiceItemsNotAllowed(dto.ProductId, _userContext.LanguageId));
            }
            else
            {
                if (dto.Quantity != decimal.Truncate(dto.Quantity))
                    return Result.Failure<List<PurchaseDocProduct>>(
                        PurchaseDocTableErrors.InvalidProductQuantity(dto.ProductId, dto.Quantity, _userContext.LanguageId));

                if (dto.Items.Count == 0)
                    return Result.Failure<List<PurchaseDocProduct>>(
                        PurchaseDocTableErrors.ProductItemsRequired(dto.ProductId, _userContext.LanguageId));

                if (dto.Quantity != dto.Items.Count)
                    return Result.Failure<List<PurchaseDocProduct>>(
                        PurchaseDocTableErrors.ProductQuantityItemsMismatch(dto.ProductId, dto.Quantity, dto.Items.Count, _userContext.LanguageId));

                foreach (var item in dto.Items)
                {
                    if (string.IsNullOrWhiteSpace(item.MarkingNumber))
                        return Result.Failure<List<PurchaseDocProduct>>(
                            PurchaseDocTableErrors.MarkingNumberRequired(dto.ProductId, _userContext.LanguageId));

                    if (!markingNumbers.Add(item.MarkingNumber.Trim()))
                        return Result.Failure<List<PurchaseDocProduct>>(
                            PurchaseDocTableErrors.DuplicateMarkingNumber(item.MarkingNumber, _userContext.LanguageId));
                }
            }

            var vatRateId = dto.VatRateId;
            var amount = dto.UnitPrice * dto.Quantity;
            var vatAmount = 0m;

            if (vatRateId.HasValue)
            {
                if (!vatRateById.TryGetValue(vatRateId.Value, out var vatRate))
                    return Result.Failure<List<PurchaseDocProduct>>(PurchaseDocTableErrors.VatRateNotFound(vatRateId.Value, _userContext.LanguageId));

                vatAmount = Math.Round(amount * vatRate.Rate / 100, 8);
            }

            var itemVatAmounts = product.IsService
                ? new List<decimal>()
                : SplitAmount(vatAmount, dto.Items.Count);

            lines.Add(new PurchaseDocProduct
            {
                ProductId = dto.ProductId,
                UnitId = dto.UnitId,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                Amount = amount,
                VatRateId = vatRateId,
                DebitAccountId = dto.DebitAccountId,
                VatAccountId = dto.VatAccountId,
                VatAmount = vatAmount,
                TotalAmount = amount + vatAmount,
                PurchaseDocTables = product.IsService
                    ? new List<PurchaseDocTable>()
                    : dto.Items.Select((item, index) => new PurchaseDocTable
                    {
                        Amount = dto.UnitPrice,
                        VatRateId = vatRateId,
                        VatAmount = itemVatAmounts[index],
                        TotalAmount = dto.UnitPrice + itemVatAmounts[index],
                        ProductTable = new ProductTable
                        {
                            ProductId = dto.ProductId,
                            SerialNumber = item.SerialNumber,
                            MarkingNumber = item.MarkingNumber.Trim(),
                            CreatedDate = DateTime.Now,
                        }
                    }).ToList()
            });
        }

        return lines;
    }

    private static List<decimal> SplitAmount(decimal amount, int count)
    {
        if (count <= 0)
            return new List<decimal>();

        var split = Math.Round(amount / count, 8);
        var result = Enumerable.Repeat(split, count).ToList();
        result[^1] += amount - result.Sum();
        return result;
    }

    private sealed record PurchaseTableLink(long OwnerId, int ProductTableId);
}
