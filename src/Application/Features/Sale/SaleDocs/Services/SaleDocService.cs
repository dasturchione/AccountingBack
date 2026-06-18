using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.CounterpartyCards;
using Application.Features.InventoryRegisterBalances;
using Application.Features.Register.AccountingRegisterEntries;
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
    private readonly IQueryRepository<Warehouse> _warehouseQuery;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
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
                          IQueryRepository<CounterpartyCard> counterpartyQuery,
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
        _warehouseQuery      = warehouseQuery;
        _counterpartyQuery   = counterpartyQuery;
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

            var warehouseQuery = _queryBuilder.For<Warehouse>().Where(x => x.Id == dto.WarehouseId).Build();
            var warehouse = await _warehouseQuery.GetAsync(warehouseQuery, ct);
            if (warehouse is null)
                return Result.Failure<long>(WarehouseErrors.NotFound(dto.WarehouseId, _userContext.LanguageId));

            var counterpartyQuery = _queryBuilder.For<CounterpartyCard>().Where(x => x.Id == dto.CounterpartyId).Build();
            var counterparty = await _counterpartyQuery.GetAsync(counterpartyQuery, ct);
            if (counterparty is null)
                return Result.Failure<long>(CounterpartyCardErrors.NotFound(dto.CounterpartyId, _userContext.LanguageId));

            var docNumber = await _docNumberGenerator.GenerateAsync(orgId, "SAL", dto.DocDate, ct);

            var doc = new SaleDoc
            {
                OrganizationId = orgId,
                DocNumber      = docNumber,
                DocDate        = dto.DocDate,
                CounterpartyId = dto.CounterpartyId,
                WarehouseId    = dto.WarehouseId,
                CurrencyId     = dto.CurrencyId,
                TotalAmount    = 0,
                VatAmount      = 0,
                FinalAmount    = 0,
                StatusId       = DocumentStatusIdConst.DRAFT,
                Comment        = dto.Comment,
                StateId        = StateIdConst.ACTIVE,
                CreatedDate    = DateTime.Now,
                Warehouse      = warehouse,
                Counterparty   = counterparty
            };

            await _command.CreateAsync(doc, ct);

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

            doc.DocDate        = DateTime.SpecifyKind(dto.DocDate, DateTimeKind.Unspecified);
            doc.CounterpartyId = dto.CounterpartyId;
            doc.WarehouseId    = dto.WarehouseId;
            doc.CurrencyId     = dto.CurrencyId;
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
}
