using Application.Abstractions.Integration;
using Integration.CentralBank.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Integration.CentralBank.Services;

public sealed class CentralBankCurrencyRateProvider : ICurrencyRateProvider
{
    private readonly HttpClient _httpClient;
    private readonly CentralBankSettings _settings;
    private readonly ILogger<CentralBankCurrencyRateProvider> _logger;

    public CentralBankCurrencyRateProvider(HttpClient httpClient, IOptions<CentralBankSettings> options, ILogger<CentralBankCurrencyRateProvider> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl, UriKind.Absolute);
        _httpClient.Timeout = TimeSpan.FromSeconds(Math.Max(5, _settings.TimeoutSeconds));
    }

    public string Code => _settings.DefaultProviderCode;

    public string Name => "Central Bank";

    public async Task<IReadOnlyCollection<CurrencyRateImportItemDto>> GetLatestAsync(CancellationToken ct = default)
    {
        var date = DateTime.UtcNow.Date;
        return await GetByDateAsync(date, ct);
    }

    public async Task<IReadOnlyCollection<CurrencyRateImportItemDto>> GetByDateAsync(DateTime date, CancellationToken ct = default)
    {
        var normalized = date.Date;
        var uri = $"{_settings.RatesPath}?date={normalized:yyyy-MM-dd}";
        _logger.LogInformation("Fetching currency rates from Central Bank for {Date}", normalized);

        var items = await SendWithRetryAsync(async token =>
            await _httpClient.GetFromJsonAsync<List<CentralBankRateDto>>(uri, token) ?? [], ct);
        return items
            .Where(x => !string.IsNullOrWhiteSpace(x.CurrencyCode) && x.Rate > 0)
            .Select(x => new CurrencyRateImportItemDto
            {
                BaseCurrencyCode = x.BaseCurrencyCode ?? "UZS",
                TargetCurrencyCode = x.CurrencyCode.Trim().ToUpperInvariant(),
                EffectiveDate = normalized,
                BuyRate = x.BuyRate ?? x.Rate,
                SellRate = x.SellRate ?? x.Rate,
                OfficialRate = x.Rate,
                RateSource = Name
            })
            .ToList();
    }

    private async Task<T> SendWithRetryAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct)
    {
        var attempts = Math.Max(1, _settings.RetryCount);
        var delay = TimeSpan.FromMilliseconds(300);

        for (var i = 1; i <= attempts; i++)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                return await action(ct);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) when (i < attempts)
            {
                _logger.LogWarning(ex, "Central Bank request attempt {Attempt}/{Attempts} failed", i, attempts);
                await Task.Delay(delay, ct);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
            }
        }

        return await action(ct);
    }

    private sealed class CentralBankRateDto
    {
        public string? CurrencyCode { get; set; }
        public string? BaseCurrencyCode { get; set; }
        public decimal Rate { get; set; }
        public decimal? BuyRate { get; set; }
        public decimal? SellRate { get; set; }
    }
}
