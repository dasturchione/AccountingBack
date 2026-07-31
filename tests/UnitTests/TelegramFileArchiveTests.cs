using Infrastructure.Integration.HrFiles;
using Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;

namespace UnitTests;

public sealed class TelegramFileArchiveTests
{
    [Fact]
    public async Task UploadAsync_IncludesTelegramErrorDescriptionWithoutExposingToken()
    {
        const string botToken = "123456789:test-token-value";
        using var httpClient = new HttpClient(new StubHandler(new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(
                "{\"ok\":false,\"error_code\":403,\"description\":\"Forbidden: bot is not a member\"}",
                Encoding.UTF8,
                "application/json")
        }));
        var archive = new TelegramFileArchive(
            httpClient,
            Options.Create(new TelegramFileStorageOptions
            {
                BotToken = botToken,
                ChatId = -1001234567890
            }));
        await using var content = new MemoryStream([1, 2, 3]);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            archive.UploadAsync(content, "document.pdf", "application/pdf", "test"));

        Assert.Contains("HTTP 403", exception.Message);
        Assert.Contains("bot is not a member", exception.Message);
        Assert.DoesNotContain(botToken, exception.Message);
    }

    private sealed class StubHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(response);
    }
}
