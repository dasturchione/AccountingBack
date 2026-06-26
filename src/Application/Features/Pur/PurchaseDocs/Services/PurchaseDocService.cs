using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.Contracts;
using Application.Features.InventoryRegisterBalances;
using Application.Features.PurchaseDocTables;
using Application.Features.Register.AccountingRegisterEntries;
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
    private readonly IAuditLogService _auditLogService;
    private readonly IAccountingDispatcher _dispatcher;
    private readonly IInventoryDispatcher _inventoryDispatcher;
    private readonly IQueryRepository<PurchaseDoc> _query;
    private readonly IQueryRepository<VatRate> _vatRateQuery;
    private readonly ICommandRepository<PurchaseDoc> _command;
    private readonly IQueryRepository<Contract> _contractQuery;
    private readonly ICommandRepository<PurchaseDocProduct> _productLineCommand;
    private readonly ICommandRepository<PurchaseDocTable> _tableLineCommand;
    private readonly IDocNumberGenerator _docNumberGenerator;

    public PurchaseDocService(IUserContext userContext,
                              IQueryBuilder queryBuilder,
                              IAuditLogService auditLogService,
                              IAccountingDispatcher dispatcher,
                              IInventoryDispatcher inventoryDispatcher,
                              IDocNumberGenerator docNumberGenerator,
                              IQueryRepository<PurchaseDoc> query,
                              IQueryRepository<VatRate> vatRateQuery,
                              IQueryRepository<Contract> contractQuery,
                              ICommandRepository<PurchaseDoc> command,
                              ICommandRepository<PurchaseDocProduct> productLineCommand,
                              ICommandRepository<PurchaseDocTable> tableLineCommand,
                              ILogger<PurchaseDocService> logger,
                              IUnitOfWork unitOfWork)
            : base(logger, unitOfWork)
    {
        _query               = query;
        _command             = command;
        _dispatcher          = dispatcher;
        _productLineCommand  = productLineCommand;
        _tableLineCommand    = tableLineCommand;
        _userContext         = userContext;
        _queryBuilder        = queryBuilder;
        _auditLogService     = auditLogService;
        _vatRateQuery        = vatRateQuery;
        _contractQuery       = contractQuery;
        _inventoryDispatcher = inventoryDispatcher;
        _docNumberGenerator  = docNumberGenerator;
    }

    public Task<Result<PagedResponse<PurchaseDocListDto>>> GetAllAsync(PurchaseDocListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query     = _queryBuilder.BuildPaged<PurchaseDoc, PurchaseDocListDto, PurchaseDocListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<PurchaseDocDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var query  = _queryBuilder.For<PurchaseDoc>().Where(p => p.Id == id).As<PurchaseDocDto>().Build();
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

            if (dto.ContractId.HasValue)
            {
                var contractExists = await _contractQuery.AnyAsync(x => x.Id == dto.ContractId.Value);
                if (!contractExists)
                    return Result.Failure<long>(ContractErrors.NotFound(dto.ContractId.Value, _userContext.LanguageId));
            }

            var docNumber = await _docNumberGenerator.GenerateAsync(_userContext.OrganizationId.Value, "PUR", dto.DocDate, ct);

            var allLinesResult = await BuildAllLinesAsync(_userContext.OrganizationId.Value, dto.Lines, dto.ServiceLines, ct);
            if (!allLinesResult.IsSuccess)
                return Result.Failure<long>(allLinesResult.Error);

            var allLines = allLinesResult.Value;

            var doc = new PurchaseDoc
            {
                OrganizationId      = _userContext.OrganizationId.Value,
                DocNumber           = docNumber,
                DocDate             = dto.DocDate,
                CurrencyId          = dto.CurrencyId,
                PurchaseDocProducts = allLines,
                TotalAmount         = allLines.Sum(l => l.Amount),
                VatAmount           = allLines.Sum(l => l.VatAmount),
                FinalAmount         = allLines.Sum(l => l.TotalAmount),
                StatusId            = DocumentStatusIdConst.DRAFT,
                Comment             = dto.Comment,
                StateId             = StateIdConst.ACTIVE,
                CreatedDate         = DateTime.Now,
                WarehouseId         = dto.WarehouseId,
                CounterpartyId      = dto.CounterpartyId,
                ContractId          = dto.ContractId,
            };

            await _command.CreateAsync(doc, ct);

            // Inventory handler uchun ProductTable navigation kerak
            var fullDocQuery = _queryBuilder.For<PurchaseDoc>().Where(d => d.Id == doc.Id).Build();
            fullDocQuery.AddIncludes(b => b.Include(d => d.PurchaseDocProducts).ThenInclude(l => l.Product));
            fullDocQuery.AddIncludes(b => b.Include(d => d.PurchaseDocProducts).ThenInclude(l => l.Unit));
            fullDocQuery.AddIncludes(b => b.Include(d => d.PurchaseDocProducts).ThenInclude(l => l.VatRate));
            fullDocQuery.AddIncludes(b => b.Include(d => d.PurchaseDocProducts).ThenInclude(l => l.PurchaseDocTables).ThenInclude(t => t.ProductTable));
            var fullDoc = await _query.GetAsync(fullDocQuery, ct) ?? doc;

            var dispatch = await _dispatcher.ProcessAsync(fullDoc, ct);
            if (!dispatch.IsSuccess)
                return Result.Failure<long>(dispatch.Error);

            var inventoryDispatch = await _inventoryDispatcher.ProcessAsync(fullDoc, ct);
            if (!inventoryDispatch.IsSuccess)
                return Result.Failure<long>(inventoryDispatch.Error);

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

            var query = _queryBuilder.For<PurchaseDoc>().Where(p => p.Id == id).Build();
            var doc   = await _query.GetAsync(query, ct);

            if (doc == null)
                return Result.Failure(PurchaseDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
                return Result.Failure(PurchaseDocErrors.AlreadyPosted(id, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            var allLinesResult = await BuildAllLinesAsync(_userContext.OrganizationId.Value, dto.Lines, dto.ServiceLines, ct);
            if (!allLinesResult.IsSuccess)
                return Result.Failure(allLinesResult.Error);

            var newLines = allLinesResult.Value;

            // Eski qatorlarni o'chirib, yangilarini yozamiz
            await _tableLineCommand.DeleteAsync(l => l.Owner.OwnerId == id, ct);
            await _productLineCommand.DeleteAsync(l => l.OwnerId == id, ct);

            foreach (var line in newLines)
                line.OwnerId = id;

            await _productLineCommand.CreateAsync(newLines, ct);

            doc.OrganizationId = _userContext.OrganizationId.Value;
            doc.DocDate        = DateTime.SpecifyKind(dto.DocDate, DateTimeKind.Unspecified);
            doc.CounterpartyId = dto.CounterpartyId;
            doc.WarehouseId    = dto.WarehouseId;
            doc.CurrencyId     = dto.CurrencyId;
            doc.TotalAmount    = newLines.Sum(l => l.Amount);
            doc.VatAmount      = newLines.Sum(l => l.VatAmount);
            doc.FinalAmount    = newLines.Sum(l => l.TotalAmount);
            doc.Comment        = dto.Comment;
            doc.StateId        = dto.StateId;

            await _command.UpdateAsync(doc, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.PurchaseDoc, id.ToString(), AuditLogOperationTypeConst.Update, dto.Comment);
            }

            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            var query = _queryBuilder.For<PurchaseDoc>().Where(x => x.Id == id).Build();
            var doc   = await _query.GetAsync(query, ct);

            if (doc == null)
                return Result.Failure(PurchaseDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
                return Result.Failure(PurchaseDocErrors.AlreadyPosted(id, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            // Avval barcha qatorlarni o'chiramiz, keyin hujjatni
            await _tableLineCommand.DeleteAsync(l => l.Owner.OwnerId == id, ct);
            await _productLineCommand.DeleteAsync(l => l.OwnerId == id, ct);

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
        var query = _queryBuilder.For<PurchaseDoc>().Where(p => p.Id == id).As<PurchaseDocDto>().Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<Result<List<PurchaseDocProduct>>> BuildAllLinesAsync(
        int organizationId,
        List<PurchaseDocLineDto> productLineDtos,
        List<PurchaseDocServiceLineDto> serviceLineDtos,
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

        if (serviceLineDtos.Count > 0)
            return Result.Failure<List<PurchaseDocProduct>>(PurchaseDocTableErrors.ServiceLinesUnsupported(_userContext.LanguageId));

        return allLines;
    }

    private async Task<Result<List<PurchaseDocProduct>>> BuildProductLinesAsync(
        int organizationId, List<PurchaseDocLineDto> lineDtos, CancellationToken ct)
    {
        var lines = new List<PurchaseDocProduct>();
        var markingNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var dto in lineDtos)
        {
            if (dto.Quantity <= 0 || dto.Quantity != decimal.Truncate(dto.Quantity))
                return Result.Failure<List<PurchaseDocProduct>>(
                    PurchaseDocTableErrors.InvalidProductQuantity(dto.ProductId, dto.Quantity, _userContext.LanguageId));

            if (dto.UnitPrice < 0)
                return Result.Failure<List<PurchaseDocProduct>>(
                    PurchaseDocTableErrors.InvalidProductUnitPrice(dto.ProductId, dto.UnitPrice, _userContext.LanguageId));

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

            var vatRateId = dto.VatRateId;
            var amount = dto.UnitPrice * dto.Quantity;
            var vatAmount = 0m;

            if (vatRateId.HasValue)
            {
                var vatQuery = _queryBuilder.For<VatRate>().Where(v => v.Id == vatRateId.Value).Build();
                var vatRate = await _vatRateQuery.GetAsync(vatQuery, ct);

                if (vatRate == null)
                    return Result.Failure<List<PurchaseDocProduct>>(PurchaseDocTableErrors.VatRateNotFound(vatRateId.Value, _userContext.LanguageId));

                vatAmount = Math.Round(amount * vatRate.Rate / 100, 8);
            }

            var itemVatAmounts = SplitAmount(vatAmount, dto.Items.Count);

            lines.Add(new PurchaseDocProduct
            {
                ItemTypeId = PurchaseItemTypeIdConst.PRODUCT,
                ProductId = dto.ProductId,
                UnitId = dto.UnitId,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                Amount = amount,
                VatRateId = vatRateId,
                VatAmount = vatAmount,
                TotalAmount = amount + vatAmount,
                PurchaseDocTables = dto.Items.Select((item, index) => new PurchaseDocTable
                {
                    Amount = dto.UnitPrice,
                    VatRateId = vatRateId,
                    VatAmount = itemVatAmounts[index],
                    TotalAmount = dto.UnitPrice + itemVatAmounts[index],
                    ProductTable = new ProductTable
                    {
                        ProductId      = dto.ProductId,
                        SerialNumber   = item.SerialNumber,
                        MarkingNumber  = item.MarkingNumber.Trim(),
                        CreatedDate    = DateTime.Now,
                        OrganizationId = organizationId,
                        StateId        = StateIdConst.ACTIVE,
                        StatusId       = ProductTableStatusIdConst.IN_STOCK
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
}
