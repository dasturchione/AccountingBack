using Integration.Tax.Configs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Abstractions.Integration;

namespace Integration.Tax.Providers;

public sealed class SoliqApiTaxProvider : TaxProviderBase, ITaxLookupProvider
{
    public SoliqApiTaxProvider(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IOptions<TaxIntegrationSettings> options, ILogger<SoliqApiTaxProvider> logger) : base(httpClientFactory, httpContextAccessor, options, logger) { }

    public override string Code => "SOLIQ_API";

    public override string Name => "Soliq API";

    protected override TaxIntegrationSettings.ProviderSettings ResolveProviderSettings() => Settings.SoliqApi;

    public async Task<IReadOnlyCollection<TaxProviderLookupItemDto>> SearchAsync(TaxProviderLookupRequestDto request, CancellationToken ct = default)
    {
        var response = await GetAsync<List<ProviderLookupResponseDto>>(Settings.SoliqApi.SearchPath, new Dictionary<string, string?>
        {
            ["query"] = request.Query,
            ["date"] = request.EffectiveDate?.ToString("yyyy-MM-dd"),
            ["organizationId"] = request.OrganizationId?.ToString()
        }, ct);

        return (response ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .Select(x =>
            {
                var code = x.Code!.Trim();
                return new TaxProviderLookupItemDto
                {
                    Code = code,
                    Name = string.IsNullOrWhiteSpace(x.Name) ? code : x.Name.Trim(),
                    Description = x.Description,
                    IsActive = x.IsActive,
                    Metadata = x.Metadata ?? new Dictionary<string, string?>()
                };
            })
            .ToList();
    }

    public async Task<TaxProviderLookupItemDto?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var response = await GetAsync<ProviderLookupResponseDto>(Settings.SoliqApi.LookupPath, new Dictionary<string, string?>
        {
            ["code"] = code
        }, ct);

        return response is null ? null : new TaxProviderLookupItemDto
        {
            Code = response.Code?.Trim() ?? string.Empty,
            Name = string.IsNullOrWhiteSpace(response.Name) ? response.Code?.Trim() ?? string.Empty : response.Name.Trim(),
            Description = response.Description,
            IsActive = response.IsActive,
            Metadata = response.Metadata ?? new Dictionary<string, string?>()
        };
    }

    private sealed class ProviderLookupResponseDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public Dictionary<string, string?>? Metadata { get; set; }
    }
}
