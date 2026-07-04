using Integration.Tax.Configs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Abstractions.Integration;

namespace Integration.Tax.Providers;

public sealed class DidoxTaxProvider : TaxProviderBase, ITaxDocumentProvider
{
    public DidoxTaxProvider(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IOptions<TaxIntegrationSettings> options, ILogger<DidoxTaxProvider> logger) : base(httpClientFactory, httpContextAccessor, options, logger) { }

    public override string Code => "DIDOX";

    public override string Name => "Didox";

    protected override TaxIntegrationSettings.ProviderSettings ResolveProviderSettings() => Settings.Didox;

    public Task<TaxProviderOperationResultDto> SubmitAsync(TaxProviderOperationRequestDto request, CancellationToken ct = default)
        => ExecuteAsync(Settings.Didox.SubmitPath, "submit", request, ct);

    public Task<TaxProviderOperationResultDto> GetDocumentStatusAsync(TaxProviderOperationRequestDto request, CancellationToken ct = default)
        => ExecuteAsync(Settings.Didox.StatusQueryPath, "status", request, ct);

    public Task<TaxProviderOperationResultDto> CancelAsync(TaxProviderOperationRequestDto request, CancellationToken ct = default)
        => ExecuteAsync(Settings.Didox.CancelPath, "cancel", request, ct);

    private async Task<TaxProviderOperationResultDto> ExecuteAsync(string path, string operation, TaxProviderOperationRequestDto request, CancellationToken ct)
    {
        var response = await PostAsync<TaxProviderOperationRequestDto, ProviderOperationResponseDto>(path, request, ct);
        return new TaxProviderOperationResultDto
        {
            ProviderCode = Code,
            Operation = operation,
            ExternalDocumentId = response?.ExternalDocumentId,
            StatusCode = response?.StatusCode,
            StatusName = response?.StatusName,
            IsSuccessful = response?.IsSuccessful ?? false,
            Message = response?.Message,
            RequestedAt = DateTime.Now
        };
    }

    private sealed class ProviderOperationResponseDto
    {
        public string? ExternalDocumentId { get; set; }
        public string? StatusCode { get; set; }
        public string? StatusName { get; set; }
        public bool IsSuccessful { get; set; }
        public string? Message { get; set; }
    }
}
