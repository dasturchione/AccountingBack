using SharedKernel.Results;

namespace Application.Features.Didox;

public class DidoxMxikCatalogWriteDto
{
    public string MxikCode { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string SourceName { get; init; } = null!;
    public DateTime? SourceUpdatedAt { get; init; }
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }
    public short StateId { get; init; }
}

public sealed class DidoxMxikCatalogDto : DidoxMxikCatalogWriteDto { public long Id { get; init; } }
public sealed class DidoxMxikCatalogImportCommand { public IReadOnlyCollection<DidoxMxikCatalogWriteDto> Items { get; init; } = []; }
public sealed class DidoxImportResultDto { public int CreatedCount { get; init; } public int UpdatedCount { get; init; } }

public class DidoxReferenceWriteDto { public short Code { get; init; } public string Name { get; init; } = null!; public short StateId { get; init; } }
public sealed class DidoxReferenceDto : DidoxReferenceWriteDto { public DateTime CreatedDate { get; init; } }

public class UnitDidoxPackageWriteDto
{
    public short UnitId { get; init; }
    public string PackageCode { get; init; } = null!;
    public string PackageName { get; init; } = null!;
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }
    public short StateId { get; init; }
}
public sealed class UnitDidoxPackageDto : UnitDidoxPackageWriteDto { public long Id { get; init; } }

public class CounterpartyDidoxProfileWriteDto
{
    public int CounterpartyId { get; init; }
    public string VatRegCode { get; init; } = null!;
    public short VatRegStatusCode { get; init; }
    public string LegalAddress { get; init; } = null!;
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }
    public short StateId { get; init; }
}
public sealed class CounterpartyDidoxProfileDto : CounterpartyDidoxProfileWriteDto { public long Id { get; init; } public int OrganizationId { get; init; } }

public class ProductDidoxProfileWriteDto
{
    public int ProductId { get; init; }
    public short DefaultOriginCode { get; init; }
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }
    public short StateId { get; init; }
}
public sealed class ProductDidoxProfileDto : ProductDidoxProfileWriteDto { public long Id { get; init; } public int OrganizationId { get; init; } }

public class ProductTableDidoxOriginWriteDto { public int ProductTableId { get; init; } public short OriginCode { get; init; } public short StateId { get; init; } }
public sealed class ProductTableDidoxOriginDto : ProductTableDidoxOriginWriteDto { public long Id { get; init; } public int OrganizationId { get; init; } }

public interface IDidoxMxikCatalogService
{
    Task<Result<long>> CreateAsync(DidoxMxikCatalogWriteDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, DidoxMxikCatalogWriteDto dto, CancellationToken ct = default);
    Task<Result<DidoxMxikCatalogDto>> GetByCodeAsync(string mxikCode, CancellationToken ct = default);
    Task<Result<DidoxMxikCatalogDto>> GetAsOfAsync(string mxikCode, DateOnly date, CancellationToken ct = default);
    Task<Result<DidoxImportResultDto>> ImportAsync(DidoxMxikCatalogImportCommand command, CancellationToken ct = default);
}

public interface IDidoxOriginService
{
    Task<Result<short>> CreateAsync(DidoxReferenceWriteDto dto, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<DidoxReferenceDto>>> ListAsync(CancellationToken ct = default);
    Task<Result<DidoxReferenceDto>> GetByCodeAsync(short code, CancellationToken ct = default);
    Task<Result> ValidateForEffectiveDateAsync(short code, DateOnly date, CancellationToken ct = default);
}

public interface IDidoxVatRegStatusService
{
    Task<Result<short>> CreateAsync(DidoxReferenceWriteDto dto, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<DidoxReferenceDto>>> ListAsync(CancellationToken ct = default);
    Task<Result<DidoxReferenceDto>> GetByCodeAsync(short code, CancellationToken ct = default);
}

public interface IUnitDidoxPackageService
{
    Task<Result<long>> CreateAsync(UnitDidoxPackageWriteDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, UnitDidoxPackageWriteDto dto, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<UnitDidoxPackageDto>>> ListAsync(short? unitId = null, CancellationToken ct = default);
    Task<Result<UnitDidoxPackageDto>> GetAsOfAsync(short unitId, DateOnly date, CancellationToken ct = default);
}

public interface ICounterpartyDidoxProfileService
{
    Task<Result<long>> CreateAsync(CounterpartyDidoxProfileWriteDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, CounterpartyDidoxProfileWriteDto dto, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<CounterpartyDidoxProfileDto>>> ListByCounterpartyAsync(int? counterpartyId = null, CancellationToken ct = default);
}

public interface IProductDidoxProfileService
{
    Task<Result<long>> CreateAsync(ProductDidoxProfileWriteDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, ProductDidoxProfileWriteDto dto, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<ProductDidoxProfileDto>>> ListByProductAsync(int? productId = null, CancellationToken ct = default);
}

public interface IProductTableDidoxOriginService
{
    Task<Result<long>> CreateAsync(ProductTableDidoxOriginWriteDto dto, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<ProductTableDidoxOriginDto>>> ListByProductTableAsync(int? productTableId = null, CancellationToken ct = default);
}
