using Integration.Tax.Configs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Abstractions.Integration;
using System.Text.Json.Serialization;

namespace Integration.Tax.Providers;

public sealed class MxikTaxProvider : TaxProviderBase, ITaxLookupProvider
{
    public MxikTaxProvider(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IOptions<TaxIntegrationSettings> options, ILogger<MxikTaxProvider> logger) : base(httpClientFactory, httpContextAccessor, options, logger) { }

    public override string Code => "MXIK";

    public override string Name => "MXIK";

    protected override TaxIntegrationSettings.ProviderSettings ResolveProviderSettings() => Settings.Mxik;

    public async Task<IReadOnlyCollection<TaxProviderLookupItemDto>> SearchAsync(TaxProviderLookupRequestDto request, CancellationToken ct = default)
    {
        var response = await GetAsync<MxikSearchResponseDto>(Settings.Mxik.SearchPath, new Dictionary<string, string?>
        {
            ["text"] = request.Query,
            ["size"] = "20"
        }, ct);

        return (response?.Data ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.MxikCode))
            .Select(Map)
            .ToList();
    }

    public async Task<TaxProviderLookupItemDto?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var response = await GetAsync<MxikSearchResponseDto>(Settings.Mxik.LookupPath, new Dictionary<string, string?>
        {
            ["text"] = code,
            ["size"] = "1"
        }, ct);

        var item = response?.Data?.FirstOrDefault(x => string.Equals(x.MxikCode?.Trim(), code.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? response?.Data?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.MxikCode));

        return item is null ? null : Map(item);
    }

    private static TaxProviderLookupItemDto Map(MxikItemDto item)
    {
        var mxikCode = item.MxikCode!.Trim();
        var name = !string.IsNullOrWhiteSpace(item.MxikName)
            ? item.MxikName!.Trim()
            : item.ClassName?.Trim() ?? mxikCode;

        return new TaxProviderLookupItemDto
        {
            Code = mxikCode,
            Name = name,
            Description = item.ClassName?.Trim() ?? item.GroupName?.Trim(),
            IsActive = true,
            Metadata = new Dictionary<string, string?>
            {
                ["internationalCode"] = item.InternationalCode,
                ["groupCode"] = item.GroupCode,
                ["groupName"] = item.GroupName
            }
        };
    }

    private sealed class MxikSearchResponseDto
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }

        [JsonPropertyName("recordTotal")]
        public long RecordTotal { get; set; }

        [JsonPropertyName("data")]
        public List<MxikItemDto>? Data { get; set; }
    }

    private sealed class MxikItemDto
    {
        [JsonPropertyName("mxikCode")]
        public string? MxikCode { get; set; }

        [JsonPropertyName("mxikName")]
        public string? MxikName { get; set; }

        [JsonPropertyName("className")]
        public string? ClassName { get; set; }

        [JsonPropertyName("groupName")]
        public string? GroupName { get; set; }

        [JsonPropertyName("groupCode")]
        public string? GroupCode { get; set; }

        [JsonPropertyName("internationalCode")]
        public string? InternationalCode { get; set; }
    }
}
