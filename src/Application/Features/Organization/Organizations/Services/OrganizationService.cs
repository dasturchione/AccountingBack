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
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<Organization> _orgQuery;
    private readonly ICommandRepository<Organization> _orgCommand;
    private readonly IFakturaService _fakturaService;
    private readonly IOrganizationManagementCore _organizationManagementCore;
    private readonly ILogger<OrganizationService> _logger;

    public OrganizationService(IUserContext userContext,
                               IQueryBuilder queryBuilder,
                               IQueryRepository<Organization> orgQuery,
                               ICommandRepository<Organization> orgCommand,
                               IFakturaService fakturaService,
                               IOrganizationManagementCore organizationManagementCore,
                               ILogger<OrganizationService> logger,
                               IUnitOfWork unitOfWork)
            : base(logger, unitOfWork)
    {
        _orgQuery = orgQuery;
        _orgCommand = orgCommand;
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _fakturaService = fakturaService;
        _organizationManagementCore = organizationManagementCore;
        _logger = logger;
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
                _logger.LogError(ex, "Failed to retrieve organization details by INN {Inn}", companyInn);
                return Result.Failure<CompanyBasicDetailsDto>(
                    OrganizationErrors.InnLookupFailed(_userContext.LanguageId));
            }
        });

    public Task<Result<int>> CreateAsync(OrganizationCreateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.TenantId is null)
                return Result.Failure<int>(CommonErrors.UserHasNoTenant(_userContext.LanguageId));

            var tenantId = _userContext.TenantId.Value;

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
                TenantId          = tenantId,
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
            var result = await _organizationManagementCore.GetOrganizationAsync(
                id,
                OrganizationManagementOptions.ForOrganization(includeDetails: true),
                ct);

            return result.IsSuccess
                ? MapOrganizationDto(result.Value)
                : Result.Failure<OrganizationDto>(result.Error);
        });

    public Task<Result> UpdateAsync(int id, OrganizationUpdateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(UpdateAsync), () =>
            _organizationManagementCore.UpdateOrganizationAsync(
                MapUpdateRequest(id, dto),
                OrganizationManagementOptions.ForOrganization(),
                ct));

    private static OrganizationDto MapOrganizationDto(Organization entity) =>
        new()
        {
            Id = entity.Id,
            ShortName = entity.ShortName,
            FullName = entity.FullName,
            Inn = entity.Inn,
            PhoneNumber = entity.PhoneNumber,
            RegionId = entity.RegionId,
            RegionName = entity.Region.FullName,
            DistrictId = entity.DistrictId,
            DistrictName = entity.District?.FullName,
            Address = entity.Address,
            Director = entity.Director,
            IsParent = entity.IsParent,
            StateId = entity.StateId,
            StateName = entity.State.FullName,
            DefaultLanguageId = entity.DefaultLanguageId,
            DefaultLanguageName = entity.DefaultLanguage?.Name,
            TenantId = entity.TenantId,
            SetupStatus = entity.SetupStatus,
            SetupCompletedAt = entity.SetupCompletedAt,
            Email = entity.Email,
            Website = entity.Website,
            Oked = entity.Oked,
            CreatedDate = entity.CreatedDate
        };

    private static OrganizationManagementUpdateRequest MapUpdateRequest(int id, OrganizationUpdateDto dto) =>
        new()
        {
            OrganizationId = id,
            ShortName = dto.ShortName,
            FullName = dto.FullName,
            Inn = dto.Inn,
            PhoneNumber = dto.PhoneNumber,
            RegionId = dto.RegionId,
            DistrictId = dto.DistrictId,
            Address = dto.Address,
            Director = dto.Director,
            IsParent = dto.IsParent,
            DefaultLanguageId = dto.DefaultLanguageId,
            TenantId = dto.TenantId,
            SetupStatus = dto.SetupStatus,
            SetupCompletedAt = dto.SetupCompletedAt,
            Email = dto.Email,
            Website = dto.Website,
            Oked = dto.Oked,
            StateId = dto.StateId
        };
}
