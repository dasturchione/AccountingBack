using Integration.Tax.Configs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Abstractions.Integration;

namespace Integration.Tax.Providers;

public sealed class MxikTaxProvider : TaxProviderBase, ITaxLookupProvider
{
    public MxikTaxProvider(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IOptions<TaxIntegrationSettings> options, ILogger<MxikTaxProvider> logger) : base(httpClientFactory, httpContextAccessor, options, logger) { }

    public override string Code => "MXIK";

    public override string Name => "MXIK";

    protected override TaxIntegrationSettings.ProviderSettings ResolveProviderSettings() => Settings.Mxik;

    public async Task<IReadOnlyCollection<TaxProviderLookupItemDto>> SearchAsync(TaxProviderLookupRequestDto request, CancellationToken ct = default)
    {
        var response = await GetAsync<List<ProviderLookupResponseDto>>(Settings.Mxik.SearchPath, new Dictionary<string, string?>
        {
            ["query"] = request.Query,
            ["date"] = request.EffectiveDate?.ToString("yyyy-MM-dd"),
            ["organizationId"] = request.OrganizationId?.ToString()
        }, ct);

        return (response ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.Code) && !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => new TaxProviderLookupItemDto
            {
                Code = x.Code.Trim(),
                Name = x.Name.Trim(),
                Description = x.Description,
                IsActive = x.IsActive,
                Metadata = x.Metadata ?? new Dictionary<string, string?>()
            })
            .ToList();
    }

    public async Task<TaxProviderLookupItemDto?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var response = await GetAsync<ProviderLookupResponseDto>(Settings.Mxik.LookupPath, new Dictionary<string, string?>
        {
            ["code"] = code
        }, ct);

        return response is null ? null : new TaxProviderLookupItemDto
        {
            Code = response.Code?.Trim() ?? string.Empty,
            Name = response.Name?.Trim() ?? string.Empty,
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
