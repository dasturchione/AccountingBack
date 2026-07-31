using Application.Abstractions.Authentication;
using Application.Features.Integration.Edocs.Services;
using Integration.Edocs.Http;
using Integration.Shared.Http;
using SharedKernel.Exceptions;
using System.Net;
using System.Text.Json;

namespace Integration.Edocs.Services;

// VAQTINCHALIK — 6.3-bosqichdagi qo'lda sinov uchun. Bu klass IEdocsDebugService bilan
// birga keyinroq butunlay o'chiriladi.
public sealed class EdocsDebugService : IEdocsDebugService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IUserContext _userContext;

    public EdocsDebugService(IHttpClientFactory httpClientFactory, IUserContext userContext)
    {
        _httpClientFactory = httpClientFactory;
        _userContext = userContext;
    }

    public async Task<JsonElement> GetDocumentAsync(string type, string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new InvalidOperationException("type is required.");
        if (string.IsNullOrWhiteSpace(id))
            throw new InvalidOperationException("id is required.");

        var organizationId = RequireOrganization();
        var client = _httpClientFactory.CreateClient(EdocsHttpClientNames.Client);

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"documents/{Uri.EscapeDataString(type)}/{Uri.EscapeDataString(id)}");

        // x-product/x-partner sarlavhalari endi EdocsAuthorizationHandler ichida,
        // markazlashgan tarzda qo'shiladi (6.5.7-bosqich) — bu yerda emas.
        request.Options.Set(IntegrationHttpRequestOptions.OrganizationId, organizationId);

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!response.IsSuccessStatusCode)
        {
            throw response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new IntegrationUnauthorizedException(
                    $"Edocs document so'rovi rad etildi (401): {type}/{id}."),
                HttpStatusCode.Forbidden => new IntegrationForbiddenException(
                    $"Edocs document so'rovini rad etdi (403): {type}/{id}."),
                HttpStatusCode.NotFound => new IntegrationHttpException(
                    $"Edocs document topilmadi (404): {type}/{id}.", 404),
                _ => new IntegrationHttpException(
                    $"Edocs document so'rovi HTTP {(int)response.StatusCode} bilan tugadi.",
                    (int)response.StatusCode)
            };
        }

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        return document.RootElement.Clone();
    }

    private int RequireOrganization() => _userContext.OrganizationId
        ?? throw new InvalidOperationException("An active organization is required for Edocs debug operations.");
}
