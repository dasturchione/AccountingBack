using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Factory;
using Application.Common.Pagination;
using Application.Options;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Organizations;

public class OrganizationService : IOrganizationService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<Organization> _orgQuery;
    private readonly ICommandRepository<Organization> _orgCommand;
    private readonly ISpecificationFactory<Organization> _orgSpecification;

    public OrganizationService(
        IUserContext userContext,
        IQueryRepository<Organization> orgQuery,
        ICommandRepository<Organization> orgCommand,
        ISpecificationFactory<Organization> orgSpecification)
    {
        _userContext      = userContext;
        _orgQuery         = orgQuery;
        _orgCommand       = orgCommand;
        _orgSpecification = orgSpecification;
    }

    public async Task<Result<int>> CreateAsync(OrganizationCreateDto dto, CancellationToken ct = default)
    {
        var exists = await _orgQuery.AnyAsync(o => o.Inn == dto.Inn, ct);
        if (exists)
            return Result.Failure<int>(OrganizationErrors.InnConflict(dto.Inn, _userContext.LanguageId));

        var org = new Organization
        {
            ShortName   = dto.ShortName,
            FullName    = dto.FullName,
            Inn         = dto.Inn,
            PhoneNumber = dto.PhoneNumber,
            RegionId    = dto.RegionId,
            DistrictId  = dto.DistrictId,
            Address     = dto.Address,
            Director    = dto.Director,
            IsParent    = dto.IsParent,
            StateId     = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };

        await _orgCommand.CreateAsync(org, ct);
        return org.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var spec   = _orgSpecification.Build(new GetByIdOptions<int>(id));
        var entity = await _orgQuery.GetAsync(spec, ct);
        if (entity == null)
            return Result.Failure(OrganizationErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;
        await _orgCommand.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<OrganizationListDto>>> GetAllAsync(OrganizationListFilter filter, CancellationToken ct = default)
    {
        var spec      = _orgSpecification.BuildPaged<OrganizationListDto, OrganizationListFilter>(filter);
        var pagedList = await _orgQuery.GetPagedAsync(spec, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<OrganizationDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var spec   = _orgSpecification.Build<OrganizationDto, GetByIdOptions<int>>(new GetByIdOptions<int>(id));
        var entity = await _orgQuery.GetAsync(spec, ct);
        if (entity == null)
            return Result.Failure<OrganizationDto>(OrganizationErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, OrganizationUpdateDto dto, CancellationToken ct = default)
    {
        var spec = _orgSpecification.Build(new GetByIdOptions<int>(id));
        var org  = await _orgQuery.GetAsync(spec, ct);
        if (org == null)
            return Result.Failure(OrganizationErrors.NotFound(id, _userContext.LanguageId));

        if (org.Inn != dto.Inn)
        {
            var exists = await _orgQuery.AnyAsync(o => o.Inn == dto.Inn, ct);
            if (exists)
                return Result.Failure(OrganizationErrors.InnConflict(dto.Inn, _userContext.LanguageId));
        }

        org.ShortName   = dto.ShortName;
        org.FullName    = dto.FullName;
        org.Inn         = dto.Inn;
        org.PhoneNumber = dto.PhoneNumber;
        org.RegionId    = dto.RegionId;
        org.DistrictId  = dto.DistrictId;
        org.Address     = dto.Address;
        org.Director    = dto.Director;
        org.IsParent    = dto.IsParent;
        org.StateId     = dto.StateId;

        await _orgCommand.UpdateAsync(org, ct);
        return Result.Success();
    }
}
