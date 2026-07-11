using System.Text.Json;
using Application.Features.Cmn.AslBelgi.Abstractions;
using Application.Features.Cmn.AslBelgi.DTOs;
using Application.Features.Cmn.AslBelgi.Services;
using SharedKernel.Results;

namespace UnitTests;

// Proves the Asl Belgisi KM order payload shape (camelCase, products array, optional-null omission)
// and the service guards. Real POST/GET need credentials + a real GTIN, so only serialization,
// parsing, and guards are asserted.
public sealed class AslBelgiOrderTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void OrderRequest_Serializes_WithCamelCaseAndProductsArray()
    {
        var request = new AslBelgiOrderRequest
        {
            ProductGroup = "alcohol",
            BusinessPlaceId = 42,
            ReleaseMethodType = "PRIMARY",
            Products =
            [
                new AslBelgiOrderProduct
                {
                    Gtin = "04600000000000",
                    Quantity = 100,
                    CisType = "UNIT",
                    SerialNumberType = "OPERATOR"
                }
            ]
        };

        var json = JsonSerializer.Serialize(request, JsonOptions);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("alcohol", root.GetProperty("productGroup").GetString());
        Assert.Equal(42, root.GetProperty("businessPlaceId").GetInt32());
        Assert.Equal("PRIMARY", root.GetProperty("releaseMethodType").GetString());

        var product = root.GetProperty("products")[0];
        Assert.Equal("04600000000000", product.GetProperty("gtin").GetString());
        Assert.Equal(100, product.GetProperty("quantity").GetInt32());
        Assert.Equal("UNIT", product.GetProperty("cisType").GetString());
        Assert.Equal("OPERATOR", product.GetProperty("serialNumberType").GetString());

        // Optional null blocks are omitted.
        Assert.False(root.TryGetProperty("contractorInfo", out _));
        Assert.False(root.TryGetProperty("isPaid", out _));
        Assert.False(product.TryGetProperty("serialNumbers", out _));
    }

    [Fact]
    public void OrderProduct_SelfMade_IncludesSerialNumbers()
    {
        var product = new AslBelgiOrderProduct
        {
            Gtin = "04600000000000",
            Quantity = 2,
            CisType = "UNIT",
            SerialNumberType = "SELF_MADE",
            SerialNumbers = ["SN1", "SN2"]
        };

        var json = JsonSerializer.Serialize(product, JsonOptions);
        using var doc = JsonDocument.Parse(json);

        var serials = doc.RootElement.GetProperty("serialNumbers");
        Assert.Equal(2, serials.GetArrayLength());
        Assert.Equal("SN1", serials[0].GetString());
    }

    [Fact]
    public void OrderResponse_And_CodesResponse_Parse()
    {
        var order = JsonSerializer.Deserialize<AslBelgiOrderResponse>("""{"orderId":"6f1e-uuid"}""", JsonOptions);
        Assert.Equal("6f1e-uuid", order!.OrderId);

        var codes = JsonSerializer.Deserialize<AslBelgiCodesResponse>("""{"packId":"pack-1","codes":["KM-AAAA","KM-BBBB"]}""", JsonOptions);
        Assert.Equal("pack-1", codes!.PackId);
        Assert.Equal(2, codes.Codes.Count);
        Assert.Equal("KM-AAAA", codes.Codes[0]);
    }

    [Fact]
    public async Task RegisterOrder_Succeeds_WhenValid()
    {
        var client = new FakeClient();
        var service = new AslBelgiService(client, SuccessToken());

        var result = await service.RegisterOrderAsync(ValidOrder());

        Assert.True(result.IsSuccess);
        Assert.Equal("ord-1", result.Value.OrderId);
        Assert.Equal(1, client.RegisterCount);
    }

    [Fact]
    public async Task RegisterOrder_NoProducts_Fails_WithoutCallingClient()
    {
        var client = new FakeClient();
        var service = new AslBelgiService(client, SuccessToken());

        var result = await service.RegisterOrderAsync(new AslBelgiOrderRequest { ProductGroup = "alcohol", ReleaseMethodType = "PRIMARY" });

        Assert.False(result.IsSuccess);
        Assert.Equal("AslBelgi.MissingOrderProducts", result.Error.Code);
        Assert.Equal(0, client.RegisterCount);
    }

    [Fact]
    public async Task RegisterOrder_MissingGtin_Fails_WithoutCallingClient()
    {
        var client = new FakeClient();
        var service = new AslBelgiService(client, SuccessToken());

        var order = ValidOrder();
        var bad = new AslBelgiOrderRequest
        {
            ProductGroup = order.ProductGroup,
            ReleaseMethodType = order.ReleaseMethodType,
            Products = [new AslBelgiOrderProduct { Gtin = "", Quantity = 1, CisType = "UNIT", SerialNumberType = "OPERATOR" }]
        };

        var result = await service.RegisterOrderAsync(bad);

        Assert.False(result.IsSuccess);
        Assert.Equal("AslBelgi.MissingGtin", result.Error.Code);
        Assert.Equal(0, client.RegisterCount);
    }

    [Fact]
    public async Task RegisterOrder_TokenFailure_Propagates_WithoutCallingClient()
    {
        var client = new FakeClient();
        var service = new AslBelgiService(client, FailingToken());

        var result = await service.RegisterOrderAsync(ValidOrder());

        Assert.False(result.IsSuccess);
        Assert.Equal("AslBelgi.CredentialsNotConfigured", result.Error.Code);
        Assert.Equal(0, client.RegisterCount);
    }

    [Fact]
    public async Task GetCodes_MissingOrderId_Fails_WithoutCallingClient()
    {
        var client = new FakeClient();
        var service = new AslBelgiService(client, SuccessToken());

        var result = await service.GetCodesAsync(orderId: "", gtin: "g", quantity: 1, lastPackId: null);

        Assert.False(result.IsSuccess);
        Assert.Equal("AslBelgi.MissingOrderId", result.Error.Code);
        Assert.Equal(0, client.CodesCount);
    }

    private static AslBelgiOrderRequest ValidOrder() => new()
    {
        ProductGroup = "alcohol",
        BusinessPlaceId = 1,
        ReleaseMethodType = "PRIMARY",
        Products = [new AslBelgiOrderProduct { Gtin = "04600000000000", Quantity = 10, CisType = "UNIT", SerialNumberType = "OPERATOR" }]
    };

    private static IAslBelgiTokenProvider SuccessToken() => new FakeTokenProvider(Result.Success("access-token"));

    private static IAslBelgiTokenProvider FailingToken()
        => new FakeTokenProvider(Result.Failure<string>(
            Application.Features.Cmn.AslBelgi.Errors.AslBelgiErrors.CredentialsNotConfigured("no creds")));

    private sealed class FakeTokenProvider : IAslBelgiTokenProvider
    {
        private readonly Result<string> _result;
        public FakeTokenProvider(Result<string> result) => _result = result;
        public Task<Result<string>> GetAccessTokenAsync(CancellationToken ct = default) => Task.FromResult(_result);
    }

    private sealed class FakeClient : IAslBelgiClient
    {
        public int RegisterCount;
        public int CodesCount;

        public Task<AslBelgiOrderResponse> RegisterOrderAsync(AslBelgiOrderRequest request, string accessToken, CancellationToken ct = default)
        {
            RegisterCount++;
            return Task.FromResult(new AslBelgiOrderResponse { OrderId = "ord-1" });
        }

        public Task<IReadOnlyList<AslBelgiOrderInfo>> GetOrdersAsync(AslBelgiOrdersFilter filter, string accessToken, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<AslBelgiOrderInfo>>([]);

        public Task<AslBelgiCodesResponse> GetCodesAsync(string orderId, string? gtin, int? quantity, string? lastPackId, string accessToken, CancellationToken ct = default)
        {
            CodesCount++;
            return Task.FromResult(new AslBelgiCodesResponse { PackId = "p", Codes = ["c"] });
        }

        public Task<AslBelgiCheckApiKeyResponseDto> CheckApiKeyAsync(string tin, string accessToken, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<AslBelgiDocumentResponseDto> GetDocumentAsync(string documentId, string accessToken, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<AslBelgiStatusResponseDto> GetStatusAsync(string identifier, string accessToken, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<AslBelgiRefreshApiKeyResponseDto> RefreshApiKeyAsync(string tin, string accessToken, AslBelgiRefreshApiKeyRequestDto request, CancellationToken ct = default)
            => throw new NotImplementedException();
    }
}
