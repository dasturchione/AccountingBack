using Application.Abstractions;
using Application.Abstractions.Integration;
using Application.Features.Cmn.CurrencyRates;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Constants;
using SharedKernel.Results;
using Integration.CentralBank.Configs;

namespace Integration.CentralBank.Services;

public sealed class CurrencyRateImportService : ICurrencyRateImportService
{
    private readonly IQueryRepository<Currency> _currencyQuery;
    private readonly IQueryRepository<CurrencyRate> _rateQuery;
    private readonly ICommandRepository<CurrencyRate> _rateCommand;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReadOnlyCollection<ICurrencyRateProvider> _providers;
    private readonly CentralBankSettings _settings;
    private readonly ILogger<CurrencyRateImportService> _logger;
    private CurrencyRateImportResultDto? _lastStatus;

    public CurrencyRateImportService(
        IQueryRepository<Currency> currencyQuery,
        IQueryRepository<CurrencyRate> rateQuery,
        ICommandRepository<CurrencyRate> rateCommand,
        IUnitOfWork unitOfWork,
        IEnumerable<ICurrencyRateProvider> providers,
        IOptions<CentralBankSettings> settings,
        ILogger<CurrencyRateImportService> logger)
    {
        _currencyQuery = currencyQuery;
        _rateQuery = rateQuery;
        _rateCommand = rateCommand;
        _unitOfWork = unitOfWork;
        _providers = providers.ToList();
        _settings = settings.Value;
        _logger = logger;
    }

    public Task<Result<IReadOnlyCollection<CurrencyRateProviderInfoDto>>> GetProvidersAsync(CancellationToken ct = default)
    {
        IReadOnlyCollection<CurrencyRateProviderInfoDto> result = _providers
            .Select(x => new CurrencyRateProviderInfoDto { Code = x.Code, Name = x.Name, SupportsLatest = true, SupportsDate = true })
            .ToList();

        return Task.FromResult(Result.Success(result));
    }

    public Task<Result<CurrencyRateImportResultDto?>> GetStatusAsync(CancellationToken ct = default)
        => Task.FromResult(Result.Success(_lastStatus));

    public Task<Result<CurrencyRateImportResultDto>> ImportLatestAsync(string? providerCode, CancellationToken ct = default)
        => ImportAsync(providerCode, null, ct);

    public Task<Result<CurrencyRateImportResultDto>> ImportByDateAsync(string? providerCode, DateTime date, CancellationToken ct = default)
        => ImportAsync(providerCode, date.Date, ct);

    private async Task<Result<CurrencyRateImportResultDto>> ImportAsync(string? providerCode, DateTime? date, CancellationToken ct)
    {
        await _unitOfWork.BeginAsync(ct);

        try
        {
            var provider = ResolveProvider(providerCode);
            if (provider is null)
                return Result.Failure<CurrencyRateImportResultDto>(Error.NotFound("CurrencyRate.ProviderNotFound", "Requested currency rate provider was not found."));

            var items = date is null
                ? await provider.GetLatestAsync(ct)
                : await provider.GetByDateAsync(date.Value, ct);

            var normalizedItems = items
                .Where(x => !string.IsNullOrWhiteSpace(x.BaseCurrencyCode) && !string.IsNullOrWhiteSpace(x.TargetCurrencyCode))
                .GroupBy(x => new
                {
                    BaseCurrencyCode = x.BaseCurrencyCode.Trim().ToUpperInvariant(),
                    TargetCurrencyCode = x.TargetCurrencyCode.Trim().ToUpperInvariant(),
                    x.EffectiveDate
                })
                .Select(x => x.OrderByDescending(i => i.OfficialRate).ThenByDescending(i => i.BuyRate).ThenByDescending(i => i.SellRate).First())
                .ToList();

            var created = 0;
            var updated = 0;
            var skipped = 0;
            var invalid = 0;
            var failed = 0;
            var messages = new List<string>();

            foreach (var item in normalizedItems)
            {
                ct.ThrowIfCancellationRequested();

                var baseCurrency = await _currencyQuery.GetAsync(new SharedKernel.Query.Specifications.QuerySpecification<Currency>
                {
                    Criteria = x => x.Code == item.BaseCurrencyCode.Trim()
                }, ct);

                var targetCurrency = await _currencyQuery.GetAsync(new SharedKernel.Query.Specifications.QuerySpecification<Currency>
                {
                    Criteria = x => x.Code == item.TargetCurrencyCode.Trim()
                }, ct);

                if (baseCurrency is null || targetCurrency is null || item.OfficialRate <= 0)
                {
                    invalid++;
                    continue;
                }

                var existingQuery = new SharedKernel.Query.Specifications.QuerySpecification<CurrencyRate>
                {
                    Criteria = x => x.BaseCurrencyId == baseCurrency.Id
                               && x.TargetCurrencyId == targetCurrency.Id
                               && x.EffectiveDate == item.EffectiveDate
                };

                var existing = await _rateQuery.GetAsync(existingQuery, ct);
                if (existing is not null)
                {
                    existing.BuyRate = item.BuyRate;
                    existing.SellRate = item.SellRate;
                    existing.OfficialRate = item.OfficialRate;
                    existing.RateSource = item.RateSource;
                    existing.IsActive = true;
                    existing.StateId = StateIdConst.ACTIVE;
                    await _rateCommand.UpdateAsync(existing, ct);
                    updated++;
                    continue;
                }

                var duplicateActiveQuery = new SharedKernel.Query.Specifications.QuerySpecification<CurrencyRate>
                {
                    Criteria = x => x.BaseCurrencyId == baseCurrency.Id
                               && x.TargetCurrencyId == targetCurrency.Id
                               && x.EffectiveDate == item.EffectiveDate
                               && x.StateId == StateIdConst.ACTIVE
                               && x.IsActive
                };

                if ((await _rateQuery.GetAllAsync(duplicateActiveQuery, ct)).Any())
                {
                    skipped++;
                    continue;
                }

                var entity = new CurrencyRate
                {
                    BaseCurrencyId = baseCurrency.Id,
                    TargetCurrencyId = targetCurrency.Id,
                    EffectiveDate = item.EffectiveDate,
                    BuyRate = item.BuyRate,
                    SellRate = item.SellRate,
                    OfficialRate = item.OfficialRate,
                    RateSource = item.RateSource,
                    IsActive = true,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.Now
                };
                try
                {
                    await _rateCommand.CreateAsync(entity, ct);
                    created++;
                }
                catch (Exception ex)
                {
                    failed++;
                    messages.Add($"Failed to import rate {item.BaseCurrencyCode}->{item.TargetCurrencyCode} on {item.EffectiveDate:yyyy-MM-dd}: {ex.Message}");
                }
            }

            _lastStatus = new CurrencyRateImportResultDto
            {
                ProviderCode = provider.Code,
                ImportedAt = DateTime.Now,
                TotalItems = normalizedItems.Count,
                CreatedCount = created,
                UpdatedCount = updated,
                SkippedCount = skipped,
                InvalidCount = invalid,
                FailedCount = failed,
                Messages = messages
            };

            await _unitOfWork.CommitAsync(ct);

            _logger.LogInformation("Imported {Created} currency rates, updated {Updated}, skipped {Skipped} from {Provider}", created, updated, skipped, provider.Code);
            return Result.Success(_lastStatus);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }
    }

    private ICurrencyRateProvider? ResolveProvider(string? providerCode)
    {
        if (!string.IsNullOrWhiteSpace(providerCode))
            return _providers.FirstOrDefault(x => x.Code.Equals(providerCode, StringComparison.OrdinalIgnoreCase));

        return _providers.FirstOrDefault(x => x.Code.Equals(_settings.DefaultProviderCode, StringComparison.OrdinalIgnoreCase))
               ?? _providers.FirstOrDefault();
    }
}
