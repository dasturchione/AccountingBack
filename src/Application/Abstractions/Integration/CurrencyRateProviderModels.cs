namespace Application.Abstractions.Integration;

public sealed class CurrencyRateProviderInfoDto
{
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public bool SupportsLatest { get; init; }
    public bool SupportsDate { get; init; }
}

public sealed class CurrencyRateImportItemDto
{
    public string BaseCurrencyCode { get; init; } = null!;
    public string TargetCurrencyCode { get; init; } = null!;
    public DateTime EffectiveDate { get; init; }
    public decimal BuyRate { get; init; }
    public decimal SellRate { get; init; }
    public decimal OfficialRate { get; init; }
    public string? RateSource { get; init; }
}

public sealed class CurrencyRateImportResultDto
{
    public string ProviderCode { get; init; } = null!;
    public DateTime ImportedAt { get; init; }
    public int TotalItems { get; init; }
    public int CreatedCount { get; init; }
    public int UpdatedCount { get; init; }
    public int SkippedCount { get; init; }
    public int InvalidCount { get; init; }
    public int FailedCount { get; init; }
    public IReadOnlyCollection<string> Messages { get; init; } = [];
}

public interface ICurrencyRateProvider
{
    string Code { get; }
    string Name { get; }
    Task<IReadOnlyCollection<CurrencyRateImportItemDto>> GetLatestAsync(CancellationToken ct = default);
    Task<IReadOnlyCollection<CurrencyRateImportItemDto>> GetByDateAsync(DateTime date, CancellationToken ct = default);
}
