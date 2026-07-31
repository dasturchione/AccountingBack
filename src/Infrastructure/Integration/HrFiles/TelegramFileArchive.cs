using Application.Features.Hr.Files;
using Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Infrastructure.Integration.HrFiles;

public sealed class TelegramFileArchive : ITelegramFileArchive
{
    private readonly HttpClient _httpClient;
    private readonly TelegramFileStorageOptions _options;

    public TelegramFileArchive(
        HttpClient httpClient,
        IOptions<TelegramFileStorageOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<TelegramArchivedFile> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        string caption,
        CancellationToken ct = default)
    {
        EnsureConfigured();

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(_options.ChatId.ToString()), "chat_id");
        form.Add(new StringContent(caption), "caption");

        var fileContent = new StreamContent(content);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(
            string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
        form.Add(fileContent, "document", Path.GetFileName(fileName));

        using var response = await _httpClient.PostAsync(GetBotUrl("sendDocument"), form, ct);
        await EnsureSuccessAsync(response, ct);

        await using var responseStream = await response.Content.ReadAsStreamAsync(ct);
        using var json = await JsonDocument.ParseAsync(responseStream, cancellationToken: ct);
        var result = json.RootElement.GetProperty("result");
        var messageId = result.GetProperty("message_id").GetInt32();
        var fileId = result.GetProperty("document").GetProperty("file_id").GetString();
        if (string.IsNullOrWhiteSpace(fileId))
            throw new InvalidOperationException("Telegram fayl identifikatorini qaytarmadi.");

        return new TelegramArchivedFile(fileId, messageId);
    }

    public async Task<Stream> DownloadAsync(string fileId, CancellationToken ct = default)
    {
        EnsureConfigured();
        using var fileResponse = await _httpClient.GetAsync(
            GetBotUrl($"getFile?file_id={Uri.EscapeDataString(fileId)}"),
            ct);
        await EnsureSuccessAsync(fileResponse, ct);

        await using var infoStream = await fileResponse.Content.ReadAsStreamAsync(ct);
        using var json = await JsonDocument.ParseAsync(infoStream, cancellationToken: ct);
        var filePath = json.RootElement.GetProperty("result").GetProperty("file_path").GetString();
        if (string.IsNullOrWhiteSpace(filePath))
            throw new InvalidOperationException("Telegram fayl manzilini qaytarmadi.");

        using var downloadResponse = await _httpClient.GetAsync(
            $"https://api.telegram.org/file/bot{_options.BotToken}/{filePath}",
            HttpCompletionOption.ResponseHeadersRead,
            ct);
        await EnsureSuccessAsync(downloadResponse, ct);

        var result = new MemoryStream();
        await downloadResponse.Content.CopyToAsync(result, ct);
        result.Position = 0;
        return result;
    }

    public async Task DeleteMessageAsync(int messageId, CancellationToken ct = default)
    {
        EnsureConfigured();
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["chat_id"] = _options.ChatId.ToString(),
            ["message_id"] = messageId.ToString()
        });
        using var response = await _httpClient.PostAsync(GetBotUrl("deleteMessage"), form, ct);
        await EnsureSuccessAsync(response, ct);
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.BotToken) || _options.ChatId == 0)
            throw new InvalidOperationException(
                "Telegram fayl arxivi sozlanmagan: BotToken va ChatId kiritilishi kerak.");
    }

    private string GetBotUrl(string method) =>
        $"https://api.telegram.org/bot{_options.BotToken}/{method}";

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        _ = await response.Content.ReadAsStringAsync(ct);
        throw new InvalidOperationException(
            $"Telegram fayl arxivi HTTP {(int)response.StatusCode} xatosini qaytardi.");
    }
}
