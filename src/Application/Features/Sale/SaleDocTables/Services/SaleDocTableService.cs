using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.SaleDocTables;

public class SaleDocTableService : ISaleDocTableService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<SaleDocTable> _query;
    private readonly ICommandRepository<SaleDocTable> _command;

    public SaleDocTableService(IUserContext userContext,
                               IQueryBuilder queryBuilder, 
                               IQueryRepository<SaleDocTable> query,
                               ICommandRepository<SaleDocTable> command)
    {
        _query = query;
        _command = command;
        _userContext = userContext; 
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<long>> CreateAsync(SaleDocTableCreateDto dto, CancellationToken ct = default)
    {
        var amount = dto.Quantity * dto.Price;
        var vatAmount = 0m;
        var entity = new SaleDocTable
        {
            OwnerId = dto.OwnerId,
            ProductTableId = dto.ProductTableId,
            Quantity = dto.Quantity,
            Price = dto.Price,
            Amount = amount,
            VatRateId = dto.VatRateId,
            VatAmount = vatAmount,
            TotalAmount = amount + vatAmount
        };
        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<SaleDocTable>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(SaleDocTableErrors.NotFound(id, _userContext.LanguageId));

        //await _command.DeleteAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<SaleDocTableListDto>>> GetAllAsync(SaleDocTableListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<SaleDocTable, SaleDocTableListDto, SaleDocTableListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<SaleDocTableDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<SaleDocTable>().Where(x => x.Id == id).As<SaleDocTableDto>().Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<SaleDocTableDto>(SaleDocTableErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(long id, SaleDocTableUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<SaleDocTable>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(SaleDocTableErrors.NotFound(id, _userContext.LanguageId));

        var amount = dto.Quantity * dto.Price;
        var vatAmount = 0m;
        entity.OwnerId = dto.OwnerId;
        entity.ProductTableId = dto.ProductTableId;
        entity.Quantity = dto.Quantity;
        entity.Price = dto.Price;
        entity.Amount = amount;
        entity.VatRateId = dto.VatRateId;
        entity.VatAmount = vatAmount;
        entity.TotalAmount = amount + vatAmount;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
