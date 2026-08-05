using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Integration.Didox.Configs;
using Integration.Didox.Facturas;
using Integration.Didox.Http;
using Integration.Didox.Services;
using Integration.Shared.Http;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using System.Text.Json;

namespace UnitTests;

public sealed class DidoxLegacyModeTests
{
    [Fact]
    public async Task AuthorizationHandler_WithoutPartnerToken_SendsOnlyUserKey()
    {
        using var fixture = CreateTokenFixture();
        fixture.Cache.Set(11, "user-key", TimeSpan.FromMinutes(5));

        var recorder = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = CreateAuthorizedClient(fixture.Options, fixture.Cache, recorder);
        using var request = new HttpRequestMessage(HttpMethod.Get, "v2/documents");
        request.Options.Set(IntegrationHttpRequestOptions.OrganizationId, 11);

        using var response = await client.SendAsync(request);

        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(recorder.LastRequest);
        Assert.True(recorder.LastRequest!.Headers.Contains("user-key"));
        Assert.False(recorder.LastRequest.Headers.Contains("Partner-Authorization"));
    }

    [Fact]
    public async Task AuthorizationHandler_WithPartnerToken_PreservesOptionalPartnerHeader()
    {
        using var fixture = CreateTokenFixture(partnerToken: "partner-token");
        fixture.Cache.Set(11, "user-key", TimeSpan.FromMinutes(5));

        var recorder = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = CreateAuthorizedClient(fixture.Options, fixture.Cache, recorder);
        using var request = new HttpRequestMessage(HttpMethod.Get, "v2/documents");
        request.Options.Set(IntegrationHttpRequestOptions.OrganizationId, 11);

        using var response = await client.SendAsync(request);

        Assert.True(response.IsSuccessStatusCode);
        Assert.True(recorder.LastRequest!.Headers.Contains("user-key"));
        Assert.True(recorder.LastRequest.Headers.Contains("Partner-Authorization"));
    }

    [Fact]
    public async Task TimestampClient_SigningFlowUsesDidoxTimestampContract()
    {
        var recorder = new RecordingHandler(_ => JsonResponse("{\"timeStampTokenB64\":\"timestamp\"}"));
        var factory = new TestHttpClientFactory(new HttpClient(recorder) { BaseAddress = new Uri("https://api.didox.uz/") });
        var client = new DidoxTimestampClient(factory);

        var token = await client.GetTimeStampTokenForSigningAsync(11, "pkcs7", "signature", CancellationToken.None);

        Assert.Equal("timestamp", token);
        Assert.NotNull(recorder.LastRequest);
        Assert.Equal("v1/dsvs/timestamp", recorder.LastRequest!.RequestUri!.PathAndQuery.TrimStart('/'));
        Assert.True(recorder.LastRequest.Options.TryGetValue(
            IntegrationHttpRequestOptions.OrganizationId,
            out var organizationId));
        Assert.Equal(11, organizationId);

        using var body = JsonDocument.Parse(recorder.LastRequestBody!);
        Assert.True(body.RootElement.TryGetProperty("pkcs7", out _));
        Assert.True(body.RootElement.TryGetProperty("signatureHex", out _));
    }

    [Fact]
    public async Task InboxMapping_UsesHasMarksAndNestedKizMarks()
    {
        const string responseBody = """
            {
              "data": [
                {
                  "doc_id": "doc-1",
                  "doctype": "002",
                  "doc_status": 3,
                  "mark_codes": ["mark-1"],
                  "ProductList": {
                    "products": [
                      { "marks": { "kiz": ["mark-2", "mark-3"] } }
                    ]
                  }
                }
              ],
              "total": 1
            }
            """;

        var recorder = new RecordingHandler(_ => JsonResponse(responseBody));
        var factory = new TestHttpClientFactory(new HttpClient(recorder) { BaseAddress = new Uri("https://api.didox.uz/") });
        var operations = new DidoxEdoOperations(
            new TestUserContext(11),
            factory,
            new DidoxTimestampClient(factory));

        var result = await operations.ListInboxAsync(
            new EdoInboxQueryDto { Page = 2, PageSize = 10, HasMarks = true },
            CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal(["mark-1", "mark-2", "mark-3"], item.MarkingCodes);
        Assert.Contains("owner=0", recorder.LastRequest!.RequestUri!.Query);
        Assert.Contains("hasMarks=true", recorder.LastRequest.RequestUri.Query);
    }

    [Fact]
    public async Task DocumentDetailAndFile_UseProviderRoutes()
    {
        var responses = new Queue<HttpResponseMessage>([
            JsonResponse("{\"doc_status\":3}"),
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent([37, 80, 68, 70])
            }
        ]);
        var recorder = new RecordingHandler(_ => responses.Dequeue());
        var factory = new TestHttpClientFactory(new HttpClient(recorder) { BaseAddress = new Uri("https://api.didox.uz/") });
        var operations = new DidoxEdoOperations(
            new TestUserContext(11),
            factory,
            new DidoxTimestampClient(factory));

        var status = await operations.GetStatusAsync("doc-1", CancellationToken.None);
        Assert.Equal(EdoDocumentStatusCode.SIGNED, status.Code);
        Assert.EndsWith("v1/documents/doc-1", recorder.LastRequest!.RequestUri!.PathAndQuery);

        var file = await operations.GetFileAsync("doc-1", CancellationToken.None);
        await using var stream = file.Content;
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);
        Assert.Equal([37, 80, 68, 70], memory.ToArray());
        Assert.EndsWith("v1/documents/view/doc-1/pdf/ru", recorder.LastRequest!.RequestUri!.PathAndQuery);
    }

    [Fact]
    public void MarkingMapper_ReadsDocumentLevelAndNestedProductMarks()
    {
        using var document = JsonDocument.Parse("""
            {
              "mark_codes": ["root-mark"],
              "marks": { "kiz": "single-mark" },
              "productlist": {
                "products": [
                  { "Marks": { "KIZ": ["nested-mark"] } }
                ]
              }
            }
            """);

        var codes = DidoxDocumentResponseMapper.ReadMarkingCodes(document.RootElement);

        Assert.Equal(["root-mark", "single-mark", "nested-mark"], codes);
    }

    private static HttpClient CreateAuthorizedClient(
        IOptions<DidoxOptions> options,
        DidoxTokenCache cache,
        RecordingHandler recorder)
    {
        var handler = new DidoxAuthorizationHandler(
            options,
            cache,
            NullLogger<DidoxAuthorizationHandler>.Instance)
        {
            InnerHandler = recorder
        };

        return new HttpClient(handler) { BaseAddress = new Uri(options.Value.BaseUrl) };
    }

    private static TokenFixture CreateTokenFixture(string partnerToken = "")
    {
        var root = Path.Combine(Path.GetTempPath(), "didox-tests", Guid.NewGuid().ToString("N"));
        var keyRoot = Path.Combine(root, "keys");
        var storageRoot = Path.Combine(root, "tokens");
        Directory.CreateDirectory(keyRoot);

        var dataProtection = DataProtectionProvider.Create(new DirectoryInfo(keyRoot));
        var options = Options.Create(new DidoxOptions
        {
            BaseUrl = "https://api.didox.uz/",
            UsePartnerlessLegacyApi = string.IsNullOrWhiteSpace(partnerToken),
            PartnerToken = partnerToken
        });
        var storageOptions = Options.Create(new DidoxTokenStorageOptions
        {
            RootPath = storageRoot
        });
        var cache = new DidoxTokenCache(dataProtection, storageOptions, new TestHostEnvironment(root));
        return new TokenFixture(root, options, cache);
    }

    private static HttpResponseMessage JsonResponse(string body) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

    private sealed record TokenFixture(
        string Root,
        IOptions<DidoxOptions> Options,
        DidoxTokenCache Cache) : IDisposable
    {
        public void Dispose()
        {
            Cache.Clear();
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }

    private sealed class TestHttpClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastRequestBody { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            LastRequestBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Task.FromResult(responseFactory(request));
        }
    }

    private sealed class TestHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "UnitTests";
        public string ContentRootPath { get; set; } = contentRootPath;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    private sealed class TestUserContext(int organizationId) : IUserContext
    {
        public int? Id => 1;
        public int? RoleId => 1;
        public short? UserKindId => 3;
        public short? LanguageId => null;
        public int? TenantId => 1;
        public int? OrganizationId => organizationId;
        public List<int> AllowedOrganizationIds => [organizationId];
        public int? BranchId => null;
        public bool HasGlobalAccess => false;
    }
}
