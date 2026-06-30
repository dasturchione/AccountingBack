using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Integration;
using Application.Abstractions.Integration.Models;
using Application.Common.Pagination;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Organizations;

public class OrganizationService : BaseService, IOrganizationService
{
    private readonly IUserContext                     _userContext;
    private readonly IQueryBuilder                    _queryBuilder;
    private readonly IQueryRepository<Organization>   _orgQuery;
    private readonly ICommandRepository<Organization> _orgCommand;
    private readonly IFakturaService                  _fakturaService;

    public OrganizationService(IUserContext                    userContext,
                               IQueryBuilder                   queryBuilder,
                               IQueryRepository<Organization>  orgQuery,
                               ICommandRepository<Organization> orgCommand,
                               IFakturaService                 fakturaService,
                               ILogger<OrganizationService>    logger, 
                               IUnitOfWork unitOfWork) 
            : base(logger, unitOfWork)
    {
        _orgQuery       = orgQuery;
        _orgCommand     = orgCommand;
        _userContext    = userContext;
        _queryBuilder   = queryBuilder;
        _fakturaService = fakturaService;
    }

    public Task<Result<CompanyBasicDetailsDto>> GetByInnAsync(string companyInn, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByInnAsync), async () =>
        {
            try
            {
                var data = await _fakturaService.GetCompanyDataAsync(companyInn);
                return data;
            }
            catch (Exception ex)
            {
                return Result.Failure<CompanyBasicDetailsDto>(
                    Error.Problem("Organization.InnLookupFailed", ex.Message));
            }
        });

    public Task<Result<int>> CreateAsync(OrganizationCreateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(CreateAsync), async () =>
        {
            var exists = await _orgQuery.AnyAsync(o => o.Inn == dto.Inn, ct);
            if (exists)
                return Result.Failure<int>(OrganizationErrors.InnConflict(dto.Inn, _userContext.LanguageId));

            var org = new Organization
            {
                ShortName         = dto.ShortName,
                FullName          = dto.FullName,
                Inn               = dto.Inn,
                PhoneNumber       = dto.PhoneNumber,
                RegionId          = dto.RegionId,
                DistrictId        = dto.DistrictId,
                Address           = dto.Address,
                Director          = dto.Director,
                IsParent          = dto.IsParent,
                DefaultLanguageId = dto.DefaultLanguageId,
                TenantId          = dto.TenantId,
                SetupStatus       = string.IsNullOrWhiteSpace(dto.SetupStatus) ? "pending" : dto.SetupStatus,
                SetupCompletedAt  = dto.SetupCompletedAt,
                Email             = dto.Email,
                Website           = dto.Website,
                Oked              = dto.Oked,
                StateId           = StateIdConst.ACTIVE,
                CreatedDate       = DateTime.Now
            };

            await _orgCommand.CreateAsync(org, ct);
            return org.Id;
        });

    public Task<Result> DeleteAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(DeleteAsync), async () =>
        {
            var query  = _queryBuilder.For<Organization>().Where(o => o.Id == id).Build();
            var entity = await _orgQuery.GetAsync(query, ct);
            if (entity == null)
                return Result.Failure(OrganizationErrors.NotFound(id, _userContext.LanguageId));

            entity.StateId = StateIdConst.PASSIVE;
            await _orgCommand.UpdateAsync(entity, ct);
            return Result.Success();
        });

    public Task<Result<PagedResponse<OrganizationListDto>>> GetAllAsync(OrganizationListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query     = _queryBuilder.BuildPaged<Organization, OrganizationListDto, OrganizationListFilter>(filter);
            var pagedList = await _orgQuery.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<OrganizationDto>> GetByIdAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var query  = _queryBuilder.For<Organization>().Where(o => o.Id == id).As<OrganizationDto>().Build();
            var entity = await _orgQuery.GetAsync(query, ct);
            if (entity == null)
                return Result.Failure<OrganizationDto>(OrganizationErrors.NotFound(id, _userContext.LanguageId));
            return entity;
        });

    public Task<Result> UpdateAsync(int id, OrganizationUpdateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(UpdateAsync), async () =>
        {
            var query = _queryBuilder.For<Organization>().Where(o => o.Id == id).Build();
            var org   = await _orgQuery.GetAsync(query, ct);
            if (org == null)
                return Result.Failure(OrganizationErrors.NotFound(id, _userContext.LanguageId));

            if (org.Inn != dto.Inn)
            {
                var exists = await _orgQuery.AnyAsync(o => o.Inn == dto.Inn, ct);
                if (exists)
                    return Result.Failure(OrganizationErrors.InnConflict(dto.Inn, _userContext.LanguageId));
            }

            org.ShortName         = dto.ShortName;
            org.FullName          = dto.FullName;
            org.Inn               = dto.Inn;
            org.PhoneNumber       = dto.PhoneNumber;
            org.RegionId          = dto.RegionId;
            org.DistrictId        = dto.DistrictId;
            org.Address           = dto.Address;
            org.Director          = dto.Director;
            org.IsParent          = dto.IsParent;
            org.DefaultLanguageId = dto.DefaultLanguageId;
            org.TenantId          = dto.TenantId;
            org.SetupStatus       = string.IsNullOrWhiteSpace(dto.SetupStatus) ? org.SetupStatus : dto.SetupStatus;
            org.SetupCompletedAt  = dto.SetupCompletedAt;
            org.Email             = dto.Email;
            org.Website           = dto.Website;
            org.Oked              = dto.Oked;
            org.StateId           = dto.StateId;

            await _orgCommand.UpdateAsync(org, ct);
            return Result.Success();
        });
}
