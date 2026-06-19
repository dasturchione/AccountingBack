using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.SaleDocTables;

public class SaleDocTableService : ISaleDocTableService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<SaleDocTable> _query;
    private readonly ICommandRepository<SaleDocTable> _command;
    private readonly IQueryRepository<SaleDoc> _docQuery;
    private readonly ICommandRepository<SaleDoc> _docCommand;
    private readonly IQueryRepository<VatRate> _vatRateQuery;

    public SaleDocTableService(IUserContext userContext,
                               IQueryBuilder queryBuilder,
                               IQueryRepository<SaleDocTable> query,
                               ICommandRepository<SaleDocTable> command,
                               IQueryRepository<SaleDoc> docQuery,
                               ICommandRepository<SaleDoc> docCommand,
                               IQueryRepository<VatRate> vatRateQuery)
    {
        _query        = query;
        _command      = command;
        _userContext  = userContext;
        _queryBuilder = queryBuilder;
        _docQuery     = docQuery;
        _docCommand   = docCommand;
        _vatRateQuery = vatRateQuery;
    }

    public async Task<Result<PagedResponse<SaleDocTableListDto>>> GetAllAsync(SaleDocTableListFilter filter, CancellationToken ct = default)
    {
        var query     = _queryBuilder.BuildPaged<SaleDocTable, SaleDocTableListDto, SaleDocTableListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<SaleDocTableDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var query  = _queryBuilder.For<SaleDocTable>().Where(e => e.Id == id).As<SaleDocTableDto>().Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null)
            return Result.Failure<SaleDocTableDto>(SaleDocTableErrors.NotFound(id, _userContext.LanguageId));

        return entity;
    }

    public async Task<Result<long>> CreateAsync(SaleDocTableCreateDto dto, CancellationToken ct = default)
    {
        var docQuery = _queryBuilder.For<SaleDoc>().Where(x => x.Id == dto.OwnerId).Build();
        var doc      = await _docQuery.GetAsync(docQuery, ct);

        if (doc == null)
            return Result.Failure<long>(SaleDocTableErrors.OwnerNotFound(dto.OwnerId, _userContext.LanguageId));

        if (doc.StatusId == DocumentStatusIdConst.POSTED)
            return Result.Failure<long>(SaleDocTableErrors.OwnerAlreadyPosted(dto.OwnerId, _userContext.LanguageId));

        var (amount, vatAmount, totalAmount, error) = await CalculateAmountsAsync(dto.Quantity, dto.Price, dto.VatRateId, ct);
        if (error != null)
            return Result.Failure<long>(error);

        var entity = new SaleDocTable
        {
            OwnerId        = dto.OwnerId,
            ProductTableId = dto.ProductTableId,
            Quantity       = dto.Quantity,
            Price          = dto.Price,
            Amount         = amount,
            VatRateId      = dto.VatRateId,
            VatAmount      = vatAmount,
            TotalAmount    = totalAmount
        };

        await _command.CreateAsync(entity, ct);

        doc.TotalAmount += amount;
        doc.VatAmount   += vatAmount;
        doc.FinalAmount += totalAmount;
        await _docCommand.UpdateAsync(doc, ct);

        return entity.Id;
    }

    public async Task<Result> UpdateAsync(long id, SaleDocTableUpdateDto dto, CancellationToken ct = default)
    {
        var lineQuery = _queryBuilder.For<SaleDocTable>().Where(e => e.Id == id).Build();
        var entity    = await _query.GetAsync(lineQuery, ct);

        if (entity == null)
            return Result.Failure(SaleDocTableErrors.NotFound(id, _userContext.LanguageId));

        var docQuery = _queryBuilder.For<SaleDoc>().Where(x => x.Id == entity.OwnerId).Build();
        var doc      = await _docQuery.GetAsync(docQuery, ct);

        if (doc == null)
            return Result.Failure(SaleDocTableErrors.OwnerNotFound(entity.OwnerId, _userContext.LanguageId));

        if (doc.StatusId == DocumentStatusIdConst.POSTED)
            return Result.Failure(SaleDocTableErrors.OwnerAlreadyPosted(entity.OwnerId, _userContext.LanguageId));

        var (newAmount, newVatAmount, newTotalAmount, error) = await CalculateAmountsAsync(dto.Quantity, dto.Price, dto.VatRateId, ct);
        if (error != null)
            return Result.Failure(error);

        doc.TotalAmount += newAmount      - entity.Amount;
        doc.VatAmount   += newVatAmount   - entity.VatAmount;
        doc.FinalAmount += newTotalAmount - entity.TotalAmount;

        entity.ProductTableId = dto.ProductTableId;
        entity.Quantity    = dto.Quantity;
        entity.Price       = dto.Price;
        entity.Amount      = newAmount;
        entity.VatRateId   = dto.VatRateId;
        entity.VatAmount   = newVatAmount;
        entity.TotalAmount = newTotalAmount;

        await _command.UpdateAsync(entity, ct);
        await _docCommand.UpdateAsync(doc, ct);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct = default)
    {
        var lineQuery = _queryBuilder.For<SaleDocTable>().Where(e => e.Id == id).Build();
        var entity    = await _query.GetAsync(lineQuery, ct);

        if (entity == null)
            return Result.Failure(SaleDocTableErrors.NotFound(id, _userContext.LanguageId));

        var docQuery = _queryBuilder.For<SaleDoc>().Where(x => x.Id == entity.OwnerId).Build();
        var doc      = await _docQuery.GetAsync(docQuery, ct);

        if (doc == null)
            return Result.Failure(SaleDocTableErrors.OwnerNotFound(entity.OwnerId, _userContext.LanguageId));

        if (doc.StatusId == DocumentStatusIdConst.POSTED)
            return Result.Failure(SaleDocTableErrors.OwnerAlreadyPosted(entity.OwnerId, _userContext.LanguageId));

        doc.TotalAmount -= entity.Amount;
        doc.VatAmount   -= entity.VatAmount;
        doc.FinalAmount -= entity.TotalAmount;

        await _command.DeleteAsync(entity, ct);
        await _docCommand.UpdateAsync(doc, ct);

        return Result.Success();
    }

    private async Task<(decimal amount, decimal vatAmount, decimal totalAmount, Error? error)> CalculateAmountsAsync(
        decimal quantity, decimal price, short? vatRateId, CancellationToken ct)
    {
        var amount    = quantity * price;
        var vatAmount = 0m;

        if (vatRateId.HasValue)
        {
            var vatRateQuery = _queryBuilder.For<VatRate>().Where(v => v.Id == vatRateId.Value).Build();
            var vatRate      = await _vatRateQuery.GetAsync(vatRateQuery, ct);

            if (vatRate == null)
                return (0, 0, 0, SaleDocTableErrors.VatRateNotFound(vatRateId.Value, _userContext.LanguageId));

            vatAmount = Math.Round(amount * vatRate.Rate / 100, 2);
        }

        return (amount, vatAmount, amount + vatAmount, null);
    }
}
