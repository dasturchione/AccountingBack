using Application.Abstractions.Integration;
using Integration.CentralBank.Configs;
using Integration.CentralBank.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Integration.CentralBank.Services;

public sealed class CentralBankCurrencyRateProvider : ICurrencyRateProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CentralBankOptions _settings;
    private readonly ILogger<CentralBankCurrencyRateProvider> _logger;

    public CentralBankCurrencyRateProvider(IHttpClientFactory httpClientFactory, IOptions<CentralBankOptions> options, ILogger<CentralBankCurrencyRateProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = options.Value;
        _logger = logger;
    }

    public string Code => _settings.DefaultProviderCode;

    public string Name => "Central Bank";

    public async Task<IReadOnlyCollection<CurrencyRateImportItemDto>> GetLatestAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching latest currency rates from Central Bank");

        // Qayta urinish CentralBankRetryHandler'da bajariladi.
        var client = _httpClientFactory.CreateClient(CentralBankHttpClientNames.Client);
        var items = await client.GetFromJsonAsync<List<CentralBankRateDto>>(string.Empty, ct) ?? [];
        return Map(items, DateTime.UtcNow.Date);
    }

    public async Task<IReadOnlyCollection<CurrencyRateImportItemDto>> GetByDateAsync(DateTime date, CancellationToken ct = default)
    {
        var normalized = date.Date;
        var uri = $"all/{normalized:yyyy-MM-dd}/";
        _logger.LogInformation("Fetching currency rates from Central Bank for {Date}", normalized);

        var client = _httpClientFactory.CreateClient(CentralBankHttpClientNames.Client);
        var items = await client.GetFromJsonAsync<List<CentralBankRateDto>>(uri, ct) ?? [];
        return Map(items, normalized);
    }

    private IReadOnlyCollection<CurrencyRateImportItemDto> Map(IEnumerable<CentralBankRateDto> items, DateTime fallbackDate)
    {
        var result = new List<CurrencyRateImportItemDto>();

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Ccy)
                || !decimal.TryParse(item.Rate, NumberStyles.Number, CultureInfo.InvariantCulture, out var rate)
                || rate <= 0)
            {
                continue;
            }

            var nominal = int.TryParse(item.Nominal, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedNominal) && parsedNominal > 0
                ? parsedNominal
                : 1;
            var officialRate = rate / nominal;

            var effectiveDate = DateTime.TryParseExact(item.Date, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate)
                ? parsedDate.Date
                : fallbackDate;

            result.Add(new CurrencyRateImportItemDto
            {
                BaseCurrencyCode = "UZS",
                TargetCurrencyCode = item.Ccy!.Trim().ToUpperInvariant(),
                EffectiveDate = effectiveDate,
                BuyRate = officialRate,
                SellRate = officialRate,
                OfficialRate = officialRate,
                RateSource = Name
            });
        }

        return result;
    }


    private sealed class CentralBankRateDto
    {
        [JsonPropertyName("Ccy")]
        public string? Ccy { get; set; }

        [JsonPropertyName("Code")]
        public string? Code { get; set; }

        [JsonPropertyName("CcyNm_UZ")]
        public string? NameUz { get; set; }

        [JsonPropertyName("CcyNm_EN")]
        public string? NameEn { get; set; }

        [JsonPropertyName("Nominal")]
        public string? Nominal { get; set; }

        [JsonPropertyName("Rate")]
        public string? Rate { get; set; }

        [JsonPropertyName("Diff")]
        public string? Diff { get; set; }

        [JsonPropertyName("Date")]
        public string? Date { get; set; }
    }
}
