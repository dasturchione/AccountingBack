using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.CounterpartyCards;
using Application.Features.InventoryRegisterBalances;
using Application.Features.Products;
using Application.Features.PurchaseDocTables;
using Application.Features.Register.AccountingRegisterEntries;
using Application.Features.Warehouses;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.PurchaseDocs;

public class PurchaseDocService : BaseService, IPurchaseDocService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAccountingDispatcher _dispatcher;
    private readonly IInventoryDispatcher _inventoryDispatcher;
    private readonly IQueryRepository<PurchaseDoc> _query;
    private readonly IQueryRepository<VatRate> _vatRateQuery;
    private readonly ICommandRepository<PurchaseDoc> _command;
    private readonly ICommandRepository<PurchaseDocTable> _lineCommand;
    private readonly IQueryRepository<Product> _productQuery;
    private readonly IQueryRepository<Warehouse> _warehouseQuery;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;

    public PurchaseDocService(IUnitOfWork unitOfWork,
                              IUserContext userContext,
                              IQueryBuilder queryBuilder,
                              IAccountingDispatcher dispatcher,
                              IInventoryDispatcher inventoryDispatcher,
                              IQueryRepository<PurchaseDoc> query,
                              IQueryRepository<VatRate> vatRateQuery,
                              ICommandRepository<PurchaseDoc> command,
                              ICommandRepository<PurchaseDocTable> lineCommand,
                              IQueryRepository<Warehouse> warehouseQuery,
                              IQueryRepository<Product> productQuery,
                              IQueryRepository<CounterpartyCard> counterpartyQuery,
                              ILogger<PurchaseDocService> logger) : base(logger)
    {
        _query               = query;
        _command             = command;
        _dispatcher          = dispatcher;
        _unitOfWork          = unitOfWork;
        _lineCommand         = lineCommand;
        _userContext         = userContext;
        _queryBuilder        = queryBuilder;
        _vatRateQuery        = vatRateQuery;
        _productQuery        = productQuery;
        _warehouseQuery      = warehouseQuery;
        _counterpartyQuery   = counterpartyQuery;
        _inventoryDispatcher = inventoryDispatcher;
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
        ExecuteAsync(nameof(CreateAsync), _unitOfWork, async () =>
        {
            if (await _query.AnyAsync(x => x.OrganizationId == dto.OrganizationId && x.DocNumber == dto.DocNumber, ct))
                return Result.Failure<long>(PurchaseDocErrors.DocNumberConflict(dto.DocNumber, _userContext.LanguageId));

            var warehouseQuery = _queryBuilder.For<Warehouse>().Where(x => x.Id == dto.WarehouseId).Build();
            var warehouse = await _warehouseQuery.GetAsync(warehouseQuery, ct);
            if (warehouse is null)
                return Result.Failure<long>(WarehouseErrors.NotFound(dto.WarehouseId, _userContext.LanguageId));

            var counterpartyQuery = _queryBuilder.For<CounterpartyCard>().Where(x => x.Id == dto.CounterpartyId).Build();
            var counterparty = await _counterpartyQuery.GetAsync(counterpartyQuery, ct);
            if (counterparty is null)
                return Result.Failure<long>(CounterpartyCardErrors.NotFound(dto.CounterpartyId, _userContext.LanguageId));

            // Barcha qatorlar uchun QQS ni oldindan hisoblaymiz
            var linesResult = await BuildLinesAsync(dto.OrganizationId, dto.Lines, ct);
            if (!linesResult.IsSuccess)
                return Result.Failure<long>(linesResult.Error);

            var lines = linesResult.Value;

            var doc = new PurchaseDoc
            {
                OrganizationId = dto.OrganizationId,
                DocNumber      = dto.DocNumber,
                DocDate        = dto.DocDate,
                CurrencyId     = dto.CurrencyId,
                PurchaseDocTables = lines,
                TotalAmount    = lines.Sum(l => l.Amount),
                VatAmount      = lines.Sum(l => l.VatAmount),
                FinalAmount    = lines.Sum(l => l.TotalAmount),
                StatusId       = DocumentStatusIdConst.DRAFT,
                Comment        = dto.Comment,
                StateId        = StateIdConst.ACTIVE,
                CreatedDate    = DateTime.Now,
                Warehouse      = warehouse,
                Counterparty   = counterparty,
            };

            await _command.CreateAsync(doc, ct);

            // Inventory handler uchun ProductTable navigation kerak
            var fullDocQuery = _queryBuilder.For<PurchaseDoc>().Where(d => d.Id == doc.Id).Build();
            fullDocQuery.AddIncludes(b => b.Include(d => d.PurchaseDocTables).ThenInclude(l => l.ProductTable));
            var fullDoc = await _query.GetAsync(fullDocQuery, ct) ?? doc;

            var dispatch = await _dispatcher.ProcessAsync(fullDoc, ct);
            if (!dispatch.IsSuccess)
                return Result.Failure<long>(dispatch.Error);

            var inventoryDispatch = await _inventoryDispatcher.ProcessAsync(fullDoc, ct);
            if (!inventoryDispatch.IsSuccess)
                return Result.Failure<long>(inventoryDispatch.Error);

            return Result.Success(doc.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, PurchaseDocUpdateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(UpdateAsync), _unitOfWork, async () =>
        {
            var query = _queryBuilder.For<PurchaseDoc>().Where(p => p.Id == id).Build();
            var doc   = await _query.GetAsync(query, ct);

            if (doc == null)
                return Result.Failure(PurchaseDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
                return Result.Failure(PurchaseDocErrors.AlreadyPosted(id, _userContext.LanguageId));

            if (doc.DocNumber != dto.DocNumber &&
                await _query.AnyAsync(x => x.OrganizationId == dto.OrganizationId && x.DocNumber == dto.DocNumber, ct))
                return Result.Failure(PurchaseDocErrors.DocNumberConflict(dto.DocNumber, _userContext.LanguageId));

            // Yangi qatorlarni hisoblaymiz
            var linesResult = await BuildLinesAsync(dto.OrganizationId, dto.Lines, ct);
            if (!linesResult.IsSuccess)
                return Result.Failure(linesResult.Error);

            var newLines = linesResult.Value;

            // Eski qatorlarni o'chirib, yangilarini yozamiz
            await _lineCommand.DeleteAsync(l => l.OwnerId == id, ct);

            foreach (var line in newLines)
                line.OwnerId = id;

            await _lineCommand.CreateAsync(newLines, ct);

            doc.OrganizationId = dto.OrganizationId;
            doc.DocNumber      = dto.DocNumber;
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

            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(DeleteAsync), _unitOfWork, async () =>
        {
            var query = _queryBuilder.For<PurchaseDoc>().Where(x => x.Id == id).Build();
            var doc   = await _query.GetAsync(query, ct);

            if (doc == null)
                return Result.Failure(PurchaseDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
                return Result.Failure(PurchaseDocErrors.AlreadyPosted(id, _userContext.LanguageId));

            // Avval barcha qatorlarni o'chiramiz, keyin hujjatni
            await _lineCommand.DeleteAsync(l => l.OwnerId == id, ct);

            doc.StateId = StateIdConst.PASSIVE;
            await _command.UpdateAsync(doc, ct);

            return Result.Success();
        }, ct);

    // dto.Lines dan PurchaseDocTable entity larini yaratib beradi
    // VatRate DB dan olinadi — agar topilmasa xato qaytaradi
    private async Task<Result<List<PurchaseDocTable>>> BuildLinesAsync(int organizationid, List<PurchaseDocLineDto> lineDtos, CancellationToken ct)
    {
        var lines = new List<PurchaseDocTable>(lineDtos.Count);

        var productIds = lineDtos.Select(s => s.ProductId).Distinct();
        var productQuery = _queryBuilder.For<Product>().Where(x => productIds.Contains(x.Id)).Build();
        var products = await _productQuery.GetAllAsync(productQuery);
        if (productIds.Count() != products.Count())
        {
            return Result.Failure<List<PurchaseDocTable>>(ProductErrors.NotFound(1, _userContext.LanguageId));
        }

        foreach (var productsGroup in lineDtos.GroupBy(g => g.ProductId))
        {
            var product = products.First(f => f.Id == productsGroup.Key);
            var vatRateId = productsGroup.First().VatRateId;
            var productPrice = productsGroup.First().Price;
            var vatAmount = 0m;

            if (vatRateId.HasValue)
            {
                var vatQuery = _queryBuilder.For<VatRate>().Where(v => v.Id == vatRateId.Value).Build();
                var vatRate = await _vatRateQuery.GetAsync(vatQuery, ct);

                if (vatRate == null)
                    return Result.Failure<List<PurchaseDocTable>>(PurchaseDocTableErrors.VatRateNotFound(vatRateId.Value, _userContext.LanguageId));

                vatAmount = Math.Round(productPrice * vatRate.Rate / 100, 2);
            }

            lines.AddRange(productsGroup.Select(s => new PurchaseDocTable()
            {
                Amount = productPrice,
                Price = productPrice,
                Quantity = 1,
                TotalAmount = productPrice + vatAmount,
                VatRateId = vatRateId,
                VatAmount = vatAmount,
                ProductTable = new ProductTable
                {
                    Product = product,
                    SerialNumber = s.SerialNumber,
                    MarkingNumber = s.MarkingNumber,
                    CreatedDate = DateTime.Now,
                    OrganizationId = organizationid,
                    StateId = StateIdConst.ACTIVE
                }
            }));
        }

        //foreach (var dto in lineDtos)
        //{
        //    var amount    = dto.Quantity * dto.Price;
        //    var vatAmount = 0m;

        //    if (dto.VatRateId.HasValue)
        //    {
        //        var vatQuery = _queryBuilder.For<VatRate>().Where(v => v.Id == dto.VatRateId.Value).Build();
        //        var vatRate  = await _vatRateQuery.GetAsync(vatQuery, ct);

        //        if (vatRate == null)
        //            return Result.Failure<List<PurchaseDocTable>>(PurchaseDocTableErrors.VatRateNotFound(dto.VatRateId.Value, _userContext.LanguageId));

        //        vatAmount = Math.Round(amount * vatRate.Rate / 100, 2);
        //    }

        //    lines.Add(new PurchaseDocTable
        //    {
        //        ProductTableId = dto.ProductTableId,
        //        Quantity    = dto.Quantity,
        //        Price       = dto.Price,
        //        Amount      = amount,
        //        VatRateId   = dto.VatRateId,
        //        VatAmount   = vatAmount,
        //        TotalAmount = amount + vatAmount
        //    });
        //}

        return lines;
    }
}
