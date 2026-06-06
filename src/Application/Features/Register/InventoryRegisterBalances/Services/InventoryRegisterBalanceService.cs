using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Query;
using SharedKernel.Query.Extensions;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public class InventoryRegisterBalanceService : IInventoryRegisterBalanceService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<InventoryRegisterBalance> _query;
    private readonly ICommandRepository<InventoryRegisterBalance> _command;
    private readonly IQueryBuilder<InventoryRegisterBalance> _queryBuilder;

    public InventoryRegisterBalanceService(
        IUserContext userContext,
        IQueryRepository<InventoryRegisterBalance> query,
        ICommandRepository<InventoryRegisterBalance> command,
        IQueryBuilder<InventoryRegisterBalance> queryBuilder)
    {
        _userContext = userContext;
        _query = query;
        _command = command;
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
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(InventoryRegisterBalanceErrors.NotFound(id, _userContext.LanguageId));

        await _command.DeleteAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<InventoryRegisterBalanceListDto>>> GetAllAsync(InventoryRegisterBalanceListFilter filter, CancellationToken ct = default)
    {
        var pagedList = await _query.GetPagedAsync(_queryBuilder.BuildPaged<InventoryRegisterBalanceListDto, InventoryRegisterBalanceListFilter>(filter), ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<InventoryRegisterBalanceDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById<InventoryRegisterBalance, InventoryRegisterBalanceDto>(id), ct);
        if (entity == null) return Result.Failure<InventoryRegisterBalanceDto>(InventoryRegisterBalanceErrors.NotFound(id, _userContext.LanguageId));

        return entity;
    }

    public async Task<Result> UpdateAsync(long id, InventoryRegisterBalanceUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(InventoryRegisterBalanceErrors.NotFound(id, _userContext.LanguageId));

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
