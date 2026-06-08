using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.PurchaseDocTables;

public class PurchaseDocTableService : IPurchaseDocTableService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<PurchaseDocTable> _query;
    private readonly ICommandRepository<PurchaseDocTable> _command;

    public PurchaseDocTableService(IUserContext userContext,
                                   IQueryBuilder queryBuilder, 
                                   IQueryRepository<PurchaseDocTable> query,
                                   ICommandRepository<PurchaseDocTable> command)
    {
        _query = query;
        _command = command;
        _userContext = userContext; 
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<long>> CreateAsync(PurchaseDocTableCreateDto dto, CancellationToken ct = default)
    {
        var amount = dto.Quantity * dto.Price;
        var vatAmount = 0m;
        var entity = new PurchaseDocTable
        {
            OwnerId = dto.OwnerId,
            ProductId = dto.ProductId,
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
        var query = _queryBuilder.For<PurchaseDocTable>().Where(e => e.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(PurchaseDocTableErrors.NotFound(id, _userContext.LanguageId));

        //await _command.DeleteAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<PurchaseDocTableListDto>>> GetAllAsync(PurchaseDocTableListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<PurchaseDocTable, PurchaseDocTableListDto, PurchaseDocTableListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<PurchaseDocTableDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<PurchaseDocTable>().Where(e => e.Id == id).As<PurchaseDocTableDto>().Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<PurchaseDocTableDto>(PurchaseDocTableErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(long id, PurchaseDocTableUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<PurchaseDocTable>().Where(e => e.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(PurchaseDocTableErrors.NotFound(id, _userContext.LanguageId));

        var amount = dto.Quantity * dto.Price;
        var vatAmount = 0m;
        entity.OwnerId = dto.OwnerId;
        entity.ProductId = dto.ProductId;
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
