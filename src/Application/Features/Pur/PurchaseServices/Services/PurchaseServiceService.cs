using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.PurchaseServices;

public class PurchaseServiceService : BaseService, IPurchaseServiceService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<PurchaseService> _query;
    private readonly IQueryRepository<PurchaseServiceType> _serviceTypeQuery;
    private readonly ICommandRepository<PurchaseService> _command;

    public PurchaseServiceService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<PurchaseService> query,
        IQueryRepository<PurchaseServiceType> serviceTypeQuery,
        ICommandRepository<PurchaseService> command,
        ILogger<PurchaseServiceService> logger,
        IUnitOfWork unitOfWork) : base(logger, unitOfWork)
    {
        _userContext      = userContext;
        _queryBuilder     = queryBuilder;
        _query            = query;
        _serviceTypeQuery = serviceTypeQuery;
        _command          = command;
    }

    public Task<Result<PagedResponse<PurchaseServiceListDto>>> GetAllAsync(PurchaseServiceListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<PurchaseService, PurchaseServiceListDto, PurchaseServiceListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<PurchaseServiceDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var query = _queryBuilder.For<PurchaseService>().Where(x => x.Id == id).As<PurchaseServiceDto>().Build();
            var entity = await _query.GetAsync(query, ct);
            if (entity == null)
                return Result.Failure<PurchaseServiceDto>(PurchaseServiceErrors.NotFound(id, _userContext.LanguageId));

            return entity;
        });

    public Task<Result<long>> CreateAsync(PurchaseServiceCreateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(CreateAsync), async () =>
        {
            var serviceTypeExists = await _serviceTypeQuery.AnyAsync(x => x.Id == dto.ServiceTypeId, ct);
            if (!serviceTypeExists)
                return Result.Failure<long>(PurchaseServiceErrors.ServiceTypeNotFound(dto.ServiceTypeId, _userContext.LanguageId));

            var entity = new PurchaseService
            {
                Name          = dto.Name,
                Description   = dto.Description,
                ServiceTypeId = dto.ServiceTypeId,
                StateId       = StateIdConst.ACTIVE,
                CreatedDate   = DateTime.Now
            };

            await _command.CreateAsync(entity, ct);
            return entity.Id;
        });

    public Task<Result> UpdateAsync(long id, PurchaseServiceUpdateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(UpdateAsync), async () =>
        {
            var query = _queryBuilder.For<PurchaseService>().Where(x => x.Id == id).Build();
            var entity = await _query.GetAsync(query, ct);
            if (entity == null)
                return Result.Failure(PurchaseServiceErrors.NotFound(id, _userContext.LanguageId));

            var serviceTypeExists = await _serviceTypeQuery.AnyAsync(x => x.Id == dto.ServiceTypeId, ct);
            if (!serviceTypeExists)
                return Result.Failure(PurchaseServiceErrors.ServiceTypeNotFound(dto.ServiceTypeId, _userContext.LanguageId));

            entity.Name          = dto.Name;
            entity.Description   = dto.Description;
            entity.ServiceTypeId = dto.ServiceTypeId;
            entity.StateId       = dto.StateId;

            await _command.UpdateAsync(entity, ct);
            return Result.Success();
        });

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(DeleteAsync), async () =>
        {
            var query = _queryBuilder.For<PurchaseService>().Where(x => x.Id == id).Build();
            var entity = await _query.GetAsync(query, ct);
            if (entity == null)
                return Result.Failure(PurchaseServiceErrors.NotFound(id, _userContext.LanguageId));

            entity.StateId = StateIdConst.PASSIVE;
            await _command.UpdateAsync(entity, ct);
            return Result.Success();
        });
}
