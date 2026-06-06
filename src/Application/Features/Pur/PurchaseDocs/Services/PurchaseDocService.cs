using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Extensions;
using SharedKernel.Results;

namespace Application.Features.PurchaseDocs;

public class PurchaseDocService : IPurchaseDocService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<PurchaseDoc> _query;
    private readonly ICommandRepository<PurchaseDoc> _command;
    private readonly IQueryBuilder<PurchaseDoc> _queryBuilder;

    public PurchaseDocService(IUserContext userContext, IQueryRepository<PurchaseDoc> query,
        ICommandRepository<PurchaseDoc> command, IQueryBuilder<PurchaseDoc> queryBuilder)
    {
        _userContext = userContext; _query = query; _command = command; _queryBuilder = queryBuilder;
    }

    public async Task<Result<long>> CreateAsync(PurchaseDocCreateDto dto, CancellationToken ct = default)
    {
        if (await _query.AnyAsync(x => x.OrganizationId == dto.OrganizationId && x.DocNumber == dto.DocNumber, ct))
            return Result.Failure<long>(PurchaseDocErrors.DocNumberConflict(dto.DocNumber, _userContext.LanguageId));

        var entity = new PurchaseDoc
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
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(PurchaseDocErrors.NotFound(id, _userContext.LanguageId));
        entity.StateId = StateIdConst.PASSIVE;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<PurchaseDocListDto>>> GetAllAsync(PurchaseDocListFilter filter, CancellationToken ct = default)
    {
        var pagedList = await _query.GetPagedAsync(_queryBuilder.BuildPaged<PurchaseDocListDto, PurchaseDocListFilter>(filter), ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<PurchaseDocDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById<PurchaseDoc, PurchaseDocDto>(id), ct);
        if (entity == null) return Result.Failure<PurchaseDocDto>(PurchaseDocErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(long id, PurchaseDocUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(PurchaseDocErrors.NotFound(id, _userContext.LanguageId));

        if (entity.DocNumber != dto.DocNumber && await _query.AnyAsync(x => x.OrganizationId == dto.OrganizationId && x.DocNumber == dto.DocNumber, ct))
            return Result.Failure(PurchaseDocErrors.DocNumberConflict(dto.DocNumber, _userContext.LanguageId));

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
