using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Integration.Didox.Facturas;
using Integration.Didox.Services;
using Integration.Edo.Providers;
using Microsoft.AspNetCore.Http;
using SharedKernel.Exceptions;
using System.Net;
using System.Text;

namespace UnitTests;

public sealed class DidoxStatusParsingTests
{
    [Fact]
    public async Task List_ReadsNestedStatusTwoAsPendingSignature()
    {
        var operations = CreateOperations(JsonResponse("""
            { "data": [{ "doc_id": "remote-1", "data": { "doc_status": 2 } }], "total": 1 }
            """));

        var result = await operations.ListInboxAsync(new EdoInboxQueryDto(), CancellationToken.None);
        var status = Assert.Single(result.Items).Status;

        AssertStatus(status, EdoDocumentStatusCode.PENDING_SIGNATURE);
    }

    [Fact]
    public async Task Detail_ReadsNestedStatusTwoAndConfirmedTotalSum()
    {
        var operations = CreateOperations(JsonResponse("""
            { "doc_id": "remote-1", "data": { "doc_status": 2, "total_sum": "123.45" } }
            """));

        var result = await operations.GetDocumentDetailsAsync(
            EdoDirection.INBOX,
            "FACTURA",
            "remote-1",
            CancellationToken.None);

        AssertStatus(result.Status, EdoDocumentStatusCode.PENDING_SIGNATURE);
        Assert.Equal(123.45m, result.TotalAmount);
        Assert.Null(result.CurrencyCode);
    }

    [Fact]
    public async Task DetailReadsDocumentNestedStatusOneAsPartnerSignaturePending()
    {
        var operations = CreateOperations(JsonResponse("""
            { "doc_id": "remote-1", "data": { "document": { "doc_status": 1 } } }
            """));

        var result = await operations.GetDocumentDetailsAsync(
            EdoDirection.OUTBOX,
            "FACTURA",
            "remote-1",
            CancellationToken.None);

        AssertStatus(result.Status, EdoDocumentStatusCode.PARTNER_SIGNATURE_PENDING, "1");
    }

    [Fact]
    public async Task Status_ReadsNestedStatusTwoAndSetsCheckedAt()
    {
        var operations = CreateOperations(JsonResponse("""
            { "data": { "doc_status": 2 } }
            """));

        var status = await operations.GetStatusAsync("remote-1", CancellationToken.None);

        AssertStatus(status, EdoDocumentStatusCode.PENDING_SIGNATURE);
    }

    [Theory]
    [InlineData("{ \"doc_status\": 2 }")]
    [InlineData("{ \"data\": { \"doc_status\": 2 } }")]
    [InlineData("{ \"data\": { \"json\": { \"doc_status\": 2 } } }")]
    [InlineData("{ \"data\": { \"document_json\": { \"doc_status\": 2 } } }")]
    public async Task StatusReadsDocStatusTwoFromEveryProviderPayloadLayer(string body)
    {
        var operations = CreateOperations(JsonResponse(body));

        var status = await operations.GetStatusAsync("remote-1", CancellationToken.None);

        AssertStatus(status, EdoDocumentStatusCode.PENDING_SIGNATURE);
    }

    [Theory]
    [InlineData("{ \"doc_status\": 1 }")]
    [InlineData("{ \"status\": 1 }")]
    [InlineData("{ \"data\": { \"document\": { \"doc_status\": 1 } } }")]
    [InlineData("{ \"data\": { \"document\": { \"status\": 1 } } }")]
    [InlineData("{ \"data\": { \"json\": { \"doc_status\": 1 } } }")]
    public async Task StatusReadsPartnerSignaturePendingFromSupportedPayloadLayers(string body)
    {
        var operations = CreateOperations(JsonResponse(body));

        var status = await operations.GetStatusAsync("remote-1", CancellationToken.None);

        AssertStatus(status, EdoDocumentStatusCode.PARTNER_SIGNATURE_PENDING, "1");
    }

    [Fact]
    public async Task RemoteOutboxStatusUsesTheSameProviderStatusMapping()
    {
        var operations = CreateOperations(JsonResponse("""
            { "data": { "document_json": { "doc_status": 2 } } }
            """));

        var status = await operations.GetStatusAsync("remote-1", CancellationToken.None);

        AssertStatus(status, EdoDocumentStatusCode.PENDING_SIGNATURE);
    }

    [Fact]
    public async Task MissingStatusReturnsUnknownWithoutProviderValues()
    {
        var operations = CreateOperations(JsonResponse("{ \"data\": {} }"));

        var status = await operations.GetStatusAsync("remote-1", CancellationToken.None);

        Assert.Equal(EdoDocumentStatusCode.UNKNOWN, status.Code);
        Assert.Equal(EdoDocumentStatusCode.UNKNOWN, status.LocalCode);
        Assert.Null(status.ProviderStatusCode);
        Assert.Null(status.ProviderRawStatus);
        Assert.Null(status.CheckedAt);
    }

    [Fact]
    public async Task MissingTotalSumDoesNotCreateAnAmount()
    {
        var operations = CreateOperations(JsonResponse("""
            { "doc_id": "remote-1", "data": { "document_json": { "doc_status": 2 } } }
            """));

        var result = await operations.GetDocumentDetailsAsync(
            EdoDirection.INBOX,
            "FACTURA",
            "remote-1",
            CancellationToken.None);

        Assert.Null(result.TotalAmount);
        Assert.Null(result.CurrencyCode);
    }

    [Fact]
    public async Task MalformedNestedStatusReturnsControlledBadGateway()
    {
        var operations = CreateOperations(JsonResponse("""
            { "data": { "json": { "doc_status": "2" } } }
            """));

        var exception = await Assert.ThrowsAsync<IntegrationHttpException>(
            () => operations.GetStatusAsync("remote-1", CancellationToken.None));

        Assert.Equal(StatusCodes.Status502BadGateway, exception.StatusCode);
    }

    [Fact]
    public async Task ProviderNotFoundReturnsControlledNotFound()
    {
        var operations = CreateOperations(new HttpResponseMessage(HttpStatusCode.NotFound));

        var exception = await Assert.ThrowsAsync<IntegrationHttpException>(
            () => operations.GetStatusAsync("remote-1", CancellationToken.None));

        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
    }

    [Theory]
    [InlineData(3, EdoDocumentStatusCode.SIGNED)]
    [InlineData(4, EdoDocumentStatusCode.REJECTED)]
    [InlineData(5, EdoDocumentStatusCode.DELETED)]
    [InlineData(50, EdoDocumentStatusCode.ARCHIVED)]
    [InlineData(55, EdoDocumentStatusCode.DELETED)]
    [InlineData(60, EdoDocumentStatusCode.AGENT_SIGNATURE_PENDING)]
    public void ExistingDidoxStatusMappingsRemainUnchanged(int providerStatus, EdoDocumentStatusCode expected)
    {
        var status = EdoProviderStatusMapper.MapDidoxStatus(providerStatus);

        Assert.Equal(expected, status.Code);
        Assert.Equal(providerStatus.ToString(), status.ProviderStatusCode);
        Assert.Equal(providerStatus.ToString(), status.ProviderRawStatus);
    }

    [Fact]
    public void EdocsAndFakturaSharedStatusMappingsRemainStable()
    {
        Assert.Equal(
            EdoDocumentStatusCode.SIGNED,
            EdoProviderStatusMapper.MapEdocsStatus("signed").Code);
        Assert.Equal(
            EdoDocumentStatusCode.SIGNED,
            EdoProviderStatusMapper.Map("signed").Code);
    }

    private static void AssertStatus(
        EdoDocumentStatusDto status,
        EdoDocumentStatusCode expected,
        string providerStatusCode = "2")
    {
        Assert.Equal(expected, status.Code);
        Assert.Equal(expected, status.LocalCode);
        Assert.Equal(providerStatusCode, status.ProviderStatusCode);
        Assert.Equal(providerStatusCode, status.ProviderRawStatus);
        Assert.NotNull(status.CheckedAt);
        Assert.False(status.IsReconciliationRequired);
    }

    private static DidoxEdoOperations CreateOperations(HttpResponseMessage response)
    {
        var handler = new RecordingHandler(_ => response);
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.didox.uz/") };
        var factory = new TestHttpClientFactory(client);
        return new DidoxEdoOperations(new TestUserContext(11), factory, new DidoxTimestampClient(factory));
    }

    private static HttpResponseMessage JsonResponse(string body) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(body, Encoding.UTF8, "application/json")
    };

    private sealed class TestHttpClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(responseFactory(request));
    }

    private sealed class TestUserContext(int organizationId) : IUserContext
    {
        public int? Id => 1;
        public int? RoleId => 1;
        public CurrentUserKind UserKind => CurrentUserKind.TenantUser;
        public short? LanguageId => null;
        public int? TenantId => 1;
        public int? OrganizationId => organizationId;
        public List<int> AllowedOrganizationIds => [organizationId];
        public int? BranchId => null;
    }
}
