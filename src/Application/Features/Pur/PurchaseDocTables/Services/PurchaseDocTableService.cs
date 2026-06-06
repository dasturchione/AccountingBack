using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Query;
using SharedKernel.Query.Extensions;
using SharedKernel.Results;

namespace Application.Features.PurchaseDocTables;

public class PurchaseDocTableService : IPurchaseDocTableService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<PurchaseDocTable> _query;
    private readonly ICommandRepository<PurchaseDocTable> _command;
    private readonly IQueryBuilder<PurchaseDocTable> _queryBuilder;

    public PurchaseDocTableService(IUserContext userContext, IQueryRepository<PurchaseDocTable> query,
        ICommandRepository<PurchaseDocTable> command, IQueryBuilder<PurchaseDocTable> queryBuilder)
    {
        _userContext = userContext; _query = query; _command = command; _queryBuilder = queryBuilder;
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
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(PurchaseDocTableErrors.NotFound(id, _userContext.LanguageId));
        await _command.DeleteAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<PurchaseDocTableListDto>>> GetAllAsync(PurchaseDocTableListFilter filter, CancellationToken ct = default)
    {
        var pagedList = await _query.GetPagedAsync(_queryBuilder.BuildPaged<PurchaseDocTableListDto, PurchaseDocTableListFilter>(filter), ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<PurchaseDocTableDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById<PurchaseDocTable, PurchaseDocTableDto>(id), ct);
        if (entity == null) return Result.Failure<PurchaseDocTableDto>(PurchaseDocTableErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(long id, PurchaseDocTableUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(PurchaseDocTableErrors.NotFound(id, _userContext.LanguageId));

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
