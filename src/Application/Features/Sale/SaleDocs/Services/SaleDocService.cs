using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.CounterpartyCards;
using Application.Features.InventoryRegisterBalances;
using Application.Features.Products;
using Application.Features.Register.AccountingRegisterEntries;
using Application.Features.SaleDocTables;
using Application.Features.Warehouses;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.SaleDocs;

public class SaleDocService : BaseService, ISaleDocService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IAccountingDispatcher _dispatcher;
    private readonly IInventoryDispatcher _inventoryDispatcher;
    private readonly IQueryRepository<SaleDoc> _query;
    private readonly IQueryRepository<VatRate> _vatRateQuery;
    private readonly ICommandRepository<SaleDoc> _command;
    private readonly ICommandRepository<SaleDocTable> _lineCommand;
    private readonly IQueryRepository<Product> _productQuery;
    private readonly IQueryRepository<Warehouse> _warehouseQuery;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
    private readonly IQueryRepository<ProductTable> _productTableQuery;
    private readonly IDocNumberGenerator _docNumberGenerator;

    public SaleDocService(IUserContext userContext,
                          IQueryBuilder queryBuilder,
                          IAuditLogService auditLogService,
                          IAccountingDispatcher dispatcher,
                          IInventoryDispatcher inventoryDispatcher,
                          IDocNumberGenerator docNumberGenerator,
                          IQueryRepository<SaleDoc> query,
                          IQueryRepository<VatRate> vatRateQuery,
                          ICommandRepository<SaleDoc> command,
                          ICommandRepository<SaleDocTable> lineCommand,
                          IQueryRepository<Warehouse> warehouseQuery,
                          IQueryRepository<Product> productQuery,
                          IQueryRepository<CounterpartyCard> counterpartyQuery,
                          IQueryRepository<ProductTable> productTableQuery,
                          ILogger<SaleDocService> logger,
                          IUnitOfWork unitOfWork)
            : base(logger, unitOfWork)
    {
        _query               = query;
        _command             = command;
        _dispatcher          = dispatcher;
        _lineCommand         = lineCommand;
        _userContext         = userContext;
        _queryBuilder        = queryBuilder;
        _auditLogService     = auditLogService;
        _vatRateQuery        = vatRateQuery;
        _productQuery        = productQuery;
        _warehouseQuery      = warehouseQuery;
        _counterpartyQuery   = counterpartyQuery;
        _productTableQuery   = productTableQuery;
        _inventoryDispatcher = inventoryDispatcher;
        _docNumberGenerator  = docNumberGenerator;
    }

    public Task<Result<PagedResponse<SaleDocListDto>>> GetAllAsync(SaleDocListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query     = _queryBuilder.BuildPaged<SaleDoc, SaleDocListDto, SaleDocListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<SaleDocDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var query  = _queryBuilder.For<SaleDoc>().Where(x => x.Id == id).As<SaleDocDto>().Build();
            var entity = await _query.GetAsync(query, ct);

            if (entity == null)
                return Result.Failure<SaleDocDto>(SaleDocErrors.NotFound(id, _userContext.LanguageId));

            return Result.Success(entity);
        });

    public Task<Result<long>> CreateAsync(SaleDocCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var orgId = _userContext.OrganizationId.Value;

            var docNumber = await _docNumberGenerator.GenerateAsync(orgId, "SAL", dto.DocDate, ct);

            var warehouseQuery = _queryBuilder.For<Warehouse>().Where(x => x.Id == dto.WarehouseId).Build();
            var warehouse = await _warehouseQuery.GetAsync(warehouseQuery, ct);
            if (warehouse is null)
                return Result.Failure<long>(WarehouseErrors.NotFound(dto.WarehouseId, _userContext.LanguageId));

            var counterpartyQuery = _queryBuilder.For<CounterpartyCard>().Where(x => x.Id == dto.CounterpartyId).Build();
            var counterparty = await _counterpartyQuery.GetAsync(counterpartyQuery, ct);
            if (counterparty is null)
                return Result.Failure<long>(CounterpartyCardErrors.NotFound(dto.CounterpartyId, _userContext.LanguageId));

            var linesResult = await BuildLinesAsync(orgId, dto.Lines, ct);
            if (!linesResult.IsSuccess)
                return Result.Failure<long>(linesResult.Error);

            var lines = linesResult.Value;

            var doc = new SaleDoc
            {
                OrganizationId    = orgId,
                DocNumber         = docNumber,
                DocDate           = dto.DocDate,
                CurrencyId        = dto.CurrencyId,
                SaleDocTables     = lines,
                TotalAmount       = lines.Sum(l => l.Amount),
                VatAmount         = lines.Sum(l => l.VatAmount),
                FinalAmount       = lines.Sum(l => l.TotalAmount),
                StatusId          = DocumentStatusIdConst.DRAFT,
                Comment           = dto.Comment,
                StateId           = StateIdConst.ACTIVE,
                CreatedDate       = DateTime.Now,
                WarehouseId        = dto.WarehouseId,
                CounterpartyId    = dto.CounterpartyId,
            };

            await _command.CreateAsync(doc, ct);

            var fullDocQuery = _queryBuilder.For<SaleDoc>().Where(d => d.Id == doc.Id).Build();
            fullDocQuery.AddIncludes(b => b.Include(d => d.SaleDocTables).ThenInclude(l => l.ProductTable));
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
                await _auditLogService.CreateAsync(AuditLogTableConst.SaleDoc, doc.Id.ToString(), AuditLogOperationTypeConst.Create);
            }

            return Result.Success(doc.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, SaleDocUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<SaleDoc>().Where(x => x.Id == id).Build();
            var doc   = await _query.GetAsync(query, ct);

            if (doc == null)
                return Result.Failure(SaleDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
                return Result.Failure(SaleDocErrors.AlreadyPosted(id, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            var linesResult = await BuildLinesAsync(_userContext.OrganizationId.Value, dto.Lines, ct);
            if (!linesResult.IsSuccess)
                return Result.Failure(linesResult.Error);

            var newLines = linesResult.Value;

            await _lineCommand.DeleteAsync(l => l.OwnerId == id, ct);

            foreach (var line in newLines)
                line.OwnerId = id;

            await _lineCommand.CreateAsync(newLines, ct);

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
                await _auditLogService.CreateAsync(AuditLogTableConst.SaleDoc, id.ToString(), AuditLogOperationTypeConst.Update, dto.Comment);
            }

            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            var query = _queryBuilder.For<SaleDoc>().Where(x => x.Id == id).Build();
            var doc   = await _query.GetAsync(query, ct);

            if (doc == null)
                return Result.Failure(SaleDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
                return Result.Failure(SaleDocErrors.AlreadyPosted(id, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            await _lineCommand.DeleteAsync(l => l.OwnerId == id, ct);

            doc.StateId = StateIdConst.PASSIVE;
            await _command.UpdateAsync(doc, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.SaleDoc, id.ToString(), AuditLogOperationTypeConst.Delete);
            }

            return Result.Success();
        }, ct);

    private async Task<SaleDocDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<SaleDoc>().Where(x => x.Id == id).As<SaleDocDto>().Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<Result<List<SaleDocTable>>> BuildLinesAsync(int organizationId, List<SaleDocLineDto> lineDtos, CancellationToken ct)
    {
        var lines = new List<SaleDocTable>(lineDtos.Count);

        var serialNumbers = lineDtos.Select(s => s.SerialNumber).Where(s => s != null).Distinct().ToList();
        var ptQuery = _queryBuilder.For<ProductTable>()
            .Where(x => x.OrganizationId == organizationId
                      && x.StateId == StateIdConst.ACTIVE
                      && serialNumbers.Contains(x.SerialNumber))
            .Build();
        var existingProductTables = await _productTableQuery.GetAllAsync(ptQuery);

        foreach (var lineDto in lineDtos)
        {
            var productTable = existingProductTables.FirstOrDefault(pt => pt.SerialNumber == lineDto.SerialNumber);
            if (productTable == null)
                return Result.Failure<List<SaleDocTable>>(ProductErrors.NotFound(lineDto.ProductId, _userContext.LanguageId));

            var vatAmount = 0m;
            if (lineDto.VatRateId.HasValue)
            {
                var vatQuery = _queryBuilder.For<VatRate>().Where(v => v.Id == lineDto.VatRateId.Value).Build();
                var vatRate = await _vatRateQuery.GetAsync(vatQuery, ct);

                if (vatRate == null)
                    return Result.Failure<List<SaleDocTable>>(SaleDocTableErrors.VatRateNotFound(lineDto.VatRateId.Value, _userContext.LanguageId));

                vatAmount = Math.Round(lineDto.Price * vatRate.Rate / 100, 2);
            }

            lines.Add(new SaleDocTable
            {
                Amount = lineDto.Price,
                Price = lineDto.Price,
                Quantity = 1,
                TotalAmount = lineDto.Price + vatAmount,
                VatRateId = lineDto.VatRateId,
                VatAmount = vatAmount,
                ProductTableId = productTable.Id
            });
        }

        return lines;
    }
}
