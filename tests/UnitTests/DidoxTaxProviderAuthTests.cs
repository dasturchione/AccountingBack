using System.Text.Json;
using Application.Abstractions.Integration;
using Application.Features.Cmn.Taxes.Integration.DTOs;
using Integration.Tax.Configs;
using Integration.Tax.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace UnitTests;

// Proves the Didox 2-header auth guards without a real Partner-Authorization token or network access:
// with a valid ЭСФ payload + configured docType, both token fail-paths short-circuit before any
// HTTP client is created.
public sealed class DidoxTaxProviderAuthTests
{
    [Fact]
    public async Task SubmitAsync_WithoutCompanyToken_ReturnsFailureAndSkipsHttp()
    {
        var factory = new ThrowingHttpClientFactory();
        var provider = CreateProvider(factory, partnerToken: "real-partner-token");

        var result = await provider.SubmitAsync(new TaxProviderOperationRequestDto
        {
            ProviderCode = "DIDOX",
            Payload = ValidInvoicePayload(),
            CompanyToken = null
        });

        Assert.False(result.IsSuccessful);
        Assert.Contains("user-key", result.Message);
        Assert.False(factory.WasCalled);
    }

    [Fact]
    public async Task SubmitAsync_WithPlaceholderPartnerToken_ReturnsFailureAndSkipsHttp()
    {
        var factory = new ThrowingHttpClientFactory();
        var provider = CreateProvider(factory, partnerToken: "SET_VIA_ENVIRONMENT");

        var result = await provider.SubmitAsync(new TaxProviderOperationRequestDto
        {
            ProviderCode = "DIDOX",
            Payload = ValidInvoicePayload(),
            CompanyToken = "company-token-360min"
        });

        Assert.False(result.IsSuccessful);
        Assert.Contains("Partner-Authorization", result.Message);
        Assert.False(factory.WasCalled);
    }

    [Fact]
    public async Task SubmitAsync_WithoutFacturaDocType_ReturnsFailureAndSkipsHttp()
    {
        var factory = new ThrowingHttpClientFactory();
        var provider = CreateProvider(factory, partnerToken: "real-partner-token", facturaDocType: "");

        var result = await provider.SubmitAsync(new TaxProviderOperationRequestDto
        {
            ProviderCode = "DIDOX",
            Payload = ValidInvoicePayload(),
            CompanyToken = "company-token-360min"
        });

        Assert.False(result.IsSuccessful);
        Assert.Contains("FacturaDocType", result.Message);
        Assert.False(factory.WasCalled);
    }

    private static string ValidInvoicePayload()
    {
        var invoice = new DidoxInvoiceRequest
        {
            FacturaType = (int)DidoxFacturaType.Standard,
            FacturaDoc = new DidoxFacturaDoc { FacturaNo = "INV-1", FacturaDate = "2026-07-10" }
        };
        return JsonSerializer.Serialize(invoice, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    private static DidoxTaxProvider CreateProvider(IHttpClientFactory factory, string partnerToken, string facturaDocType = "factura")
    {
        var settings = new TaxIntegrationSettings();
        settings.Didox.BaseUrl = "https://testapi3.didox.uz";
        settings.Didox.PartnerToken = partnerToken;
        settings.Didox.FacturaDocType = facturaDocType;

        return new DidoxTaxProvider(
            factory,
            new HttpContextAccessor(),
            Options.Create(settings),
            NullLogger<DidoxTaxProvider>.Instance);
    }

    private sealed class ThrowingHttpClientFactory : IHttpClientFactory
    {
        public bool WasCalled { get; private set; }

        public HttpClient CreateClient(string name)
        {
            WasCalled = true;
            throw new InvalidOperationException("HTTP client must not be created on the auth fail-path.");
        }
    }
}
