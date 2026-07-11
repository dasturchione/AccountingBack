using System.Linq.Expressions;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Cmn.AslBelgi.Abstractions;
using Application.Features.Cmn.AslBelgi.DTOs;
using Application.Features.Cmn.AslBelgi.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace UnitTests;

// Proves the marking business logic (order -> codes -> ProductTable binding) without a real
// Asl Belgisi backend or database: in-memory read port + capturing command repo + no-op UoW.
public sealed class AslBelgiMarkingServiceTests
{
    [Fact]
    public async Task RequestMarking_WithProductGtin_RegistersOrder()
    {
        var repo = new FakeMarkingRepo { Product = new AslBelgiProductMarkingInfo(100, "04600000000000") };
        var asl = new FakeAslBelgiService();
        var service = CreateService(repo, new FakeCommandRepo(), asl);

        var result = await service.RequestMarkingAsync(ValidMarkingRequest());

        Assert.True(result.IsSuccess);
        Assert.Equal("ord-1", result.Value.OrderId);
        Assert.Equal(1, asl.RegisterCount);
    }

    [Fact]
    public async Task RequestMarking_MissingGtin_Fails_WithoutRegistering()
    {
        var repo = new FakeMarkingRepo { Product = new AslBelgiProductMarkingInfo(100, Gtin: null) };
        var asl = new FakeAslBelgiService();
        var service = CreateService(repo, new FakeCommandRepo(), asl);

        var result = await service.RequestMarkingAsync(ValidMarkingRequest()); // no Gtin override

        Assert.False(result.IsSuccess);
        Assert.Equal("AslBelgi.MissingGtinForProduct", result.Error.Code);
        Assert.Equal(0, asl.RegisterCount);
    }

    [Fact]
    public async Task RequestMarking_ProductNotFound_Fails()
    {
        var repo = new FakeMarkingRepo { Product = null };
        var service = CreateService(repo, new FakeCommandRepo(), new FakeAslBelgiService());

        var result = await service.RequestMarkingAsync(ValidMarkingRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal("AslBelgi.ProductNotFound", result.Error.Code);
    }

    [Fact]
    public async Task FetchAndBind_CreatesOneProductTablePerCode()
    {
        var repo = new FakeMarkingRepo
        {
            Product = new AslBelgiProductMarkingInfo(100, "04600000000000"),
            Existing = []
        };
        var command = new FakeCommandRepo();
        var asl = new FakeAslBelgiService { Codes = new AslBelgiCodesResponse { PackId = "pack-1", Codes = ["KM-1", "KM-2", "KM-3"] } };
        var service = CreateService(repo, command, asl);

        var result = await service.FetchAndBindCodesAsync(BindRequest());

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalCodes);
        Assert.Equal(3, result.Value.BoundCount);
        Assert.Equal(0, result.Value.SkippedExisting);
        Assert.Equal("pack-1", result.Value.PackId);

        Assert.Equal(3, command.Created.Count);
        var first = command.Created[0];
        Assert.Equal("KM-1", first.MarkingNumber);
        Assert.Equal(100, first.ProductId);
        Assert.Equal(8, first.OrganizationId);
        Assert.Equal(ProductTableStatusIdConst.IN_STOCK, first.StatusId);
        Assert.Equal(StateIdConst.ACTIVE, first.StateId);
    }

    [Fact]
    public async Task FetchAndBind_Idempotent_SkipsAlreadyBoundCodes()
    {
        var repo = new FakeMarkingRepo
        {
            Product = new AslBelgiProductMarkingInfo(100, "04600000000000"),
            Existing = ["KM-1"] // already bound
        };
        var command = new FakeCommandRepo();
        var asl = new FakeAslBelgiService { Codes = new AslBelgiCodesResponse { PackId = "pack-1", Codes = ["KM-1", "KM-2"] } };
        var service = CreateService(repo, command, asl);

        var result = await service.FetchAndBindCodesAsync(BindRequest());

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCodes);
        Assert.Equal(1, result.Value.BoundCount);
        Assert.Equal(1, result.Value.SkippedExisting);

        var row = Assert.Single(command.Created);
        Assert.Equal("KM-2", row.MarkingNumber);
    }

    [Fact]
    public async Task FetchAndBind_NoCodes_BindsNothing()
    {
        var repo = new FakeMarkingRepo { Product = new AslBelgiProductMarkingInfo(100, "04600000000000") };
        var command = new FakeCommandRepo();
        var asl = new FakeAslBelgiService { Codes = new AslBelgiCodesResponse { PackId = "pack-1", Codes = [] } };
        var service = CreateService(repo, command, asl);

        var result = await service.FetchAndBindCodesAsync(BindRequest());

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.BoundCount);
        Assert.Empty(command.Created);
    }

    private static AslBelgiMarkingRequestDto ValidMarkingRequest() => new()
    {
        ProductId = 100,
        Quantity = 3,
        ProductGroup = "alcohol",
        ReleaseMethodType = "PRIMARY",
        CisType = "UNIT",
        SerialNumberType = "OPERATOR",
        BusinessPlaceId = 1
    };

    private static AslBelgiBindCodesRequestDto BindRequest() => new()
    {
        OrderId = "ord-1",
        ProductId = 100,
        Quantity = 3
    };

    private static AslBelgiMarkingService CreateService(
        IAslBelgiMarkingRepository repo,
        ICommandRepository<ProductTable> command,
        IAslBelgiService asl)
        => new(
            new FakeUserContext(),
            repo,
            command,
            asl,
            NullLogger<AslBelgiMarkingService>.Instance,
            new FakeUnitOfWork());

    private sealed class FakeUserContext : IUserContext
    {
        public int? Id => 1;
        public int? RoleId => 1;
        public short? LanguageId => 1;
        public int? OrganizationId => 8;
        public List<int> AllowedOrganizationIds => [8];
        public int? BranchId => null;
        public bool HasGlobalAccess => false;
    }

    private sealed class FakeMarkingRepo : IAslBelgiMarkingRepository
    {
        public AslBelgiProductMarkingInfo? Product;
        public IReadOnlyCollection<string> Existing = [];

        public Task<AslBelgiProductMarkingInfo?> GetProductForMarkingAsync(int productId, int organizationId, CancellationToken ct = default)
            => Task.FromResult(Product);

        public Task<IReadOnlyCollection<string>> GetExistingMarkingsAsync(int organizationId, IReadOnlyCollection<string> markings, CancellationToken ct = default)
            => Task.FromResult(Existing);
    }

    private sealed class FakeCommandRepo : ICommandRepository<ProductTable>
    {
        public readonly List<ProductTable> Created = [];

        public Task CreateAsync(ProductTable entity, CancellationToken ct = default) { Created.Add(entity); return Task.CompletedTask; }
        public Task CreateAsync(IEnumerable<ProductTable> entities, CancellationToken ct = default) { Created.AddRange(entities); return Task.CompletedTask; }
        public Task UpdateAsync(ProductTable entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task UpdateAsync(IEnumerable<ProductTable> entities, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(ProductTable entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(IEnumerable<ProductTable> entities, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Expression<Func<ProductTable, bool>> predicate, CancellationToken ct = default) => Task.CompletedTask;
        public Task ReloadAsync(ProductTable entity, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class FakeAslBelgiService : IAslBelgiService
    {
        public int RegisterCount;
        public AslBelgiCodesResponse Codes = new() { PackId = "p", Codes = [] };

        public Task<Result<AslBelgiOrderResponse>> RegisterOrderAsync(AslBelgiOrderRequest request, CancellationToken ct = default)
        {
            RegisterCount++;
            return Task.FromResult(Result.Success(new AslBelgiOrderResponse { OrderId = "ord-1" }));
        }

        public Task<Result<AslBelgiCodesResponse>> GetCodesAsync(string orderId, string? gtin, int? quantity, string? lastPackId, CancellationToken ct = default)
            => Task.FromResult(Result.Success(Codes));

        public Task<Result<AslBelgiCheckApiKeyResponseDto>> CheckApiKeyAsync(string tin, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<IReadOnlyList<AslBelgiOrderInfo>>> GetOrdersAsync(AslBelgiOrdersFilter filter, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<AslBelgiDocumentResponseDto>> GetDocumentAsync(string documentId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<AslBelgiStatusResponseDto>> GetStatusAsync(string identifier, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<AslBelgiRefreshApiKeyResponseDto>> RefreshApiKeyAsync(AslBelgiRefreshApiKeyRequestDto request, CancellationToken ct = default) => throw new NotImplementedException();
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
    }
}
