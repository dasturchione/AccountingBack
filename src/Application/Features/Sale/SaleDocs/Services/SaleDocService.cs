using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.SaleDocs;

public class SaleDocService : ISaleDocService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<SaleDoc> _query;
    private readonly ICommandRepository<SaleDoc> _command;

    public SaleDocService(IUserContext userContext,
                          IQueryBuilder queryBuilder, 
                          IQueryRepository<SaleDoc> query,
                          ICommandRepository<SaleDoc> command)
    {
        _query = query;
        _command = command;
        _userContext = userContext; 
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<long>> CreateAsync(SaleDocCreateDto dto, CancellationToken ct = default)
    {
        if (await _query.AnyAsync(x => x.OrganizationId == dto.OrganizationId && x.DocNumber == dto.DocNumber, ct))
            return Result.Failure<long>(SaleDocErrors.DocNumberConflict(dto.DocNumber, _userContext.LanguageId));

        var entity = new SaleDoc
        {
            OrganizationId = dto.OrganizationId,
            DocNumber = dto.DocNumber,
            DocDate = dto.DocDate,
            CounterpartyId = dto.CounterpartyId,
            WarehouseId = dto.WarehouseId,
            CurrencyId = dto.CurrencyId,
            TotalAmount = 0,
            VatAmount = 0,
            FinalAmount = 0,
            StatusId = dto.StatusId,
            Comment = dto.Comment,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };
        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<SaleDoc>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(SaleDocErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;
        
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<SaleDocListDto>>> GetAllAsync(SaleDocListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<SaleDoc, SaleDocListDto, SaleDocListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<SaleDocDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<SaleDoc>().Where(x => x.Id == id).As<SaleDocDto>().Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<SaleDocDto>(SaleDocErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(long id, SaleDocUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<SaleDoc>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(SaleDocErrors.NotFound(id, _userContext.LanguageId));

        if (entity.DocNumber != dto.DocNumber && await _query.AnyAsync(x => x.OrganizationId == dto.OrganizationId && x.DocNumber == dto.DocNumber, ct))
            return Result.Failure(SaleDocErrors.DocNumberConflict(dto.DocNumber, _userContext.LanguageId));

        entity.OrganizationId = dto.OrganizationId;
        entity.DocNumber = dto.DocNumber;
        entity.DocDate = dto.DocDate;
        entity.CounterpartyId = dto.CounterpartyId;
        entity.WarehouseId = dto.WarehouseId;
        entity.CurrencyId = dto.CurrencyId;
        entity.StatusId = dto.StatusId;
        entity.Comment = dto.Comment;
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
