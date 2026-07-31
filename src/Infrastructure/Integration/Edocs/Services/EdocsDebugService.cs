using Application.Features.Integration.Edocs.Services;
using Integration.Edocs.Configs;
using Integration.Edocs.Http;
using Microsoft.Extensions.Options;
using SharedKernel.Exceptions;
using System.Net;
using System.Text.Json;

namespace Integration.Edocs.Services;

// VAQTINCHALIK — 6.3-bosqichdagi qo'lda sinov uchun. Bu klass IEdocsDebugService bilan
// birga keyinroq butunlay o'chiriladi.
public sealed class EdocsDebugService : IEdocsDebugService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<EdocsOptions> _options;

    public EdocsDebugService(IHttpClientFactory httpClientFactory, IOptions<EdocsOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
    }

    public async Task<JsonElement> GetDocumentAsync(string type, string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new InvalidOperationException("type is required.");
        if (string.IsNullOrWhiteSpace(id))
            throw new InvalidOperationException("id is required.");

        var client = _httpClientFactory.CreateClient(EdocsHttpClientNames.Client);

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"documents/{Uri.EscapeDataString(type)}/{Uri.EscapeDataString(id)}");

        // doc.edocs.uz frontend bundle'idan (index-H5tLYbOJ.js.download) aniqlandi:
        // barcha /documents/ so'rovlariga bu ikkita sarlavha qo'shiladi — E-DOCS.pdf
        // (rasmiy hujjat) bularni umuman qayd etmaydi. Ularsiz so'rov rad etilishi mumkin.
        var options = _options.Value;
        if (!string.IsNullOrWhiteSpace(options.Product))
            request.Headers.TryAddWithoutValidation("x-product", options.Product);
        if (!string.IsNullOrWhiteSpace(options.PartnerId))
            request.Headers.TryAddWithoutValidation("x-partner", options.PartnerId);

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
}
