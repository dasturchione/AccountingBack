using Integration.Tax.Configs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Abstractions.Integration;

namespace Integration.Tax.Providers;

public sealed class EFakturaTaxProvider : TaxProviderBase, ITaxDocumentProvider
{
    public EFakturaTaxProvider(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IOptions<TaxIntegrationOptions> options, ILogger<EFakturaTaxProvider> logger) : base(httpClientFactory, httpContextAccessor, options, logger) { }

    public override string Code => "E_FAKTURA";

    public override string Name => "E-Faktura";

    protected override TaxIntegrationOptions.ProviderOptions ResolveProviderSettings() => Settings.EFaktura;

    public Task<TaxProviderOperationResultDto> SubmitAsync(TaxProviderOperationRequestDto request, CancellationToken ct = default)
        => ExecuteAsync(Settings.EFaktura.SubmitPath, "submit", request, ct);

    public Task<TaxProviderOperationResultDto> GetDocumentStatusAsync(TaxProviderOperationRequestDto request, CancellationToken ct = default)
        => ExecuteAsync(Settings.EFaktura.StatusQueryPath, "status", request, ct);

    public Task<TaxProviderOperationResultDto> CancelAsync(TaxProviderOperationRequestDto request, CancellationToken ct = default)
        => ExecuteAsync(Settings.EFaktura.CancelPath, "cancel", request, ct);

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
