using System.Net;
using System.Text;
using Application.Abstractions.Integration;
using Integration.EImzo.Configs;
using Integration.EImzo.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace UnitTests;

public sealed class EImzoVerifierTests
{
    [Fact]
    public async Task VerifyAttachedAsync_ShouldMapPkcs7InfoResponse()
    {
        var expectedDocument = Encoding.UTF8.GetBytes("signed-document");
        using var verifier = CreateVerifier(
            """
            {
              "status": 1,
              "message": "",
              "pkcs7Info": {
                "documentBase64": "c2lnbmVkLWRvY3VtZW50",
                "signers": [
                  {
                    "signingTime": "2026-07-05 10:20:30",
                    "verified": true,
                    "certificateVerified": true,
                    "certificateValidAtSigningTime": true,
                    "certificate": [
                      {
                        "subjectInfo": {
                          "1.2.860.3.16.1.2": "30123456789012",
                          "1.2.860.3.16.1.1": "305465738",
                          "CN": "TEST USER"
                        },
                        "subjectName": "CN=TEST USER,1.2.860.3.16.1.2=30123456789012",
                        "serialNumber": "abc123"
                      }
                    ],
                    "timeStampInfo": {
                      "time": "2026-07-05 10:21:00",
                      "verified": true,
                      "certificateVerified": true
                    }
                  }
                ]
              }
            }
            """);

        var result = await verifier.Service.VerifyAttachedAsync(Encoding.UTF8.GetBytes("pkcs7-data"));

        Assert.True(result.IsSuccess, result.IsSuccess ? string.Empty : result.Error.Description);
        Assert.True(result.Value.IsValid);
        Assert.Equal("CN=TEST USER,1.2.860.3.16.1.2=30123456789012", result.Value.SubjectName);
        Assert.Equal("30123456789012", result.Value.Pinfl);
        Assert.Equal("305465738", result.Value.Inn);
        Assert.Equal("abc123", result.Value.SerialNumber);
        Assert.Equal("Valid", result.Value.OcspStatus);
        Assert.True(result.Value.HasTimestamp);
        Assert.Equal(expectedDocument, result.Value.DocumentBytes);
        Assert.Equal(new DateTime(2026, 7, 5, 10, 21, 0), result.Value.SignedAt);
    }

    [Fact]
    public async Task VerifyDetachedAsync_ShouldSendCombinedPayloadAndMapDetachedResponse()
    {
        const string responseJson =
            """
            {
              "status": 1,
              "message": "",
              "pkcs7Info": {
                "signers": [
                  {
                    "signingTime": "2026-07-05 10:20:30",
                    "verified": true,
                    "certificateVerified": true,
                    "certificateValidAtSigningTime": true,
                    "certificate": [
                      {
                        "subjectInfo": {
                          "1.2.860.3.16.1.2": "30000000000000"
                        },
                        "subjectName": "CN=DETACHED USER",
                        "serialNumber": "detached-1"
                      }
                    ]
                  }
                ]
              }
            }
            """;

        using var verifier = CreateVerifier(responseJson);
        var documentBytes = Encoding.UTF8.GetBytes("doc");
        var pkcs7Bytes = Encoding.UTF8.GetBytes("pkcs7");

        var result = await verifier.Service.VerifyDetachedAsync(documentBytes, pkcs7Bytes);

        Assert.True(result.IsSuccess, result.IsSuccess ? string.Empty : result.Error.Description);
        Assert.True(result.Value.IsValid);
        Assert.Null(result.Value.DocumentBytes);
        Assert.Equal($"{Convert.ToBase64String(documentBytes)}|{Convert.ToBase64String(pkcs7Bytes)}", verifier.Handler.LastBody);
        Assert.Equal("api.example.uz", verifier.Handler.LastHostHeader);
        Assert.Equal("203.0.113.10", verifier.Handler.LastRealIpHeader);
    }

    [Fact]
    public async Task VerifyAttachedAsync_ShouldReturnFailure_WhenServerUrlMissing()
    {
        using var verifier = CreateVerifier("{}", serverUrl: "");

        var result = await verifier.Service.VerifyAttachedAsync(Encoding.UTF8.GetBytes("pkcs7-data"));

        Assert.False(result.IsSuccess);
        Assert.Equal("EImzo.ServerUrlMissing", result.Error.Code);
    }

    private static VerifierContext CreateVerifier(string responseJson, string serverUrl = "http://localhost:8080/")
    {
        var handler = new StubHttpMessageHandler(responseJson);
        var httpClient = new HttpClient(handler);
        var factory = new StubHttpClientFactory(httpClient);
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };

        accessor.HttpContext.Request.Host = new HostString("api.example.uz");
        accessor.HttpContext.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.10");

        var settings = Options.Create(new EImzoSettings
        {
            EImzoServerUrl = serverUrl,
            CertificatePath = "unused",
            CertificatePassword = "unused"
        });

        var service = new EImzoVerifier(factory, accessor, settings, NullLogger<EImzoVerifier>.Instance);
        return new VerifierContext(service, handler, httpClient);
    }

    private sealed class VerifierContext : IDisposable
    {
        private readonly HttpClient _httpClient;

        public VerifierContext(EImzoVerifier service, StubHttpMessageHandler handler, HttpClient httpClient)
        {
            Service = service;
            Handler = handler;
            _httpClient = httpClient;
        }

        public EImzoVerifier Service { get; }
        public StubHttpMessageHandler Handler { get; }

        public void Dispose()
        {
            _httpClient.Dispose();
            Handler.Dispose();
        }
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public StubHttpClientFactory(HttpClient client)
        {
            _client = client;
        }

        public HttpClient CreateClient(string name) => _client;
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseJson;

        public StubHttpMessageHandler(string responseJson)
        {
            _responseJson = responseJson;
        }

        public string? LastBody { get; private set; }
        public string? LastHostHeader { get; private set; }
        public string? LastRealIpHeader { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            LastHostHeader = request.Headers.Host;
            LastRealIpHeader = request.Headers.TryGetValues("X-Real-IP", out var values)
                ? values.FirstOrDefault()
                : null;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_responseJson, Encoding.UTF8, "application/json")
            };
        }
    }
}
