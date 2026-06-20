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
    private readonly ICommandRepository<PurchaseDocTable> _lineCommand;
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
                              ICommandRepository<PurchaseDocTable> lineCommand,
                              ILogger<PurchaseDocService> logger,
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

            // Barcha qatorlar uchun QQS ni oldindan hisoblaymiz
            var linesResult = await BuildLinesAsync(_userContext.OrganizationId.Value, dto.Lines, ct);
            if (!linesResult.IsSuccess)
                return Result.Failure<long>(linesResult.Error);

            var lines = linesResult.Value;

            var doc = new PurchaseDoc
            {
                OrganizationId      = _userContext.OrganizationId.Value,
                DocNumber           = docNumber,
                DocDate             = dto.DocDate,
                CurrencyId          = dto.CurrencyId,
                PurchaseDocTables   = lines,
                TotalAmount         = lines.Sum(l => l.Amount),
                VatAmount           = lines.Sum(l => l.VatAmount),
                FinalAmount         = lines.Sum(l => l.TotalAmount),
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
            fullDocQuery.AddIncludes(b => b.Include(d => d.PurchaseDocTables).ThenInclude(l => l.ProductTable));
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

            // Yangi qatorlarni hisoblaymiz
            var linesResult = await BuildLinesAsync(_userContext.OrganizationId.Value, dto.Lines, ct);
            if (!linesResult.IsSuccess)
                return Result.Failure(linesResult.Error);

            var newLines = linesResult.Value;

            // Eski qatorlarni o'chirib, yangilarini yozamiz
            await _lineCommand.DeleteAsync(l => l.OwnerId == id, ct);

            foreach (var line in newLines)
                line.OwnerId = id;

            await _lineCommand.CreateAsync(newLines, ct);

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
            await _lineCommand.DeleteAsync(l => l.OwnerId == id, ct);

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

    // dto.Lines dan PurchaseDocTable entity larini yaratib beradi
    // VatRate DB dan olinadi — agar topilmasa xato qaytaradi
    private async Task<Result<List<PurchaseDocTable>>> BuildLinesAsync(int organizationid, List<PurchaseDocLineDto> lineDtos, CancellationToken ct)
    {
        var lines = new List<PurchaseDocTable>(lineDtos.Count);

        foreach (var productsGroup in lineDtos.GroupBy(g => g.ProductId))
        {
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
                    ProductId = productsGroup.Key,
                    SerialNumber = s.SerialNumber,
                    MarkingNumber = s.MarkingNumber,
                    CreatedDate = DateTime.Now,
                    OrganizationId = organizationid,
                    StateId = StateIdConst.ACTIVE,
                    StatusId = ProductTableStatusIdConst.IN_STOCK
                }
            }));
        }

        return lines;
    }
}
