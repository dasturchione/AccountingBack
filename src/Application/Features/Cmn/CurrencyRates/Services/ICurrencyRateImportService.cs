using Application.Abstractions.Integration;
using SharedKernel.Results;

namespace Application.Features.Cmn.CurrencyRates;

public interface ICurrencyRateImportService
{
    Task<Result<CurrencyRateImportResultDto>> ImportLatestAsync(string? providerCode, CancellationToken ct = default);
    Task<Result<CurrencyRateImportResultDto>> ImportByDateAsync(string? providerCode, DateTime date, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<CurrencyRateProviderInfoDto>>> GetProvidersAsync(CancellationToken ct = default);
    Task<Result<CurrencyRateImportResultDto?>> GetStatusAsync(CancellationToken ct = default);
}
