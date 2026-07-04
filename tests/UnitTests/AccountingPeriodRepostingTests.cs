using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.BankOperations;
using Application.Features.CashOperations;
using Application.Features.InventoryAdjustments;
using Application.Features.InventoryCounts;
using Application.Features.InventoryRegisterBalances;
using Application.Features.PurchaseDocs;
using Application.Features.Reposting;
using Application.Features.SaleDocs;
using Application.Features.WarehouseTransfers;
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

public class AccountingPeriodServiceTests
{
    [Fact]
    public async Task Close_ShouldReject_WhenDraftDocumentsExist()
    {
        var fixture = PeriodTestFixture.Create();
        fixture.ReadRepository.UnconfirmedDocumentCount = 2;

        var result = await fixture.Service.CloseAsync(fixture.Period.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("AccountingPeriod.DraftDocumentsExist", result.Error.Code);
        Assert.Empty(fixture.CommandRepository.UpdatedEntities);
    }

    [Fact]
    public async Task Reopen_ShouldReject_WhenLaterClosedPeriodsExist()
    {
        var fixture = PeriodTestFixture.Create(isClosed: true);
        fixture.ReadRepository.HasLaterClosedPeriods = true;

        var result = await fixture.Service.ReopenAsync(fixture.Period.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("AccountingPeriod.LaterClosedPeriodsExist", result.Error.Code);
        Assert.Empty(fixture.CommandRepository.UpdatedEntities);
    }
}

public class RepostServiceTests
{
    [Fact]
    public async Task Repost_ShouldReject_WhenDocumentIdProvidedWithoutDocumentType()
    {
        var fixture = RepostTestFixture.Create();

        var result = await fixture.Service.RepostAsync(new RepostFilter
        {
            DocumentId = 100
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("Repost.DocumentTypeRequired", result.Error.Code);
        Assert.Empty(fixture.PurchaseService.CancelledIds);
        Assert.Empty(fixture.PurchaseService.ConfirmedIds);
    }

    [Fact]
    public async Task Repost_ShouldReject_WhenDocumentLockIsBusy()
    {
        var fixture = RepostTestFixture.Create();
        fixture.ReadRepository.Candidates.Add(new RepostCandidate
        {
            DocumentType = DocumentTypeIdConst.PURCHASE,
            DocumentId = 100,
            DocDate = new DateTime(2026, 7, 3)
        });
        fixture.PostingLock.TryAcquireResult = false;

        var result = await fixture.Service.RepostAsync(new RepostFilter
        {
            DocumentType = DocumentTypeIdConst.PURCHASE
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("Repost.AlreadyReposting", result.Error.Code);
        Assert.Empty(fixture.PurchaseService.CancelledIds);
        Assert.Empty(fixture.PurchaseService.ConfirmedIds);
    }

    [Fact]
    public async Task Repost_ShouldCancelThenConfirmPurchaseDocument()
    {
        var fixture = RepostTestFixture.Create();
        fixture.ReadRepository.Candidates.Add(new RepostCandidate
        {
            DocumentType = DocumentTypeIdConst.PURCHASE,
            DocumentId = 100,
            DocDate = new DateTime(2026, 7, 3)
        });

        var result = await fixture.Service.RepostAsync(new RepostFilter
        {
            DocumentType = DocumentTypeIdConst.PURCHASE
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.ProcessedCount);
        Assert.Equal([100L], fixture.PurchaseService.CancelledIds);
        Assert.Equal([100L], fixture.PurchaseService.ConfirmedIds);
    }
}

file sealed class PeriodTestFixture
{
    public required AccountingPeriodService Service { get; init; }
    public required AccountingPeriod Period { get; init; }
    public required PeriodReadRepository ReadRepository { get; init; }
    public required PeriodCommandRepository<AccountingPeriod> CommandRepository { get; init; }

    public static PeriodTestFixture Create(bool isClosed = false)
    {
        var period = new AccountingPeriod
        {
            Id = 202607,
            OrganizationId = 8,
            Year = 2026,
            Month = 7,
            StartDate = new DateOnly(2026, 7, 1),
            EndDate = new DateOnly(2026, 7, 31),
            IsClosed = isClosed,
            CreatedDate = DateTime.Today
        };

        var readRepository = new PeriodReadRepository();
        var commandRepository = new PeriodCommandRepository<AccountingPeriod>();
        var service = new AccountingPeriodService(
            new PeriodUserContext(),
            new PeriodQueryBuilder(),
            new PeriodAuditLogService(),
            readRepository,
            new PeriodQueryRepository<AccountingPeriod>([period]),
            commandRepository,
            NullLogger<AccountingPeriodService>.Instance,
            new PeriodUnitOfWork());

        return new PeriodTestFixture
        {
            Service = service,
            Period = period,
            ReadRepository = readRepository,
            CommandRepository = commandRepository
        };
    }
}

file sealed class RepostTestFixture
{
    public required RepostService Service { get; init; }
    public required RepostReadRepository ReadRepository { get; init; }
    public required RepostPostingLock PostingLock { get; init; }
    public required RepostPurchaseDocService PurchaseService { get; init; }

    public static RepostTestFixture Create()
    {
        var readRepository = new RepostReadRepository();
        var postingLock = new RepostPostingLock();
        var purchaseService = new RepostPurchaseDocService();
        var service = new RepostService(
            new PeriodUserContext(),
            new PeriodQueryBuilder(),
            postingLock,
            readRepository,
            new PeriodQueryRepository<AccountingPeriod>([]),
            new PeriodQueryRepository<SaleDoc>([]),
            purchaseService,
            new RepostSaleDocService(),
            new RepostBankOperationService(),
            new RepostCashOperationService(),
            new RepostWarehouseTransferService(),
            new RepostInventoryAdjustmentService(),
            new RepostInventoryCountService(),
            NullLogger<RepostService>.Instance,
            new PeriodUnitOfWork());

        return new RepostTestFixture
        {
            Service = service,
            ReadRepository = readRepository,
            PostingLock = postingLock,
            PurchaseService = purchaseService
        };
    }
}

file sealed class PeriodUserContext : IUserContext
{
    public int? Id => 10;
    public int? RoleId => 1;
    public short? LanguageId => LanguageIdConst.UZ;
    public int? OrganizationId => 8;
    public List<int> AllowedOrganizationIds => [8];
    public int? BranchId => null;
    public bool HasGlobalAccess => false;
}

file sealed class PeriodUnitOfWork : IUnitOfWork
{
    public int BeginCount { get; private set; }
    public int CommitCount { get; private set; }
    public int RollbackCount { get; private set; }

    public Task BeginAsync(CancellationToken ct = default)
    {
        BeginCount++;
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

file sealed class PeriodReadRepository : IAccountingPeriodReadRepository
{
    public bool HasOpenPreviousPeriods { get; set; }
    public bool HasLaterClosedPeriods { get; set; }
    public int UnconfirmedDocumentCount { get; set; }
    public bool HasInvalidPostingBatchState { get; set; }

    public Task<bool> HasOpenPreviousPeriodsAsync(int organizationId, DateOnly startDate, CancellationToken ct = default) =>
        Task.FromResult(HasOpenPreviousPeriods);

    public Task<bool> HasLaterClosedPeriodsAsync(int organizationId, DateOnly startDate, CancellationToken ct = default) =>
        Task.FromResult(HasLaterClosedPeriods);

    public Task<int> CountUnconfirmedDocumentsAsync(int organizationId, DateTime dateFrom, DateTime dateTo, CancellationToken ct = default) =>
        Task.FromResult(UnconfirmedDocumentCount);

    public Task<bool> HasInvalidPostingBatchStateAsync(int organizationId, CancellationToken ct = default) =>
        Task.FromResult(HasInvalidPostingBatchState);
}

file sealed class PeriodAuditLogService : IAuditLogService
{
    public void SetOldValues(object oldValues) { }
    public void SetNewValues(object newValues) { }
    public Task CreateAsync(string tableName, string recordId, string operationType, string? comment = null) => Task.CompletedTask;
    public Task<List<AuditLogDto>> GetByRecordAsync(AuditLogFilter filter) => Task.FromResult(new List<AuditLogDto>());
}

file sealed class PeriodCommandRepository<TEntity> : ICommandRepository<TEntity> where TEntity : class
{
    public List<TEntity> UpdatedEntities { get; } = new();

    public Task CreateAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task CreateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => Task.CompletedTask;

    public Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        UpdatedEntities.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
    {
        UpdatedEntities.AddRange(entities);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) => Task.CompletedTask;
    public Task ReloadAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
}

file sealed class PeriodQueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity> _data;

    public PeriodQueryRepository(List<TEntity> data)
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

    public Task<PagedList<TEntity>> GetPagedAsync(PagedQuerySpecification<TEntity> specification, CancellationToken ct = default) =>
        Task.FromResult(new PagedList<TEntity>([], 0));

    public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
        Task.FromResult(new PagedList<TResult>([], 0));
}

file sealed class PeriodQueryBuilder : IQueryBuilder
{
    private static readonly PeriodQueryBuilderResolver Resolver = new();

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

file sealed class PeriodQueryBuilderResolver : IQueryBuilderResolver
{
    public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;
    public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() => new PeriodProjectionBuilder<TEntity, TResult>();
}

file sealed class PeriodProjectionBuilder<TEntity, TResult> : IProjectionBuilder<TEntity, TResult>
{
    public Expression<Func<TEntity, TResult>> Build() => _ => default!;
}

file sealed class RepostReadRepository : IRepostReadRepository
{
    public List<RepostCandidate> Candidates { get; } = new();

    public Task<List<RepostCandidate>> GetCandidatesAsync(RepostReadRequest request, CancellationToken ct = default) =>
        Task.FromResult(Candidates.ToList());
}

file sealed class RepostPostingLock : IDocumentPostingLock
{
    public bool TryAcquireResult { get; set; } = true;

    public Task AcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) => Task.CompletedTask;

    public Task<bool> TryAcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) =>
        Task.FromResult(TryAcquireResult);
}

file sealed class RepostPurchaseDocService : IPurchaseDocService
{
    public List<long> CancelledIds { get; } = new();
    public List<long> ConfirmedIds { get; } = new();

    public Task<Result<PagedResponse<PurchaseDocListDto>>> GetAllAsync(PurchaseDocListFilter filter, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new PagedResponse<PurchaseDocListDto>()));

    public Task<Result<PurchaseDocDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new PurchaseDocDto()));

    public Task<Result<long>> CreateAsync(PurchaseDocCreateDto dto, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(1L));

    public Task<Result> UpdateAsync(long id, PurchaseDocUpdateDto dto, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default)
    {
        ConfirmedIds.Add(id);
        return Task.FromResult(Result.Success());
    }

    public Task<Result> CancelAsync(long id, CancellationToken ct = default)
    {
        CancelledIds.Add(id);
        return Task.FromResult(Result.Success());
    }

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());
}

file sealed class RepostSaleDocService : ISaleDocService
{
    public Task<Result<PagedResponse<SaleDocListDto>>> GetAllAsync(SaleDocListFilter filter, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new PagedResponse<SaleDocListDto>()));

    public Task<Result<SaleDocDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new SaleDocDto()));

    public Task<Result<long>> CreateAsync(SaleDocCreateDto dto, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(1L));

    public Task<Result> UpdateAsync(long id, SaleDocUpdateDto dto, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> WarehouseConfirmAsync(long id, SaleDocWarehouseConfirmDto dto, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> ConfirmAsync(long id, SaleDocConfirmDto dto, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());
}

file sealed class RepostBankOperationService : IBankOperationService
{
    public Task<Result<PagedResponse<BankOperationListDto>>> GetAllAsync(BankOperationListFilter filter, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new PagedResponse<BankOperationListDto>()));

    public Task<Result<BankOperationDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new BankOperationDto()));

    public Task<Result<long>> CreateAsync(BankOperationCreateDto dto, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(1L));

    public Task<Result<List<long>>> CreateManyAsync(BankOperationsCreateDto dto, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new List<long>()));

    public Task<Result> UpdateAsync(long id, BankOperationUpdateDto dto, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());
}

file sealed class RepostCashOperationService : ICashOperationService
{
    public Task<Result<PagedResponse<CashOperationListDto>>> GetAllAsync(CashOperationListFilter filter, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new PagedResponse<CashOperationListDto>()));

    public Task<Result<CashOperationDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new CashOperationDto()));

    public Task<Result<long>> CreateAsync(CashOperationCreateDto dto, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(1L));

    public Task<Result> UpdateAsync(long id, CashOperationUpdateDto dto, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());
}

file sealed class RepostWarehouseTransferService : IWarehouseTransferService
{
    public Task<Result<PagedResponse<WarehouseTransferListDto>>> GetAllAsync(WarehouseTransferListFilter filter, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new PagedResponse<WarehouseTransferListDto>()));

    public Task<Result<WarehouseTransferDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new WarehouseTransferDto()));

    public Task<Result<long>> CreateAsync(WarehouseTransferCreateDto dto, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(1L));

    public Task<Result> UpdateAsync(long id, WarehouseTransferUpdateDto dto, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result<List<WarehouseTransferPostingBatchDto>>> GetPostingBatchesAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new List<WarehouseTransferPostingBatchDto>()));

    public Task<Result<List<InventoryRegisterBalanceListDto>>> GetInventoryMovementsAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new List<InventoryRegisterBalanceListDto>()));
}

file sealed class RepostInventoryAdjustmentService : IInventoryAdjustmentService
{
    public Task<Result<PagedResponse<InventoryAdjustmentListDto>>> GetAllAsync(InventoryAdjustmentListFilter filter, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new PagedResponse<InventoryAdjustmentListDto>()));

    public Task<Result<InventoryAdjustmentDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new InventoryAdjustmentDto()));

    public Task<Result<long>> CreateAsync(InventoryAdjustmentCreateDto dto, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(1L));

    public Task<Result> UpdateAsync(long id, InventoryAdjustmentUpdateDto dto, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result<List<InventoryAdjustmentPostingBatchDto>>> GetPostingBatchesAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new List<InventoryAdjustmentPostingBatchDto>()));

    public Task<Result<List<InventoryRegisterBalanceListDto>>> GetInventoryMovementsAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new List<InventoryRegisterBalanceListDto>()));
}

file sealed class RepostInventoryCountService : IInventoryCountService
{
    public Task<Result<PagedResponse<InventoryCountListDto>>> GetAllAsync(InventoryCountListFilter filter, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new PagedResponse<InventoryCountListDto>()));

    public Task<Result<InventoryCountDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new InventoryCountDto()));

    public Task<Result<long>> CreateAsync(InventoryCountCreateDto dto, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(1L));

    public Task<Result> UpdateAsync(long id, InventoryCountUpdateDto dto, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result<List<InventoryCountPostingBatchDto>>> GetPostingBatchesAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new List<InventoryCountPostingBatchDto>()));

    public Task<Result<List<InventoryRegisterBalanceListDto>>> GetInventoryMovementsAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new List<InventoryRegisterBalanceListDto>()));

    public Task<Result<List<InventoryCountDifferenceDto>>> GetDifferencesAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new List<InventoryCountDifferenceDto>()));
}
