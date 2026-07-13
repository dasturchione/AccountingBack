using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;

namespace Application.Features.Didox;

/// <summary>Application boundary for Didox reference data and organization-scoped profiles.</summary>
public sealed class DidoxPersistenceService :
    IDidoxMxikCatalogService, IDidoxOriginService, IDidoxVatRegStatusService,
    IUnitDidoxPackageService, ICounterpartyDidoxProfileService,
    IProductDidoxProfileService, IProductTableDidoxOriginService
{
    private readonly IUserContext _user;
    private readonly IUnitOfWork _uow;
    private readonly IQueryRepository<DidoxMxikCatalog> _mxikQuery;
    private readonly ITrackingRepository<DidoxMxikCatalog> _mxikWrite;
    private readonly IQueryRepository<DidoxOrigin> _originQuery;
    private readonly ITrackingRepository<DidoxOrigin> _originWrite;
    private readonly IQueryRepository<DidoxVatRegStatus> _vatStatusQuery;
    private readonly ITrackingRepository<DidoxVatRegStatus> _vatStatusWrite;
    private readonly IQueryRepository<UnitDidoxPackage> _packageQuery;
    private readonly ITrackingRepository<UnitDidoxPackage> _packageWrite;
    private readonly IQueryRepository<CounterpartyDidoxProfile> _counterpartyProfileQuery;
    private readonly ITrackingRepository<CounterpartyDidoxProfile> _counterpartyProfileWrite;
    private readonly IQueryRepository<ProductDidoxProfile> _productProfileQuery;
    private readonly ITrackingRepository<ProductDidoxProfile> _productProfileWrite;
    private readonly IQueryRepository<ProductTableDidoxOrigin> _tableOriginQuery;
    private readonly ITrackingRepository<ProductTableDidoxOrigin> _tableOriginWrite;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
    private readonly IQueryRepository<Product> _productQuery;
    private readonly IQueryRepository<ProductTable> _productTableQuery;
    private readonly IQueryRepository<Unit> _unitQuery;
    private readonly IOrganizationSourceReader _organizationSourceReader;

    public DidoxPersistenceService(
        IUserContext user, IUnitOfWork uow,
        IQueryRepository<DidoxMxikCatalog> mxikQuery, ITrackingRepository<DidoxMxikCatalog> mxikWrite,
        IQueryRepository<DidoxOrigin> originQuery, ITrackingRepository<DidoxOrigin> originWrite,
        IQueryRepository<DidoxVatRegStatus> vatStatusQuery, ITrackingRepository<DidoxVatRegStatus> vatStatusWrite,
        IQueryRepository<UnitDidoxPackage> packageQuery, ITrackingRepository<UnitDidoxPackage> packageWrite,
        IQueryRepository<CounterpartyDidoxProfile> counterpartyProfileQuery, ITrackingRepository<CounterpartyDidoxProfile> counterpartyProfileWrite,
        IQueryRepository<ProductDidoxProfile> productProfileQuery, ITrackingRepository<ProductDidoxProfile> productProfileWrite,
        IQueryRepository<ProductTableDidoxOrigin> tableOriginQuery, ITrackingRepository<ProductTableDidoxOrigin> tableOriginWrite,
        IQueryRepository<CounterpartyCard> counterpartyQuery, IQueryRepository<Product> productQuery,
        IQueryRepository<ProductTable> productTableQuery, IQueryRepository<Unit> unitQuery,
        IOrganizationSourceReader organizationSourceReader)
    {
        _user = user; _uow = uow; _mxikQuery = mxikQuery; _mxikWrite = mxikWrite;
        _originQuery = originQuery; _originWrite = originWrite; _vatStatusQuery = vatStatusQuery; _vatStatusWrite = vatStatusWrite;
        _packageQuery = packageQuery; _packageWrite = packageWrite; _counterpartyProfileQuery = counterpartyProfileQuery;
        _counterpartyProfileWrite = counterpartyProfileWrite; _productProfileQuery = productProfileQuery; _productProfileWrite = productProfileWrite;
        _tableOriginQuery = tableOriginQuery; _tableOriginWrite = tableOriginWrite; _counterpartyQuery = counterpartyQuery;
        _productQuery = productQuery; _productTableQuery = productTableQuery; _unitQuery = unitQuery;
        _organizationSourceReader = organizationSourceReader;
    }

    public Task<Result<long>> CreateAsync(DidoxMxikCatalogWriteDto dto, CancellationToken ct = default) =>
        InTransactionAsync(async () =>
        {
            var valid = ValidateDates(dto.EffectiveFrom, dto.EffectiveTo); if (!valid.IsSuccess) return Result.Failure<long>(valid.Error);
            if (await HasMxikOverlapAsync(dto.MxikCode, dto.EffectiveFrom, dto.EffectiveTo, null, ct)) return Result.Failure<long>(DidoxErrors.OverlappingEffectiveDates());
            var entity = ToMxik(dto); await _mxikWrite.AddAsync(entity, ct); return Result.Success(entity.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, DidoxMxikCatalogWriteDto dto, CancellationToken ct = default) =>
        InTransactionAsync(async () =>
        {
            var entity = await GetOneAsync(_mxikQuery, x => x.Id == id, ct); if (entity is null) return Result.Failure(DidoxErrors.NotFound("MXIK catalog item"));
            var valid = ValidateDates(dto.EffectiveFrom, dto.EffectiveTo); if (!valid.IsSuccess) return valid;
            if (await HasMxikOverlapAsync(dto.MxikCode, dto.EffectiveFrom, dto.EffectiveTo, id, ct)) return Result.Failure(DidoxErrors.OverlappingEffectiveDates());
            Apply(entity, dto); await _mxikWrite.UpdateAsync(entity, ct); return Result.Success();
        }, ct);

    public async Task<Result<DidoxMxikCatalogDto>> GetByCodeAsync(string mxikCode, CancellationToken ct = default)
    {
        var item = await GetOneAsync(_mxikQuery, x => x.MxikCode == mxikCode, ct);
        return item is null ? Result.Failure<DidoxMxikCatalogDto>(DidoxErrors.NotFound("MXIK code")) : Result.Success(ToDto(item));
    }

    public async Task<Result<DidoxMxikCatalogDto>> GetAsOfAsync(string mxikCode, DateOnly date, CancellationToken ct = default)
    {
        var item = (await _mxikQuery.GetAllAsync(new QuerySpecification<DidoxMxikCatalog> { Criteria = x => x.MxikCode == mxikCode && x.EffectiveFrom <= date && (!x.EffectiveTo.HasValue || x.EffectiveTo >= date), OrderBy = x => x.OrderByDescending(i => i.EffectiveFrom) }, ct)).FirstOrDefault();
        return item is null ? Result.Failure<DidoxMxikCatalogDto>(DidoxErrors.SourceMissing($"MXIK catalog '{mxikCode}' as of {date:yyyy-MM-dd}")) : Result.Success(ToDto(item));
    }

    public Task<Result<DidoxImportResultDto>> ImportAsync(DidoxMxikCatalogImportCommand command, CancellationToken ct = default) =>
        InTransactionAsync(async () =>
        {
            if (command.Items.Count == 0) return Result.Failure<DidoxImportResultDto>(DidoxErrors.EmptyImport());
            var created = 0; var updated = 0;
            foreach (var item in command.Items)
            {
                var valid = ValidateDates(item.EffectiveFrom, item.EffectiveTo); if (!valid.IsSuccess) return Result.Failure<DidoxImportResultDto>(valid.Error);
                var existing = await GetOneAsync(_mxikQuery, x => x.MxikCode == item.MxikCode && x.EffectiveFrom == item.EffectiveFrom, ct);
                if (await HasMxikOverlapAsync(item.MxikCode, item.EffectiveFrom, item.EffectiveTo, existing?.Id, ct)) return Result.Failure<DidoxImportResultDto>(DidoxErrors.OverlappingEffectiveDates());
                if (existing is null) { await _mxikWrite.AddAsync(ToMxik(item), ct); created++; }
                else { Apply(existing, item); await _mxikWrite.UpdateAsync(existing, ct); updated++; }
            }
            return Result.Success(new DidoxImportResultDto { CreatedCount = created, UpdatedCount = updated });
        }, ct);

    public Task<Result<short>> CreateAsync(DidoxReferenceWriteDto dto, CancellationToken ct = default) =>
        CreateOriginAsync(dto, ct);
    private Task<Result<short>> CreateOriginAsync(DidoxReferenceWriteDto dto, CancellationToken ct) => InTransactionAsync(async () =>
    {
        if (await _originQuery.AnyAsync(x => x.Code == dto.Code, ct)) return Result.Failure<short>(DidoxErrors.Duplicate("origin code"));
        await _originWrite.AddAsync(new DidoxOrigin { Code = dto.Code, Name = dto.Name, StateId = dto.StateId, CreatedDate = DateTime.Now }, ct); return Result.Success(dto.Code);
    }, ct);

    Task<Result<short>> IDidoxVatRegStatusService.CreateAsync(DidoxReferenceWriteDto dto, CancellationToken ct) => InTransactionAsync(async () =>
    {
        if (await _vatStatusQuery.AnyAsync(x => x.Code == dto.Code, ct)) return Result.Failure<short>(DidoxErrors.Duplicate("VAT registration status code"));
        await _vatStatusWrite.AddAsync(new DidoxVatRegStatus { Code = dto.Code, Name = dto.Name, StateId = dto.StateId, CreatedDate = DateTime.Now }, ct); return Result.Success(dto.Code);
    }, ct);

    public async Task<Result<IReadOnlyCollection<DidoxReferenceDto>>> ListAsync(CancellationToken ct = default) =>
        Result.Success<IReadOnlyCollection<DidoxReferenceDto>>((await _originQuery.GetAllAsync(new QuerySpecification<DidoxOrigin> { Criteria = _ => true, OrderBy = q => q.OrderBy(x => x.Code) }, ct)).Select(ToDto).ToList());
    async Task<Result<IReadOnlyCollection<DidoxReferenceDto>>> IDidoxVatRegStatusService.ListAsync(CancellationToken ct) =>
        Result.Success<IReadOnlyCollection<DidoxReferenceDto>>((await _vatStatusQuery.GetAllAsync(new QuerySpecification<DidoxVatRegStatus> { Criteria = _ => true, OrderBy = q => q.OrderBy(x => x.Code) }, ct)).Select(ToDto).ToList());
    public async Task<Result<DidoxReferenceDto>> GetByCodeAsync(short code, CancellationToken ct = default)
    {
        var item = await GetOneAsync(_originQuery, x => x.Code == code, ct); return item is null ? Result.Failure<DidoxReferenceDto>(DidoxErrors.NotFound("origin code")) : Result.Success(ToDto(item));
    }
    async Task<Result<DidoxReferenceDto>> IDidoxVatRegStatusService.GetByCodeAsync(short code, CancellationToken ct)
    {
        var item = await GetOneAsync(_vatStatusQuery, x => x.Code == code, ct); return item is null ? Result.Failure<DidoxReferenceDto>(DidoxErrors.NotFound("VAT registration status code")) : Result.Success(ToDto(item));
    }
    public async Task<Result> ValidateForEffectiveDateAsync(short code, DateOnly date, CancellationToken ct = default) =>
        await _originQuery.AnyAsync(x => x.Code == code, ct) ? Result.Success() : Result.Failure(DidoxErrors.SourceMissing($"origin '{code}' for {date:yyyy-MM-dd}"));

    public Task<Result<long>> CreateAsync(UnitDidoxPackageWriteDto dto, CancellationToken ct = default) => InTransactionAsync(async () =>
    {
        var valid = ValidateDates(dto.EffectiveFrom, dto.EffectiveTo); if (!valid.IsSuccess) return Result.Failure<long>(valid.Error);
        if (!await _unitQuery.AnyAsync(x => x.Id == dto.UnitId, ct)) return Result.Failure<long>(DidoxErrors.SourceMissing($"unit '{dto.UnitId}'"));
        if (await HasPackageOverlapAsync(dto.UnitId, dto.EffectiveFrom, dto.EffectiveTo, null, ct)) return Result.Failure<long>(DidoxErrors.OverlappingEffectiveDates());
        var item = ToPackage(dto); await _packageWrite.AddAsync(item, ct); return Result.Success(item.Id);
    }, ct);
    public Task<Result> UpdateAsync(long id, UnitDidoxPackageWriteDto dto, CancellationToken ct = default) => InTransactionAsync(async () =>
    {
        var item = await GetOneAsync(_packageQuery, x => x.Id == id, ct); if (item is null) return Result.Failure(DidoxErrors.NotFound("unit package mapping"));
        var valid = ValidateDates(dto.EffectiveFrom, dto.EffectiveTo); if (!valid.IsSuccess) return valid;
        if (!await _unitQuery.AnyAsync(x => x.Id == dto.UnitId, ct)) return Result.Failure(DidoxErrors.SourceMissing($"unit '{dto.UnitId}'"));
        if (await HasPackageOverlapAsync(dto.UnitId, dto.EffectiveFrom, dto.EffectiveTo, id, ct)) return Result.Failure(DidoxErrors.OverlappingEffectiveDates());
        Apply(item, dto); await _packageWrite.UpdateAsync(item, ct); return Result.Success();
    }, ct);
    public async Task<Result<IReadOnlyCollection<UnitDidoxPackageDto>>> ListAsync(short? unitId = null, CancellationToken ct = default) => Result.Success<IReadOnlyCollection<UnitDidoxPackageDto>>((await _packageQuery.GetAllAsync(new QuerySpecification<UnitDidoxPackage> { Criteria = x => !unitId.HasValue || x.UnitId == unitId.Value, OrderBy = q => q.OrderBy(x => x.UnitId).ThenBy(x => x.EffectiveFrom) }, ct)).Select(ToDto).ToList());
    public async Task<Result<UnitDidoxPackageDto>> GetAsOfAsync(short unitId, DateOnly date, CancellationToken ct = default)
    {
        var item = (await _packageQuery.GetAllAsync(new QuerySpecification<UnitDidoxPackage> { Criteria = x => x.UnitId == unitId && x.EffectiveFrom <= date && (!x.EffectiveTo.HasValue || x.EffectiveTo >= date), OrderBy = q => q.OrderByDescending(x => x.EffectiveFrom) }, ct)).FirstOrDefault();
        return item is null ? Result.Failure<UnitDidoxPackageDto>(DidoxErrors.SourceMissing($"Didox package for unit '{unitId}' as of {date:yyyy-MM-dd}")) : Result.Success(ToDto(item));
    }

    public Task<Result<long>> CreateAsync(CounterpartyDidoxProfileWriteDto dto, CancellationToken ct = default) => InTransactionAsync(async () =>
    {
        var org = RequireOrganization(); if (!org.IsSuccess) return Result.Failure<long>(org.Error); var valid = ValidateDates(dto.EffectiveFrom, dto.EffectiveTo); if (!valid.IsSuccess) return Result.Failure<long>(valid.Error);
        var guard = await GuardCounterpartyAsync(dto.CounterpartyId, org.Value, ct); if (!guard.IsSuccess) return Result.Failure<long>(guard.Error);
        if (!await _vatStatusQuery.AnyAsync(x => x.Code == dto.VatRegStatusCode, ct)) return Result.Failure<long>(DidoxErrors.SourceMissing($"VAT registration status '{dto.VatRegStatusCode}'"));
        if (await HasCounterpartyOverlapAsync(org.Value, dto.CounterpartyId, dto.EffectiveFrom, dto.EffectiveTo, null, ct)) return Result.Failure<long>(DidoxErrors.OverlappingEffectiveDates());
        var item = ToCounterpartyProfile(dto, org.Value); await _counterpartyProfileWrite.AddAsync(item, ct); return Result.Success(item.Id);
    }, ct);
    public Task<Result> UpdateAsync(long id, CounterpartyDidoxProfileWriteDto dto, CancellationToken ct = default) => InTransactionAsync(async () =>
    {
        var org = RequireOrganization(); if (!org.IsSuccess) return Result.Failure(org.Error); var item = await GetOneAsync(_counterpartyProfileQuery, x => x.Id == id, ct); if (item is null) return Result.Failure(DidoxErrors.NotFound("counterparty profile")); if (item.OrganizationId != org.Value) return Result.Failure(DidoxErrors.OrganizationMismatch("counterparty profile"));
        var valid = ValidateDates(dto.EffectiveFrom, dto.EffectiveTo); if (!valid.IsSuccess) return valid; var guard = await GuardCounterpartyAsync(dto.CounterpartyId, org.Value, ct); if (!guard.IsSuccess) return guard;
        if (!await _vatStatusQuery.AnyAsync(x => x.Code == dto.VatRegStatusCode, ct)) return Result.Failure(DidoxErrors.SourceMissing($"VAT registration status '{dto.VatRegStatusCode}'"));
        if (await HasCounterpartyOverlapAsync(org.Value, dto.CounterpartyId, dto.EffectiveFrom, dto.EffectiveTo, id, ct)) return Result.Failure(DidoxErrors.OverlappingEffectiveDates()); Apply(item, dto); await _counterpartyProfileWrite.UpdateAsync(item, ct); return Result.Success();
    }, ct);
    public async Task<Result<IReadOnlyCollection<CounterpartyDidoxProfileDto>>> ListByCounterpartyAsync(int? counterpartyId = null, CancellationToken ct = default)
    {
        var org = RequireOrganization(); if (!org.IsSuccess) return Result.Failure<IReadOnlyCollection<CounterpartyDidoxProfileDto>>(org.Error); return Result.Success<IReadOnlyCollection<CounterpartyDidoxProfileDto>>((await _counterpartyProfileQuery.GetAllAsync(new QuerySpecification<CounterpartyDidoxProfile> { Criteria = x => x.OrganizationId == org.Value && (!counterpartyId.HasValue || x.CounterpartyId == counterpartyId.Value), OrderBy = q => q.OrderBy(x => x.CounterpartyId).ThenBy(x => x.EffectiveFrom) }, ct)).Select(ToDto).ToList());
    }

    public Task<Result<long>> CreateAsync(ProductDidoxProfileWriteDto dto, CancellationToken ct = default) => InTransactionAsync(async () =>
    {
        var org = RequireOrganization(); if (!org.IsSuccess) return Result.Failure<long>(org.Error); var valid = ValidateDates(dto.EffectiveFrom, dto.EffectiveTo); if (!valid.IsSuccess) return Result.Failure<long>(valid.Error);
        var guard = await GuardProductAsync(dto.ProductId, org.Value, ct); if (!guard.IsSuccess) return Result.Failure<long>(guard.Error); if (!await _originQuery.AnyAsync(x => x.Code == dto.DefaultOriginCode, ct)) return Result.Failure<long>(DidoxErrors.SourceMissing($"origin '{dto.DefaultOriginCode}'"));
        if (await HasProductOverlapAsync(org.Value, dto.ProductId, dto.EffectiveFrom, dto.EffectiveTo, null, ct)) return Result.Failure<long>(DidoxErrors.OverlappingEffectiveDates()); var item = ToProductProfile(dto, org.Value); await _productProfileWrite.AddAsync(item, ct); return Result.Success(item.Id);
    }, ct);
    public Task<Result> UpdateAsync(long id, ProductDidoxProfileWriteDto dto, CancellationToken ct = default) => InTransactionAsync(async () =>
    {
        var org = RequireOrganization(); if (!org.IsSuccess) return Result.Failure(org.Error); var item = await GetOneAsync(_productProfileQuery, x => x.Id == id, ct); if (item is null) return Result.Failure(DidoxErrors.NotFound("product profile")); if (item.OrganizationId != org.Value) return Result.Failure(DidoxErrors.OrganizationMismatch("product profile"));
        var valid = ValidateDates(dto.EffectiveFrom, dto.EffectiveTo); if (!valid.IsSuccess) return valid; var guard = await GuardProductAsync(dto.ProductId, org.Value, ct); if (!guard.IsSuccess) return guard; if (!await _originQuery.AnyAsync(x => x.Code == dto.DefaultOriginCode, ct)) return Result.Failure(DidoxErrors.SourceMissing($"origin '{dto.DefaultOriginCode}'"));
        if (await HasProductOverlapAsync(org.Value, dto.ProductId, dto.EffectiveFrom, dto.EffectiveTo, id, ct)) return Result.Failure(DidoxErrors.OverlappingEffectiveDates()); Apply(item, dto); await _productProfileWrite.UpdateAsync(item, ct); return Result.Success();
    }, ct);
    public async Task<Result<IReadOnlyCollection<ProductDidoxProfileDto>>> ListByProductAsync(int? productId = null, CancellationToken ct = default)
    {
        var org = RequireOrganization(); if (!org.IsSuccess) return Result.Failure<IReadOnlyCollection<ProductDidoxProfileDto>>(org.Error); return Result.Success<IReadOnlyCollection<ProductDidoxProfileDto>>((await _productProfileQuery.GetAllAsync(new QuerySpecification<ProductDidoxProfile> { Criteria = x => x.OrganizationId == org.Value && (!productId.HasValue || x.ProductId == productId.Value), OrderBy = q => q.OrderBy(x => x.ProductId).ThenBy(x => x.EffectiveFrom) }, ct)).Select(ToDto).ToList());
    }

    public Task<Result<long>> CreateAsync(ProductTableDidoxOriginWriteDto dto, CancellationToken ct = default) => InTransactionAsync(async () =>
    {
        var org = RequireOrganization(); if (!org.IsSuccess) return Result.Failure<long>(org.Error); var tableOrganizationId = await _organizationSourceReader.GetProductTableOrganizationIdAsync(dto.ProductTableId, ct); if (tableOrganizationId is null) return Result.Failure<long>(DidoxErrors.SourceMissing($"product table '{dto.ProductTableId}'")); if (tableOrganizationId != org.Value) return Result.Failure<long>(DidoxErrors.OrganizationMismatch("product table"));
        if (!await _originQuery.AnyAsync(x => x.Code == dto.OriginCode, ct)) return Result.Failure<long>(DidoxErrors.SourceMissing($"origin '{dto.OriginCode}'")); if (await _tableOriginQuery.AnyAsync(x => x.ProductTableId == dto.ProductTableId, ct)) return Result.Failure<long>(DidoxErrors.Duplicate("product table origin"));
        var item = new ProductTableDidoxOrigin { OrganizationId = org.Value, ProductTableId = dto.ProductTableId, OriginCode = dto.OriginCode, StateId = dto.StateId, CreatedDate = DateTime.UtcNow }; await _tableOriginWrite.AddAsync(item, ct); return Result.Success(item.Id);
    }, ct);
    public async Task<Result<IReadOnlyCollection<ProductTableDidoxOriginDto>>> ListByProductTableAsync(int? productTableId = null, CancellationToken ct = default)
    {
        var org = RequireOrganization(); if (!org.IsSuccess) return Result.Failure<IReadOnlyCollection<ProductTableDidoxOriginDto>>(org.Error); return Result.Success<IReadOnlyCollection<ProductTableDidoxOriginDto>>((await _tableOriginQuery.GetAllAsync(new QuerySpecification<ProductTableDidoxOrigin> { Criteria = x => x.OrganizationId == org.Value && (!productTableId.HasValue || x.ProductTableId == productTableId.Value), OrderBy = q => q.OrderBy(x => x.ProductTableId) }, ct)).Select(ToDto).ToList());
    }

    private async Task<Result<T>> InTransactionAsync<T>(Func<Task<Result<T>>> action, CancellationToken ct)
    {
        await _uow.BeginAsync(ct); try { var result = await action(); if (!result.IsSuccess) { await _uow.RollbackAsync(ct); return result; } await _uow.SaveChangesAsync(ct); await _uow.CommitAsync(ct); return result; } catch { await _uow.RollbackAsync(ct); throw; }
    }
    private async Task<Result> InTransactionAsync(Func<Task<Result>> action, CancellationToken ct)
    {
        await _uow.BeginAsync(ct); try { var result = await action(); if (!result.IsSuccess) { await _uow.RollbackAsync(ct); return result; } await _uow.SaveChangesAsync(ct); await _uow.CommitAsync(ct); return result; } catch { await _uow.RollbackAsync(ct); throw; }
    }
    private Result<int> RequireOrganization() => _user.OrganizationId is int id ? Result.Success(id) : Result.Failure<int>(DidoxErrors.OrganizationRequired());
    private static Result ValidateDates(DateOnly from, DateOnly? to) => to.HasValue && to.Value < from ? Result.Failure(DidoxErrors.InvalidEffectiveDates()) : Result.Success();
    private static async Task<T?> GetOneAsync<T>(IQueryRepository<T> query, System.Linq.Expressions.Expression<Func<T, bool>> predicate, CancellationToken ct) where T : class => await query.GetAsync(new QuerySpecification<T> { Criteria = predicate }, ct);
    private static async Task<T?> GetOneIgnoringFiltersAsync<T>(IQueryRepository<T> query, System.Linq.Expressions.Expression<Func<T, bool>> predicate, CancellationToken ct) where T : class => await query.GetAsync(new QuerySpecification<T> { Criteria = predicate, IgnoreQueryFilters = true }, ct);
    private async Task<Result> GuardCounterpartyAsync(int id, int org, CancellationToken ct) { var organizationId = await _organizationSourceReader.GetCounterpartyOrganizationIdAsync(id, ct); return organizationId is null ? Result.Failure(DidoxErrors.SourceMissing($"counterparty '{id}'")) : organizationId != org ? Result.Failure(DidoxErrors.OrganizationMismatch("counterparty")) : Result.Success(); }
    private async Task<Result> GuardProductAsync(int id, int org, CancellationToken ct) { var organizationId = await _organizationSourceReader.GetProductOrganizationIdAsync(id, ct); return organizationId is null ? Result.Failure(DidoxErrors.SourceMissing($"product '{id}'")) : organizationId != org ? Result.Failure(DidoxErrors.OrganizationMismatch("product")) : Result.Success(); }
    private async Task<bool> HasMxikOverlapAsync(string code, DateOnly from, DateOnly? to, long? id, CancellationToken ct) => (await _mxikQuery.GetAllAsync(new QuerySpecification<DidoxMxikCatalog> { Criteria = x => x.MxikCode == code && (!id.HasValue || x.Id != id.Value) }, ct)).Any(x => Overlaps(from, to, x.EffectiveFrom, x.EffectiveTo));
    private async Task<bool> HasPackageOverlapAsync(short unitId, DateOnly from, DateOnly? to, long? id, CancellationToken ct) => (await _packageQuery.GetAllAsync(new QuerySpecification<UnitDidoxPackage> { Criteria = x => x.UnitId == unitId && (!id.HasValue || x.Id != id.Value) }, ct)).Any(x => Overlaps(from, to, x.EffectiveFrom, x.EffectiveTo));
    private async Task<bool> HasCounterpartyOverlapAsync(int org, int party, DateOnly from, DateOnly? to, long? id, CancellationToken ct) => (await _counterpartyProfileQuery.GetAllAsync(new QuerySpecification<CounterpartyDidoxProfile> { Criteria = x => x.OrganizationId == org && x.CounterpartyId == party && (!id.HasValue || x.Id != id.Value) }, ct)).Any(x => Overlaps(from, to, x.EffectiveFrom, x.EffectiveTo));
    private async Task<bool> HasProductOverlapAsync(int org, int product, DateOnly from, DateOnly? to, long? id, CancellationToken ct) => (await _productProfileQuery.GetAllAsync(new QuerySpecification<ProductDidoxProfile> { Criteria = x => x.OrganizationId == org && x.ProductId == product && (!id.HasValue || x.Id != id.Value) }, ct)).Any(x => Overlaps(from, to, x.EffectiveFrom, x.EffectiveTo));
    private static bool Overlaps(DateOnly from, DateOnly? to, DateOnly otherFrom, DateOnly? otherTo) => from <= (otherTo ?? DateOnly.MaxValue) && otherFrom <= (to ?? DateOnly.MaxValue);
    private static DidoxMxikCatalog ToMxik(DidoxMxikCatalogWriteDto x) => new() { MxikCode = x.MxikCode, Name = x.Name, SourceName = x.SourceName, SourceUpdatedAt = x.SourceUpdatedAt, EffectiveFrom = x.EffectiveFrom, EffectiveTo = x.EffectiveTo, StateId = x.StateId, CreatedDate = DateTime.Now };
    private static void Apply(DidoxMxikCatalog e, DidoxMxikCatalogWriteDto x) { e.MxikCode = x.MxikCode; e.Name = x.Name; e.SourceName = x.SourceName; e.SourceUpdatedAt = x.SourceUpdatedAt; e.EffectiveFrom = x.EffectiveFrom; e.EffectiveTo = x.EffectiveTo; e.StateId = x.StateId; }
    private static DidoxMxikCatalogDto ToDto(DidoxMxikCatalog x) => new() { Id = x.Id, MxikCode = x.MxikCode, Name = x.Name, SourceName = x.SourceName, SourceUpdatedAt = x.SourceUpdatedAt, EffectiveFrom = x.EffectiveFrom, EffectiveTo = x.EffectiveTo, StateId = x.StateId };
    private static DidoxReferenceDto ToDto(DidoxOrigin x) => new() { Code = x.Code, Name = x.Name, StateId = x.StateId, CreatedDate = x.CreatedDate };
    private static DidoxReferenceDto ToDto(DidoxVatRegStatus x) => new() { Code = x.Code, Name = x.Name, StateId = x.StateId, CreatedDate = x.CreatedDate };
    private static UnitDidoxPackage ToPackage(UnitDidoxPackageWriteDto x) => new() { UnitId = x.UnitId, PackageCode = x.PackageCode, PackageName = x.PackageName, EffectiveFrom = x.EffectiveFrom, EffectiveTo = x.EffectiveTo, StateId = x.StateId, CreatedDate = DateTime.Now };
    private static void Apply(UnitDidoxPackage e, UnitDidoxPackageWriteDto x) { e.UnitId = x.UnitId; e.PackageCode = x.PackageCode; e.PackageName = x.PackageName; e.EffectiveFrom = x.EffectiveFrom; e.EffectiveTo = x.EffectiveTo; e.StateId = x.StateId; }
    private static UnitDidoxPackageDto ToDto(UnitDidoxPackage x) => new() { Id = x.Id, UnitId = x.UnitId, PackageCode = x.PackageCode, PackageName = x.PackageName, EffectiveFrom = x.EffectiveFrom, EffectiveTo = x.EffectiveTo, StateId = x.StateId };
    private static CounterpartyDidoxProfile ToCounterpartyProfile(CounterpartyDidoxProfileWriteDto x, int org) => new() { OrganizationId = org, CounterpartyId = x.CounterpartyId, VatRegCode = x.VatRegCode, VatRegStatusCode = x.VatRegStatusCode, LegalAddress = x.LegalAddress, EffectiveFrom = x.EffectiveFrom, EffectiveTo = x.EffectiveTo, StateId = x.StateId, CreatedDate = DateTime.Now };
    private static void Apply(CounterpartyDidoxProfile e, CounterpartyDidoxProfileWriteDto x) { e.CounterpartyId = x.CounterpartyId; e.VatRegCode = x.VatRegCode; e.VatRegStatusCode = x.VatRegStatusCode; e.LegalAddress = x.LegalAddress; e.EffectiveFrom = x.EffectiveFrom; e.EffectiveTo = x.EffectiveTo; e.StateId = x.StateId; }
    private static CounterpartyDidoxProfileDto ToDto(CounterpartyDidoxProfile x) => new() { Id = x.Id, OrganizationId = x.OrganizationId, CounterpartyId = x.CounterpartyId, VatRegCode = x.VatRegCode, VatRegStatusCode = x.VatRegStatusCode, LegalAddress = x.LegalAddress, EffectiveFrom = x.EffectiveFrom, EffectiveTo = x.EffectiveTo, StateId = x.StateId };
    private static ProductDidoxProfile ToProductProfile(ProductDidoxProfileWriteDto x, int org) => new() { OrganizationId = org, ProductId = x.ProductId, DefaultOriginCode = x.DefaultOriginCode, EffectiveFrom = x.EffectiveFrom, EffectiveTo = x.EffectiveTo, StateId = x.StateId, CreatedDate = DateTime.Now };
    private static void Apply(ProductDidoxProfile e, ProductDidoxProfileWriteDto x) { e.ProductId = x.ProductId; e.DefaultOriginCode = x.DefaultOriginCode; e.EffectiveFrom = x.EffectiveFrom; e.EffectiveTo = x.EffectiveTo; e.StateId = x.StateId; }
    private static ProductDidoxProfileDto ToDto(ProductDidoxProfile x) => new() { Id = x.Id, OrganizationId = x.OrganizationId, ProductId = x.ProductId, DefaultOriginCode = x.DefaultOriginCode, EffectiveFrom = x.EffectiveFrom, EffectiveTo = x.EffectiveTo, StateId = x.StateId };
    private static ProductTableDidoxOriginDto ToDto(ProductTableDidoxOrigin x) => new() { Id = x.Id, OrganizationId = x.OrganizationId, ProductTableId = x.ProductTableId, OriginCode = x.OriginCode, StateId = x.StateId };
}
