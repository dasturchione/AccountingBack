using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public class InventoryRegisterBalanceService : IInventoryRegisterBalanceService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<InventoryRegisterBalance> _query;
    private readonly ICommandRepository<InventoryRegisterBalance> _command;

    public InventoryRegisterBalanceService(IUserContext userContext,
                                           IQueryBuilder queryBuilder,
                                           IQueryRepository<InventoryRegisterBalance> query,
                                           ICommandRepository<InventoryRegisterBalance> command)
    {
        _query = query;
        _command = command;
        _userContext = userContext;
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<long>> CreateAsync(InventoryRegisterBalanceCreateDto dto, CancellationToken ct = default)
    {
        var entity = new InventoryRegisterBalance
        {
            OrganizationId = dto.OrganizationId,
            DocumentTypeId = dto.DocumentTypeId,
            DocumentId = dto.DocumentId,
            WarehouseId = dto.WarehouseId,
            ProductId = dto.ProductId,
            OperationTypeId = dto.OperationTypeId,
            Quantity = dto.Quantity,
            Amount = dto.Amount,
            DocDate = dto.DocDate,
            CreatedDate = DateTime.Now
        };

        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<InventoryRegisterBalance>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(InventoryRegisterBalanceErrors.NotFound(id, _userContext.LanguageId));

        //await _command.DeleteAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<InventoryRegisterBalanceListDto>>> GetAllAsync(InventoryRegisterBalanceListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<InventoryRegisterBalance, InventoryRegisterBalanceListDto, InventoryRegisterBalanceListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<InventoryRegisterBalanceDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<InventoryRegisterBalance>().Where(x => x.Id == id).As<InventoryRegisterBalanceDto>().Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<InventoryRegisterBalanceDto>(InventoryRegisterBalanceErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(long id, InventoryRegisterBalanceUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<InventoryRegisterBalance>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(InventoryRegisterBalanceErrors.NotFound(id, _userContext.LanguageId));

        entity.OrganizationId = dto.OrganizationId;
        entity.DocumentTypeId = dto.DocumentTypeId;
        entity.DocumentId = dto.DocumentId;
        entity.WarehouseId = dto.WarehouseId;
        entity.ProductId = dto.ProductId;
        entity.OperationTypeId = dto.OperationTypeId;
        entity.Quantity = dto.Quantity;
        entity.Amount = dto.Amount;
        entity.DocDate = dto.DocDate;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
