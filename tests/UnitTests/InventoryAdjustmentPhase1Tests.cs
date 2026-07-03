using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.InventoryAdjustments;
using Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Builders;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using SharedKernel.Results;
using System.Linq.Expressions;

namespace UnitTests;

public class InventoryAdjustmentPhase1Tests
{
    [Fact]
    public async Task Create_ShouldPersistDraftDocument()
    {
        var fixture = InventoryAdjustmentFixture.Create();

        var result = await fixture.Service.CreateAsync(fixture.CreateDto);

        Assert.True(result.IsSuccess);
        Assert.Single(fixture.DocCommand.CreatedEntities);
        var created = fixture.DocCommand.CreatedEntities.Single();
        Assert.Equal(DocumentStatusIdConst.DRAFT, created.StatusId);
        Assert.Equal(StateIdConst.ACTIVE, created.StateId);
    }

    [Fact]
    public async Task Update_ShouldRejectNonDraftDocument()
    {
        var fixture = InventoryAdjustmentFixture.Create();
        fixture.Docs.Add(InventoryAdjustmentTestData.BuildDoc(statusId: DocumentStatusIdConst.POSTED));

        var result = await fixture.Service.UpdateAsync(10, fixture.UpdateDto);

        Assert.False(result.IsSuccess);
        Assert.Equal("InventoryAdjustment.CannotUpdateInCurrentStatus", result.Error.Code);
    }

    [Fact]
    public async Task Delete_ShouldRejectNonDraftDocument()
    {
        var fixture = InventoryAdjustmentFixture.Create();
        fixture.Docs.Add(InventoryAdjustmentTestData.BuildDoc(id: 11, statusId: DocumentStatusIdConst.CANCELLED));

        var result = await fixture.Service.DeleteAsync(11);

        Assert.False(result.IsSuccess);
        Assert.Equal("InventoryAdjustment.CannotDeleteInCurrentStatus", result.Error.Code);
    }

    [Fact]
    public async Task Create_ShouldRejectDuplicateProductTableRows()
    {
        var fixture = InventoryAdjustmentFixture.Create();
        fixture.CreateDto.Lines[0].Items.Add(new InventoryAdjustmentTableRequestDto
        {
            ProductTableId = fixture.CreateDto.Lines[0].Items[0].ProductTableId,
            CostPrice = 12m
        });
        fixture.CreateDto.Lines[0].Quantity = 2;

        var result = await fixture.Service.CreateAsync(fixture.CreateDto);

        Assert.False(result.IsSuccess);
        Assert.Equal("InventoryAdjustment.DuplicateProductTable", result.Error.Code);
    }

    [Fact]
    public async Task Create_ShouldRejectDuplicateLines()
    {
        var fixture = InventoryAdjustmentFixture.Create();
        fixture.CreateDto.Lines.Add(new InventoryAdjustmentLineRequestDto
        {
            ProductId = 100,
            UnitId = 1,
            Quantity = 1,
            Items = [new InventoryAdjustmentTableRequestDto { ProductTableId = 501, CostPrice = 20m }]
        });
        fixture.ProductTables.Add(new ProductTable
        {
            Id = 501,
            ProductId = 100,
            OrganizationId = 8,
            CurrentWarehouseId = 1,
            StateId = StateIdConst.ACTIVE,
            StatusId = ProductTableStatusIdConst.IN_STOCK,
            CreatedDate = DateTime.Today
        });

        var result = await fixture.Service.CreateAsync(fixture.CreateDto);

        Assert.False(result.IsSuccess);
        Assert.Equal("InventoryAdjustment.DuplicateLine", result.Error.Code);
    }

    [Fact]
    public async Task Create_ShouldRejectWarehouseOwnershipMismatch()
    {
        var fixture = InventoryAdjustmentFixture.Create();
        fixture.Warehouses.Clear();
        fixture.Warehouses.Add(new Warehouse
        {
            Id = 1,
            OrganizationId = 99,
            Name = "Other",
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Today
        });

        var result = await fixture.Service.CreateAsync(fixture.CreateDto);

        Assert.False(result.IsSuccess);
        Assert.Equal("InventoryAdjustment.WarehouseOrganizationMismatch", result.Error.Code);
    }

    [Fact]
    public async Task Create_ShouldRejectProductTableWarehouseMismatch()
    {
        var fixture = InventoryAdjustmentFixture.Create();
        fixture.ProductTables.Single().CurrentWarehouseId = 2;

        var result = await fixture.Service.CreateAsync(fixture.CreateDto);

        Assert.False(result.IsSuccess);
        Assert.Equal("InventoryAdjustment.ProductTableWarehouseMismatch", result.Error.Code);
    }

    [Fact]
    public void Validator_ShouldRejectEmptyAdjustmentType()
    {
        var validator = new InventoryAdjustmentCreateDtoValidator();
        var dto = InventoryAdjustmentTestData.BuildCreateDto();
        dto.AdjustmentType = "";

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }
}

file sealed class InventoryAdjustmentFixture
{
    public required InventoryAdjustmentService Service { get; init; }
    public required FakeInventoryAdjustmentCommandRepository<InventoryAdjustmentDoc> DocCommand { get; init; }
    public required List<InventoryAdjustmentDoc> Docs { get; init; }
    public required List<Warehouse> Warehouses { get; init; }
    public required List<ProductTable> ProductTables { get; init; }
    public required InventoryAdjustmentCreateDto CreateDto { get; init; }
    public required InventoryAdjustmentUpdateDto UpdateDto { get; init; }

    public static InventoryAdjustmentFixture Create()
    {
        var data = InventoryAdjustmentTestData.CreateBaseData();
        var userContext = new FakeInventoryAdjustmentUserContext();
        var unitOfWork = new FakeInventoryAdjustmentUnitOfWork();
        var docCommand = new FakeInventoryAdjustmentCommandRepository<InventoryAdjustmentDoc>(data.Docs, x => x.Id = x.Id == 0 ? 999 : x.Id);

        var service = new InventoryAdjustmentService(
            userContext,
            new FakeInventoryAdjustmentQueryBuilder(),
            new FakeInventoryAdjustmentAuditLogService(),
            new FakeInventoryAdjustmentLifecycleService(),
            new FakeInventoryAdjustmentDocNumberGenerator(),
            new FakeInventoryAdjustmentQueryRepository<InventoryAdjustmentDoc>(data.Docs),
            docCommand,
            new FakeInventoryAdjustmentCommandRepository<InventoryAdjustmentLine>(),
            new FakeInventoryAdjustmentCommandRepository<InventoryAdjustmentDocTable>(),
            new FakeInventoryAdjustmentQueryRepository<PostingBatch>(new List<PostingBatch>()),
            new FakeInventoryAdjustmentQueryRepository<RegisterBalance>(new List<RegisterBalance>()),
            new FakeInventoryAdjustmentQueryRepository<Organization>(data.Organizations),
            new FakeInventoryAdjustmentQueryRepository<Warehouse>(data.Warehouses),
            new FakeInventoryAdjustmentQueryRepository<Product>(data.Products),
            new FakeInventoryAdjustmentQueryRepository<Unit>(data.Units),
            new FakeInventoryAdjustmentQueryRepository<ProductTable>(data.ProductTables),
            NullLogger<InventoryAdjustmentService>.Instance,
            unitOfWork);

        return new InventoryAdjustmentFixture
        {
            Service = service,
            DocCommand = docCommand,
            Docs = data.Docs,
            Warehouses = data.Warehouses,
            ProductTables = data.ProductTables,
            CreateDto = InventoryAdjustmentTestData.BuildCreateDto(),
            UpdateDto = InventoryAdjustmentTestData.BuildUpdateDto()
        };
    }
}

file static class InventoryAdjustmentTestData
{
    public static InventoryAdjustmentCreateDto BuildCreateDto() => new()
    {
        DocDate = DateTime.Today,
        WarehouseId = 1,
        AdjustmentType = "WRITE_OFF",
        Comment = "draft",
        Lines =
        [
            new InventoryAdjustmentLineRequestDto
            {
                ProductId = 100,
                UnitId = 1,
                Quantity = 1,
                Comment = "line",
                Items =
                [
                    new InventoryAdjustmentTableRequestDto
                    {
                        ProductTableId = 500,
                        CostPrice = 15m
                    }
                ]
            }
        ]
    };

    public static InventoryAdjustmentUpdateDto BuildUpdateDto() => new()
    {
        DocDate = DateTime.Today,
        WarehouseId = 1,
        AdjustmentType = "DAMAGE",
        Comment = "updated",
        StateId = StateIdConst.ACTIVE,
        Lines = BuildCreateDto().Lines
    };

    public static (List<InventoryAdjustmentDoc> Docs, List<Organization> Organizations, List<Warehouse> Warehouses, List<Product> Products, List<Unit> Units, List<ProductTable> ProductTables) CreateBaseData()
    {
        var docs = new List<InventoryAdjustmentDoc>();
        var organizations = new List<Organization>
        {
            new() { Id = 8, ShortName = "Org", FullName = "Org", Inn = "123", RegionId = 1, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today, SetupStatus = "DONE" }
        };
        var warehouses = new List<Warehouse>
        {
            new() { Id = 1, OrganizationId = 8, Name = "Main", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today, IsMain = true }
        };
        var products = new List<Product>
        {
            new() { Id = 100, OrganizationId = 8, Name = "Product", UnitId = 1, IsService = false, IsPieceTracked = true, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
        };
        var units = new List<Unit>
        {
            new() { Id = 1, Code = "pcs", Name = "pcs", StateId = StateIdConst.ACTIVE }
        };
        var productTables = new List<ProductTable>
        {
            new() { Id = 500, ProductId = 100, OrganizationId = 8, CurrentWarehouseId = 1, StateId = StateIdConst.ACTIVE, StatusId = ProductTableStatusIdConst.IN_STOCK, CreatedDate = DateTime.Today, MarkingNumber = "M1" }
        };

        return (docs, organizations, warehouses, products, units, productTables);
    }

    public static InventoryAdjustmentDoc BuildDoc(long id = 10, short statusId = DocumentStatusIdConst.DRAFT) =>
        new()
        {
            Id = id,
            OrganizationId = 8,
            DocNumber = $"IAD-{id}",
            DocDate = DateTime.Today,
            WarehouseId = 1,
            AdjustmentType = "WRITE_OFF",
            StatusId = statusId,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Today
        };
}

file sealed class FakeInventoryAdjustmentUserContext : IUserContext
{
    public int? Id => 10;
    public int? RoleId => 1;
    public short? LanguageId => 1;
    public int? OrganizationId => 8;
    public List<int> AllowedOrganizationIds => [8];
    public int? BranchId => null;
    public bool HasGlobalAccess => false;
}

file sealed class FakeInventoryAdjustmentUnitOfWork : IUnitOfWork
{
    public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class FakeInventoryAdjustmentDocNumberGenerator : IDocNumberGenerator
{
    public Task<string> GenerateAsync(int organizationId, string prefix, DateTime docDate, CancellationToken ct = default) =>
        Task.FromResult($"{prefix}-000001");
}

file sealed class FakeInventoryAdjustmentAuditLogService : IAuditLogService
{
    public void SetOldValues(object oldValues) { }
    public void SetNewValues(object newValues) { }
    public Task CreateAsync(string tableName, string recordId, string operationType, string? comment = null) => Task.CompletedTask;
    public Task<List<AuditLogDto>> GetByRecordAsync(AuditLogFilter filter) => Task.FromResult(new List<AuditLogDto>());
}

file sealed class FakeInventoryAdjustmentLifecycleService : IInventoryAdjustmentLifecycleService
{
    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) => Task.FromResult(Result.Success());
    public Task<Result> CancelAsync(long id, CancellationToken ct = default) => Task.FromResult(Result.Success());
}

file sealed class FakeInventoryAdjustmentCommandRepository<TEntity> : ICommandRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity>? _store;
    private readonly Action<TEntity>? _onCreate;

    public FakeInventoryAdjustmentCommandRepository(List<TEntity>? store = null, Action<TEntity>? onCreate = null)
    {
        _store = store;
        _onCreate = onCreate;
    }

    public List<TEntity> CreatedEntities { get; } = new();

    public Task CreateAsync(TEntity entity, CancellationToken ct = default)
    {
        _onCreate?.Invoke(entity);
        CreatedEntities.Add(entity);
        _store?.Add(entity);
        return Task.CompletedTask;
    }

    public Task CreateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
    {
        foreach (var entity in entities)
        {
            _onCreate?.Invoke(entity);
            CreatedEntities.Add(entity);
            _store?.Add(entity);
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) => Task.CompletedTask;
    public Task ReloadAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class FakeInventoryAdjustmentQueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity> _data;

    public FakeInventoryAdjustmentQueryRepository(List<TEntity> data)
    {
        _data = data;
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
        Task.FromResult(_data.AsQueryable().Any(predicate));

    public Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
        Task.FromResult(_data.AsQueryable().Where(specification.Criteria).FirstOrDefault());

    public Task<TResult?> GetAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
        Task.FromResult(default(TResult));

    public Task<List<TEntity>> GetAllAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
        Task.FromResult(_data.AsQueryable().Where(specification.Criteria).ToList());

    public Task<List<TResult>> GetAllAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
        Task.FromResult(new List<TResult>());

    public Task<PagedList<TEntity>> GetPagedAsync(PagedQuerySpecification<TEntity> specification, CancellationToken ct = default) =>
        Task.FromResult(new PagedList<TEntity>([], 0));

    public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
        Task.FromResult(new PagedList<TResult>([], 0));
}

file sealed class FakeInventoryAdjustmentQueryBuilder : IQueryBuilder
{
    private static readonly FakeInventoryAdjustmentQueryBuilderResolver Resolver = new();

    public EntityQueryBuilder<TEntity> For<TEntity>() where TEntity : class =>
        new(new QueryState<TEntity> { Resolver = Resolver });

    public QuerySpecification<TEntity> Build<TEntity, TOptions>(TOptions options) where TEntity : class =>
        new() { Criteria = _ => true };

    public QuerySpecification<TEntity, TResult> Build<TEntity, TResult, TOptions>(TOptions options) where TEntity : class =>
        new() { Criteria = _ => true, ResultCriteria = _ => true, Selector = _ => default! };

    public PagedQuerySpecification<TEntity> BuildPaged<TEntity, TOptions>(TOptions options) where TEntity : class where TOptions : SharedKernel.Filters.IPaginationFilter =>
        new() { Criteria = _ => true, Skip = 0, Take = 50 };

    public PagedQuerySpecification<TEntity, TResult> BuildPaged<TEntity, TResult, TOptions>(TOptions options) where TEntity : class where TOptions : SharedKernel.Filters.IPaginationFilter =>
        new() { Criteria = _ => true, ResultCriteria = _ => true, Selector = _ => default!, Skip = 0, Take = 50 };
}

file sealed class FakeInventoryAdjustmentQueryBuilderResolver : IQueryBuilderResolver
{
    public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
    public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => new FakeInventoryAdjustmentProjectionBuilder<TEntity, TResult>();
}

file sealed class FakeInventoryAdjustmentProjectionBuilder<TEntity, TResult> : IProjectionBuilder<TEntity, TResult>
{
    public Expression<Func<TEntity, TResult>> Build() => _ => default!;
}
