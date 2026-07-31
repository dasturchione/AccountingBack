using Application.Abstractions.Integration;
using Integration.AslBelgi.Configs;
using Integration.Shared.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Constants;
using SharedKernel.Exceptions;
using System.Net.Http.Headers;

namespace Integration.AslBelgi.Http;

public sealed class AslBelgiAuthorizationHandler : DelegatingHandler
{
    private readonly IOptions<AslBelgiOptions> _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AslBelgiAuthorizationHandler> _logger;

    public AslBelgiAuthorizationHandler(
        IOptions<AslBelgiOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<AslBelgiAuthorizationHandler> logger)
    {
        _options = options;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var options = _options.Value;
        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri)
            || !string.Equals(baseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("CRPT BaseUrl must use HTTPS.");
        }

        if (request.RequestUri is { IsAbsoluteUri: true } requestUri
            && !string.Equals(requestUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("CRPT requests must use HTTPS.");
        }

        if (!request.Options.TryGetValue(IntegrationHttpRequestOptions.OrganizationId, out var organizationId))
        {
            throw new InvalidOperationException(
                "AslBelgi so'rovida organizationId topilmadi — chaqiruvchi request.Options ga IntegrationHttpRequestOptions.OrganizationId qo'yishi shart.");
        }

        // IIntegrationCredentialProvider Scoped (ichida AppDbContext bor), handler esa
        // Transient bo'lib ro'yxatdan o'tgan bo'lsa ham HttpClientFactory tomonidan
        // ~2 daqiqa saqlanadi (handler lifetime) — uni to'g'ridan-to'g'ri konstruktorga
        // inject qilish captive dependency bo'lardi (DbContext oqimlar bo'ylab qayta
        // ishlatilib qolardi). Shuning uchun har SendAsync ichida alohida DI scope yaratiladi.
        using var scope = _scopeFactory.CreateScope();
        var credentialProvider = scope.ServiceProvider.GetRequiredService<IIntegrationCredentialProvider>();
        var credential = await credentialProvider.GetAsync(organizationId, IntegrationProviderConst.AslBelgi, cancellationToken);

        if (credential is null)
        {
            throw new IntegrationUnauthorizedException(
                $"AslBelgi uchun tashkilot {organizationId} kredensiali topilmadi yoki faol emas (provider='{IntegrationProviderConst.AslBelgi}').");
        }

        if (string.IsNullOrWhiteSpace(credential.ApiKey))
        {
            throw new IntegrationUnauthorizedException(
                $"AslBelgi kredensialida (tashkilot {organizationId}) api_key bo'sh.");
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credential.ApiKey);

        _logger.LogDebug(
            "Sending CRPT request {Method} to host {Host} for organization {OrganizationId}.",
            request.Method.Method,
            request.RequestUri?.Host ?? baseUri.Host,
            organizationId);

        return await base.SendAsync(request, cancellationToken);
    }
}
