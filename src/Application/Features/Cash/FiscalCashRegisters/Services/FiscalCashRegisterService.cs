using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.FiscalCashRegisters;

public class FiscalCashRegisterService : IFiscalCashRegisterService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<FiscalCashRegister> _query;
    private readonly IQueryRepository<Warehouse> _warehouseQuery;
    private readonly IQueryRepository<FiscalCashRegisterType> _registerTypeQuery;
    private readonly ICommandRepository<FiscalCashRegister> _command;

    public FiscalCashRegisterService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<FiscalCashRegister> query,
        IQueryRepository<Warehouse> warehouseQuery,
        IQueryRepository<FiscalCashRegisterType> registerTypeQuery,
        ICommandRepository<FiscalCashRegister> command)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _query = query;
        _warehouseQuery = warehouseQuery;
        _registerTypeQuery = registerTypeQuery;
        _command = command;
    }

    public async Task<Result<int>> CreateAsync(FiscalCashRegisterCreateDto dto, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId)
            return Result.Failure<int>(FiscalCashRegisterErrors.OrganizationRequired(_userContext.LanguageId));

        var validationResult = await ValidateReferencesAndUniquenessAsync(dto, organizationId, null, ct);
        if (!validationResult.IsSuccess)
            return Result.Failure<int>(validationResult.Error);

        var entity = new FiscalCashRegister
        {
            OrganizationId = organizationId,
            WarehouseId = dto.WarehouseId,
            RegisterTypeId = dto.RegisterTypeId,
            Name = dto.Name,
            ExternalRegisterId = NormalizeOptional(dto.ExternalRegisterId),
            Model = NormalizeOptional(dto.Model),
            SerialNumber = NormalizeOptional(dto.SerialNumber),
            FiscalModuleNumber = NormalizeOptional(dto.FiscalModuleNumber),
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };

        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await GetEntityAsync(id, ct);
        if (entity is null)
            return Result.Failure(FiscalCashRegisterErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<FiscalCashRegisterListDto>>> GetAllAsync(FiscalCashRegisterListFilter filter, CancellationToken ct = default)
    {
        var page = Math.Max(filter.Page, 1);
        var pageSize = filter.PageSize.GetValueOrDefault(50);
        var search = string.IsNullOrWhiteSpace(filter.Search) ? null : filter.Search.Trim().ToLower();
        var languageId = _userContext.LanguageId ?? LanguageIdConst.UZ;

        var query = _queryBuilder.For<FiscalCashRegister>()
            .Where(x =>
                (!filter.WarehouseId.HasValue || x.WarehouseId == filter.WarehouseId.Value) &&
                (!filter.RegisterTypeId.HasValue || x.RegisterTypeId == filter.RegisterTypeId.Value) &&
                (!filter.StateId.HasValue || x.StateId == filter.StateId.Value))
            .As(ToListDto(languageId))
            .Where(x => search == null ||
                x.Name.ToLower().Contains(search) ||
                (x.ExternalRegisterId != null && x.ExternalRegisterId.ToLower().Contains(search)) ||
                (x.Model != null && x.Model.ToLower().Contains(search)) ||
                (x.SerialNumber != null && x.SerialNumber.ToLower().Contains(search)) ||
                (x.FiscalModuleNumber != null && x.FiscalModuleNumber.ToLower().Contains(search)))
            .OrderBy(x => x.OrderBy(register => register.Name).ThenBy(register => register.Id))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .BuildPaged();

        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, page, pageSize);
    }

    public async Task<Result<FiscalCashRegisterDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var languageId = _userContext.LanguageId ?? LanguageIdConst.UZ;
        var query = _queryBuilder.For<FiscalCashRegister>()
            .Where(x => x.Id == id)
            .As(ToDto(languageId))
            .Build();

        var register = await _query.GetAsync(query, ct);
        return register is null
            ? Result.Failure<FiscalCashRegisterDto>(FiscalCashRegisterErrors.NotFound(id, _userContext.LanguageId))
            : register;
    }

    public async Task<Result> UpdateAsync(int id, FiscalCashRegisterUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await GetEntityAsync(id, ct);
        if (entity is null)
            return Result.Failure(FiscalCashRegisterErrors.NotFound(id, _userContext.LanguageId));

        var validationResult = await ValidateReferencesAndUniquenessAsync(dto, entity.OrganizationId, id, ct);
        if (!validationResult.IsSuccess)
            return validationResult;

        entity.WarehouseId = dto.WarehouseId;
        entity.RegisterTypeId = dto.RegisterTypeId;
        entity.Name = dto.Name;
        entity.ExternalRegisterId = NormalizeOptional(dto.ExternalRegisterId);
        entity.Model = NormalizeOptional(dto.Model);
        entity.SerialNumber = NormalizeOptional(dto.SerialNumber);
        entity.FiscalModuleNumber = NormalizeOptional(dto.FiscalModuleNumber);
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    private async Task<FiscalCashRegister?> GetEntityAsync(int id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FiscalCashRegister>()
            .Where(x => x.Id == id)
            .Build();

        return await _query.GetAsync(query, ct);
    }

    private async Task<Result> ValidateReferencesAndUniquenessAsync(
        FiscalCashRegisterBaseDto dto,
        int organizationId,
        int? excludedId,
        CancellationToken ct)
    {
        if (dto.WarehouseId.HasValue && !await _warehouseQuery.AnyAsync(
                x => x.Id == dto.WarehouseId.Value && x.OrganizationId == organizationId,
                ct))
        {
            return Result.Failure(FiscalCashRegisterErrors.WarehouseNotFound(dto.WarehouseId.Value, _userContext.LanguageId));
        }

        if (!await _registerTypeQuery.AnyAsync(x => x.Id == dto.RegisterTypeId, ct))
            return Result.Failure(FiscalCashRegisterErrors.RegisterTypeNotFound(dto.RegisterTypeId, _userContext.LanguageId));

        var externalRegisterId = NormalizeOptional(dto.ExternalRegisterId);
        if (externalRegisterId is not null && await _query.AnyAsync(
                x => x.OrganizationId == organizationId &&
                     x.ExternalRegisterId == externalRegisterId &&
                     (!excludedId.HasValue || x.Id != excludedId.Value),
                ct))
        {
            return Result.Failure(FiscalCashRegisterErrors.ExternalRegisterIdConflict(externalRegisterId, _userContext.LanguageId));
        }

        var serialNumber = NormalizeOptional(dto.SerialNumber);
        if (serialNumber is not null && await _query.AnyAsync(
                x => x.OrganizationId == organizationId &&
                     x.SerialNumber == serialNumber &&
                     (!excludedId.HasValue || x.Id != excludedId.Value),
                ct))
        {
            return Result.Failure(FiscalCashRegisterErrors.SerialNumberConflict(serialNumber, _userContext.LanguageId));
        }

        var fiscalModuleNumber = NormalizeOptional(dto.FiscalModuleNumber);
        if (fiscalModuleNumber is not null && await _query.AnyAsync(
                x => x.OrganizationId == organizationId &&
                     x.FiscalModuleNumber == fiscalModuleNumber &&
                     (!excludedId.HasValue || x.Id != excludedId.Value),
                ct))
        {
            return Result.Failure(FiscalCashRegisterErrors.FiscalModuleNumberConflict(fiscalModuleNumber, _userContext.LanguageId));
        }

        return Result.Success();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static System.Linq.Expressions.Expression<Func<FiscalCashRegister, FiscalCashRegisterDto>> ToDto(short languageId) =>
        x => new FiscalCashRegisterDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            WarehouseId = x.WarehouseId,
            WarehouseName = x.Warehouse != null ? x.Warehouse.Name : null,
            RegisterTypeId = x.RegisterTypeId,
            RegisterTypeCode = x.RegisterType.Code,
            RegisterTypeName = x.RegisterType.FiscalCashRegisterTypeTranslations
                .Where(t => t.LanguageId == languageId)
                .Select(t => t.Name)
                .FirstOrDefault() ?? x.RegisterType.Name,
            Name = x.Name,
            ExternalRegisterId = x.ExternalRegisterId,
            Model = x.Model,
            SerialNumber = x.SerialNumber,
            FiscalModuleNumber = x.FiscalModuleNumber,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };

    private static System.Linq.Expressions.Expression<Func<FiscalCashRegister, FiscalCashRegisterListDto>> ToListDto(short languageId) =>
        x => new FiscalCashRegisterListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            WarehouseId = x.WarehouseId,
            WarehouseName = x.Warehouse != null ? x.Warehouse.Name : null,
            RegisterTypeId = x.RegisterTypeId,
            RegisterTypeCode = x.RegisterType.Code,
            RegisterTypeName = x.RegisterType.FiscalCashRegisterTypeTranslations
                .Where(t => t.LanguageId == languageId)
                .Select(t => t.Name)
                .FirstOrDefault() ?? x.RegisterType.Name,
            Name = x.Name,
            ExternalRegisterId = x.ExternalRegisterId,
            Model = x.Model,
            SerialNumber = x.SerialNumber,
            FiscalModuleNumber = x.FiscalModuleNumber,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
