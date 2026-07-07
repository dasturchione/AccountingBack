using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.FaAssets;
using Application.Features.FaMovements;
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

public class FaMovementCrudTests
{
    [Fact]
    public async Task Create_ShouldReject_WhenTargetMatchesCurrentOwnership()
    {
        var fixture = FaMovementCrudFixture.Create();
        fixture.CreateDto.ToDepartmentId = 10;
        fixture.CreateDto.ToResponsibleUserId = 100;

        var result = await fixture.Service.CreateAsync(fixture.CreateDto);

        Assert.False(result.IsSuccess);
        Assert.Equal("FaMovement.NoTargetChange", result.Error.Code);
    }
}

public class FaMovementLifecycleTests
{
    [Fact]
    public async Task Confirm_ShouldUpdateAssetOwnership_AndMarkPosted()
    {
        var fixture = FaMovementLifecycleFixture.Create();

        var result = await fixture.LifecycleService.ConfirmAsync(fixture.Doc.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, fixture.PostingLock.AcquireCount);
        Assert.Equal(DocumentStatusIdConst.POSTED, fixture.Doc.StatusId);
        Assert.Equal(20, fixture.Asset.DepartmentId);
        Assert.Equal(200, fixture.Asset.ResponsibleUserId);
        Assert.Equal(1, fixture.UnitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Cancel_ShouldRestoreAssetOwnership_AndMarkCancelled()
    {
        var fixture = FaMovementLifecycleFixture.Create();
        var confirm = await fixture.LifecycleService.ConfirmAsync(fixture.Doc.Id);
        Assert.True(confirm.IsSuccess);

        var result = await fixture.LifecycleService.CancelAsync(fixture.Doc.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, fixture.PostingLock.AcquireCount);
        Assert.Equal(DocumentStatusIdConst.CANCELLED, fixture.Doc.StatusId);
        Assert.Equal(10, fixture.Asset.DepartmentId);
        Assert.Equal(100, fixture.Asset.ResponsibleUserId);
    }
}

file sealed class FaMovementCrudFixture
{
    public required FaMovementService Service { get; init; }
    public required FaMovementCreateDto CreateDto { get; init; }

    public static FaMovementCrudFixture Create()
    {
        var docs = new List<FaMovementDoc>();
        var assets = new List<FaAsset> { FaMovementTestData.BuildAsset() };
        var departments = FaMovementTestData.BuildDepartments();
        var users = FaMovementTestData.BuildUsers();

        var service = new FaMovementService(
            new FakeMovementUserContext(),
            new FakeMovementUnitOfWork(),
            new FakeMovementQueryBuilder(),
            new FakeMovementAuditLogService(),
            new FakeMovementLifecycleService(),
            new FakeMovementDocNumberGenerator(),
            new FakeMovementQueryRepository<FaMovementDoc>(docs),
            new FakeMovementQueryRepository<FaAsset>(assets),
            new FakeMovementQueryRepository<Department>(departments),
            new FakeMovementQueryRepository<User>(users),
            new FakeMovementCommandRepository(docs),
            NullLogger<FaMovementService>.Instance);

        return new FaMovementCrudFixture
        {
            Service = service,
            CreateDto = FaMovementTestData.BuildCreateDto()
        };
    }
}

file sealed class FaMovementLifecycleFixture
{
    public required FaMovementLifecycleService LifecycleService { get; init; }
    public required FakeMovementPostingLock PostingLock { get; init; }
    public required FakeMovementUnitOfWork UnitOfWork { get; init; }
    public required FaMovementDoc Doc { get; init; }
    public required FaAsset Asset { get; init; }

    public static FaMovementLifecycleFixture Create()
    {
        var asset = FaMovementTestData.BuildAsset();
        var doc = FaMovementTestData.BuildDoc(asset);
        var docs = new List<FaMovementDoc> { doc };
        var unitOfWork = new FakeMovementUnitOfWork();
        var postingLock = new FakeMovementPostingLock();

        var service = new FaMovementLifecycleService(
            new FakeMovementUserContext(),
            unitOfWork,
            new FakeMovementQueryBuilder(),
            postingLock,
            new FakeMovementAccountingPeriodValidator(),
            new FakeMovementAuditLogService(),
            new FakeMovementQueryRepository<FaMovementDoc>(docs),
            new FakeMovementCommandRepository(docs),
            new FakeMovementAssetCommandRepository(new List<FaAsset> { asset }),
            NullLogger<FaMovementLifecycleService>.Instance);

        return new FaMovementLifecycleFixture
        {
            LifecycleService = service,
            PostingLock = postingLock,
            UnitOfWork = unitOfWork,
            Doc = doc,
            Asset = asset
        };
    }
}

file static class FaMovementTestData
{
    public static FaMovementCreateDto BuildCreateDto() => new()
    {
        DocDate = DateTime.Today,
        ToDepartmentId = 20,
        ToResponsibleUserId = 200,
        Note = "move",
        Lines =
        [
            new FaMovementLineWriteDto
            {
                FaAssetId = 1000,
                Note = "asset"
            }
        ]
    };

    public static List<Department> BuildDepartments() =>
    [
        new Department { Id = 10, OrganizationId = 8, Code = "D1", Name = "Dept 1", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
        new Department { Id = 20, OrganizationId = 8, Code = "D2", Name = "Dept 2", StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
    ];

    public static List<User> BuildUsers() =>
    [
        new User { Id = 100, OrganizationId = 8, UserName = "u1", PasswordHash = "h", PasswordSalt = "s", PhoneNumber = "1", FirstName = "A", LastName = "One", RoleId = 1, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today },
        new User { Id = 200, OrganizationId = 8, UserName = "u2", PasswordHash = "h", PasswordSalt = "s", PhoneNumber = "2", FirstName = "B", LastName = "Two", RoleId = 1, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Today }
    ];

    public static FaAsset BuildAsset() => new()
    {
        Id = 1000,
        OrganizationId = 8,
        StateId = StateIdConst.ACTIVE,
        InventoryNumber = "FA-1000",
        Name = "Asset",
        FaGroupId = 1,
        DepreciationMethodId = 1,
        UsefulLifeMonths = 60,
        InitialCost = 1000,
        SalvageValue = 0,
        DepartmentId = 10,
        ResponsibleUserId = 100,
        StatusId = FaAssetStatusIdConst.ACTIVE,
        CreatedDate = DateTime.Today,
        UpdatedDate = DateTime.Today,
        Status = new FaAssetStatus { Id = FaAssetStatusIdConst.ACTIVE, Code = "ACTIVE", Name = "Active", StateId = StateIdConst.ACTIVE }
    };

    public static FaMovementDoc BuildDoc(FaAsset asset)
    {
        var line = new FaMovementDocLine
        {
            Id = 1,
            MovementDocId = 1,
            FaAssetId = asset.Id,
            FaAsset = asset,
            Note = "asset line"
        };

        var doc = new FaMovementDoc
        {
            Id = 1,
            OrganizationId = 8,
            StateId = StateIdConst.ACTIVE,
            DocNumber = "FAM-1",
            DocDate = DateTime.Today,
            StatusId = DocumentStatusIdConst.DRAFT,
            FromDepartmentId = 10,
            ToDepartmentId = 20,
            FromResponsibleUserId = 100,
            ToResponsibleUserId = 200,
            CreatedDate = DateTime.Today,
            UpdatedDate = DateTime.Today,
            Lines = [line]
        };

        line.MovementDoc = doc;
        return doc;
    }
}

file sealed class FakeMovementUserContext : IUserContext
{
    public int? Id => 10;
    public int? RoleId => 1;
    public short? LanguageId => 1;
    public int? OrganizationId => 8;
    public List<int> AllowedOrganizationIds => [8];
    public int? BranchId => null;
    public bool HasGlobalAccess => false;
}

file sealed class FakeMovementUnitOfWork : IUnitOfWork
{
    public int BeginCount { get; private set; }
    public int SaveChangesCount { get; private set; }
    public int CommitCount { get; private set; }
    public int RollbackCount { get; private set; }

    public Task BeginAsync(CancellationToken ct = default)
    {
        BeginCount++;
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }

    public Task CommitAsync(CancellationToken ct = default)
    {
        CommitCount++;
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken ct = default)
    {
        RollbackCount++;
        return Task.CompletedTask;
    }
}

file sealed class FakeMovementDocNumberGenerator : IDocNumberGenerator
{
    public Task<string> GenerateAsync(int organizationId, string prefix, DateTime docDate, CancellationToken ct = default) =>
        Task.FromResult($"{prefix}-000001");
}

file sealed class FakeMovementLifecycleService : IFaMovementLifecycleService
{
    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) => Task.FromResult(Result.Success());
    public Task<Result> CancelAsync(long id, CancellationToken ct = default) => Task.FromResult(Result.Success());
}

file sealed class FakeMovementPostingLock : IDocumentPostingLock
{
    public int AcquireCount { get; private set; }

    public Task AcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default)
    {
        AcquireCount++;
        return Task.CompletedTask;
    }

    public Task<bool> TryAcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) =>
        Task.FromResult(true);
}

file sealed class FakeMovementAccountingPeriodValidator : IAccountingPeriodValidator
{
    public Task<Result> EnsureOpenAsync(int organizationId, DateTime date, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());
}

file sealed class FakeMovementAuditLogService : IAuditLogService
{
    public void SetOldValues(object oldValues) { }
    public void SetNewValues(object newValues) { }
    public Task CreateAsync(string tableName, string recordId, string operationType, string? comment = null) => Task.CompletedTask;
    public Task<List<AuditLogDto>> GetByRecordAsync(AuditLogFilter filter) => Task.FromResult(new List<AuditLogDto>());
}

file sealed class FakeMovementCommandRepository : IFaMovementCommandRepository
{
    private readonly List<FaMovementDoc> _docs;

    public FakeMovementCommandRepository(List<FaMovementDoc> docs)
    {
        _docs = docs;
    }

    public List<FaMovementDoc> CreatedDocs { get; } = new();
    public List<FaMovementDoc> UpdatedDocs { get; } = new();

    public Task CreateAsync(FaMovementDoc entity, CancellationToken ct = default)
    {
        if (entity.Id == 0)
            entity.Id = _docs.Count + 1;
        foreach (var line in entity.Lines)
            line.MovementDocId = entity.Id;
        CreatedDocs.Add(entity);
        _docs.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(FaMovementDoc entity, CancellationToken ct = default)
    {
        UpdatedDocs.Add(entity);
        return Task.CompletedTask;
    }

    public Task DeleteLinesAsync(IEnumerable<FaMovementDocLine> entities, CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class FakeMovementAssetCommandRepository : IFaAssetCommandRepository
{
    private readonly List<FaAsset> _assets;

    public FakeMovementAssetCommandRepository(List<FaAsset> assets)
    {
        _assets = assets;
    }

    public Task CreateAsync(FaAsset entity, CancellationToken ct = default)
    {
        _assets.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(FaAsset entity, CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class FakeMovementQueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity> _data;

    public FakeMovementQueryRepository(List<TEntity> data)
    {
        _data = data;
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
        Task.FromResult(_data.AsQueryable().Any(predicate));

    public Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
        Task.FromResult(_data.AsQueryable().Where(specification.Criteria).FirstOrDefault());

    public Task<TResult?> GetAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
        Task.FromResult(_data.AsQueryable().Where(specification.Criteria).Select(specification.Selector).FirstOrDefault());

    public Task<List<TEntity>> GetAllAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
        Task.FromResult(_data.AsQueryable().Where(specification.Criteria).ToList());

    public Task<List<TResult>> GetAllAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
        Task.FromResult(_data.AsQueryable().Where(specification.Criteria).Select(specification.Selector).ToList());

    public Task<PagedList<TEntity>> GetPagedAsync(PagedQuerySpecification<TEntity> specification, CancellationToken ct = default)
    {
        var take = specification.Take.GetValueOrDefault(50);
        var filtered = _data.AsQueryable().Where(specification.Criteria);
        return Task.FromResult(new PagedList<TEntity>(filtered.Skip(specification.Skip).Take(take).ToList(), filtered.Count()));
    }

    public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default)
    {
        var filtered = _data.AsQueryable().Where(specification.Criteria).Select(specification.Selector);
        var take = specification.Take.GetValueOrDefault(50);
        var items = filtered.Where(specification.ResultCriteria).Skip(specification.Skip).Take(take).ToList();
        var total = filtered.Count(specification.ResultCriteria);
        return Task.FromResult(new PagedList<TResult>(items, total));
    }
}

file sealed class FakeMovementQueryBuilder : IQueryBuilder
{
    private static readonly FakeMovementQueryBuilderResolver Resolver = new();

    public EntityQueryBuilder<TEntity> For<TEntity>() where TEntity : class =>
        new(new QueryState<TEntity> { Resolver = Resolver });

    public QuerySpecification<TEntity> Build<TEntity, TOptions>(TOptions options) where TEntity : class =>
        new() { Criteria = _ => true };

    public QuerySpecification<TEntity, TResult> Build<TEntity, TResult, TOptions>(TOptions options) where TEntity : class =>
        new() { Criteria = _ => true, ResultCriteria = _ => true, Selector = _ => default! };

    public PagedQuerySpecification<TEntity> BuildPaged<TEntity, TOptions>(TOptions options)
        where TEntity : class
        where TOptions : SharedKernel.Filters.IPaginationFilter =>
        new() { Criteria = _ => true, Skip = 0, Take = 50 };

    public PagedQuerySpecification<TEntity, TResult> BuildPaged<TEntity, TResult, TOptions>(TOptions options)
        where TEntity : class
        where TOptions : SharedKernel.Filters.IPaginationFilter =>
        new() { Criteria = _ => true, ResultCriteria = _ => true, Selector = _ => default!, Skip = 0, Take = 50 };
}

file sealed class FakeMovementQueryBuilderResolver : IQueryBuilderResolver
{
    public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
    public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => new FakeMovementProjectionBuilder<TEntity, TResult>();
}

file sealed class FakeMovementProjectionBuilder<TEntity, TResult> : IProjectionBuilder<TEntity, TResult>
{
    public Expression<Func<TEntity, TResult>> Build() => _ => default!;
}
