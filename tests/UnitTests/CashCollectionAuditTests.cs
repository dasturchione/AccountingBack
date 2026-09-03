using System.Linq.Expressions;
using System.Text.Json;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.BankOperations;
using Application.Features.CashCollections;
using Application.Features.CashFiscalTransfers;
using Application.Features.DocumentNumbers;
using Application.Features.PaymentAcceptancePointOperations;
using Application.Features.Register.AccountingRegisterEntries;
using Domain.Entities;
using Infrastructure.Query;
using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.QueryResults;
using SharedKernel.Results;

namespace UnitTests;

public sealed class CashCollectionAuditTests
{
    [Fact]
    public async Task PaymentAcceptancePointOperationCreateAsync_SavesNewAuditSnapshot()
    {
        var fixture = new PaymentAcceptancePointOperationFixture();

        var result = await fixture.Service.CreateAsync(new PaymentAcceptancePointOperationCreateDto
        {
            PaymentAcceptancePointId = fixture.Point.Id,
            DirectionId = MovementDirectionIdConst.IN,
            DocDate = fixture.DocumentDate,
            CurrencyId = fixture.Currency.Id,
            Amount = 250_000m,
            ExchangeRate = 1m,
            ExternalTransactionNumber = "EXT-42"
        });

        Assert.True(result.IsSuccess);
        var audit = Assert.Single(fixture.AuditLogs.Items);
        Assert.Equal(AuditLogOperationTypeConst.Create, audit.Action);
        Assert.Null(audit.OldData);
        using var newData = JsonDocument.Parse(Assert.IsType<string>(audit.NewData));
        Assert.Equal(result.Value, newData.RootElement.GetProperty("id").GetInt64());
        Assert.Equal(DocumentStatusIdConst.DRAFT, newData.RootElement.GetProperty("statusId").GetInt16());
        Assert.Equal(250_000m, newData.RootElement.GetProperty("amount").GetDecimal());
    }

    [Fact]
    public async Task CashFiscalTransferConfirmAsync_SavesDraftAndPostedAuditSnapshots()
    {
        var fixture = new CashFiscalTransferFixture();

        var result = await fixture.Service.ConfirmAsync(fixture.Document.Id);

        Assert.True(result.IsSuccess);
        var audit = Assert.Single(fixture.AuditLogs.Items);
        Assert.Equal(AuditLogOperationTypeConst.Update, audit.Action);
        using var oldData = JsonDocument.Parse(Assert.IsType<string>(audit.OldData));
        using var newData = JsonDocument.Parse(Assert.IsType<string>(audit.NewData));
        Assert.Equal(DocumentStatusIdConst.DRAFT, oldData.RootElement.GetProperty("statusId").GetInt16());
        Assert.Equal(DocumentStatusIdConst.POSTED, newData.RootElement.GetProperty("statusId").GetInt16());
        Assert.Null(oldData.RootElement.GetProperty("postedAt").GetDateTimeOrNull());
        Assert.NotNull(newData.RootElement.GetProperty("postedAt").GetDateTimeOrNull());
    }

    [Fact]
    public async Task SendToBankAsync_SavesDraftAndInTransitAuditSnapshots()
    {
        var fixture = new CashCollectionFixture();

        var result = await fixture.Service.SendToBankAsync(fixture.Document.Id);

        Assert.True(result.IsSuccess);
        var audit = Assert.Single(fixture.AuditLogs.Items);
        Assert.Equal(AuditLogOperationTypeConst.Update, audit.Action);
        Assert.Equal(AuditLogTableConst.CashCollection, audit.TableName);
        Assert.Equal(fixture.Document.Id.ToString(), audit.RecordId);

        using var oldData = JsonDocument.Parse(Assert.IsType<string>(audit.OldData));
        using var newData = JsonDocument.Parse(Assert.IsType<string>(audit.NewData));
        Assert.Equal(DocumentStatusIdConst.DRAFT, oldData.RootElement.GetProperty("statusId").GetInt16());
        Assert.Equal(DocumentStatusIdConst.IN_TRANSIT, newData.RootElement.GetProperty("statusId").GetInt16());
        Assert.Null(oldData.RootElement.GetProperty("inTransitAt").GetDateTimeOrNull());
        Assert.NotNull(newData.RootElement.GetProperty("inTransitAt").GetDateTimeOrNull());
        Assert.True(fixture.UnitOfWork.CommitCalled);
        Assert.False(fixture.UnitOfWork.RollbackCalled);
    }

    [Fact]
    public async Task SendToBankAsync_WhenAuditPersistenceFails_RollsBackAllChanges()
    {
        var fixture = new CashCollectionFixture(new AuditPersistenceException());

        await Assert.ThrowsAsync<AuditPersistenceException>(
            () => fixture.Service.SendToBankAsync(fixture.Document.Id));

        Assert.Equal(1, fixture.AuditLogs.CreateAttempts);
        Assert.True(fixture.UnitOfWork.RollbackCalled);
        Assert.False(fixture.UnitOfWork.CommitCalled);
        Assert.Equal(DocumentStatusIdConst.DRAFT, fixture.Document.StatusId);
        Assert.Null(fixture.Document.InTransitAt);
        Assert.Null(fixture.Document.InTransitByUserId);
        Assert.Empty(fixture.PostingBatches.Items);
    }

    private sealed class CashCollectionFixture
    {
        public CashCollectionFixture(Exception? auditCreateException = null)
        {
            var activeState = new State
            {
                Id = StateIdConst.ACTIVE,
                ShortName = "Active",
                FullName = "Active"
            };
            var organization = new Organization
            {
                Id = 1,
                ShortName = "Test organization",
                FullName = "Test organization",
                Inn = "123456789",
                SetupStatus = "COMPLETED"
            };
            var currency = new Currency
            {
                Id = 1,
                Code = "UZS",
                Name = "Uzbek sum",
                StateId = StateIdConst.ACTIVE,
                State = activeState
            };
            var cashBox = new CashBox
            {
                Id = 10,
                OrganizationId = organization.Id,
                Name = "Main cash box",
                Code = "MAIN",
                CurrencyId = currency.Id,
                StateId = StateIdConst.ACTIVE,
                State = activeState,
                Organization = organization,
                Currency = currency,
                IsMain = true
            };
            var bank = new Bank
            {
                Id = 20,
                Code = "TEST_BANK",
                Name = "Test bank",
                StateId = StateIdConst.ACTIVE,
                State = activeState
            };
            var bankAccount = new BankAccount
            {
                Id = 30,
                OrganizationId = organization.Id,
                BankId = bank.Id,
                AccountNumber = "20208000100000000001",
                CurrencyId = currency.Id,
                StateId = StateIdConst.ACTIVE,
                Organization = organization,
                Bank = bank,
                Currency = currency,
                State = activeState
            };

            Document = new CashCollectionDoc
            {
                Id = 1,
                OrganizationId = organization.Id,
                Organization = organization,
                DocNumber = "1",
                DocDate = new DateTime(2026, 8, 29, 10, 0, 0),
                CashBoxId = cashBox.Id,
                CashBox = cashBox,
                BankAccountId = bankAccount.Id,
                BankAccount = bankAccount,
                CurrencyId = currency.Id,
                Currency = currency,
                Amount = 1_000_000m,
                ExchangeRate = 1m,
                CashChartAccountId = 101,
                CashChartAccount = CreateAccount(101, "5010", organization, activeState),
                CashInTransitAccountId = 102,
                CashInTransitAccount = CreateAccount(102, "5710", organization, activeState),
                BankChartAccountId = 103,
                BankChartAccount = CreateAccount(103, "5110", organization, activeState),
                StatusId = DocumentStatusIdConst.DRAFT,
                Status = CreateStatus(DocumentStatusIdConst.DRAFT),
                StateId = StateIdConst.ACTIVE,
                State = activeState,
                CreatedDate = new DateTime(2026, 8, 29, 9, 0, 0)
            };

            var queryBuilder = new QueryBuilder(new ProjectionResolver());
            var documentQuery = new InMemoryQueryRepository<CashCollectionDoc>(Document);
            var documentCommand = new RecordingCommandRepository<CashCollectionDoc>();
            PostingBatches = new RecordingCommandRepository<PostingBatch>();
            AuditLogs = new RecordingCommandRepository<AuditLog>(auditCreateException);
            UnitOfWork = new RecordingUnitOfWork(() =>
            {
                Document.StatusId = DocumentStatusIdConst.DRAFT;
                Document.Status = CreateStatus(DocumentStatusIdConst.DRAFT);
                Document.InTransitAt = null;
                Document.InTransitByUserId = null;
                PostingBatches.Items.Clear();
            });

            var userContext = new TestUserContext(organization.Id, userId: 7);
            var auditService = new AuditLogService(userContext, AuditLogs, new UnusedAuditLogQueryCore());
            Service = new CashCollectionLifecycleService(
                userContext,
                queryBuilder,
                new NoOpPostingLock(),
                new OpenAccountingPeriodValidator(),
                new SuccessfulAccountingDispatcher(),
                new SufficientCashCollectionMoneyService(),
                auditService,
                documentQuery,
                documentCommand,
                new InMemoryQueryRepository<PostingBatch>(),
                PostingBatches,
                new InMemoryQueryRepository<AccountingRegisterEntry>(),
                new RecordingCommandRepository<AccountingRegisterEntry>(),
                new InMemoryQueryRepository<MoneyRegisterBalance>(),
                new UnusedRelatedDocumentService(),
                NullLogger<CashCollectionLifecycleService>.Instance,
                UnitOfWork);
        }

        public CashCollectionLifecycleService Service { get; }
        public CashCollectionDoc Document { get; }
        public RecordingCommandRepository<PostingBatch> PostingBatches { get; }
        public RecordingCommandRepository<AuditLog> AuditLogs { get; }
        public RecordingUnitOfWork UnitOfWork { get; }

        public static ChartAccount CreateAccount(int id, string number, Organization organization, State state) => new()
        {
            Id = id,
            Number = number,
            Name = number,
            OrganizationId = organization.Id,
            Organization = organization,
            StateId = StateIdConst.ACTIVE,
            State = state
        };

        public static DocumentStatus CreateStatus(short id) => new()
        {
            Id = id,
            Code = id switch
            {
                DocumentStatusIdConst.DRAFT => "DRAFT",
                DocumentStatusIdConst.IN_TRANSIT => "IN_TRANSIT",
                DocumentStatusIdConst.POSTED => "POSTED",
                _ => id.ToString()
            },
            Name = id switch
            {
                DocumentStatusIdConst.DRAFT => "Draft",
                DocumentStatusIdConst.IN_TRANSIT => "In transit",
                DocumentStatusIdConst.POSTED => "Posted",
                _ => id.ToString()
            },
            StateId = StateIdConst.ACTIVE
        };
    }

    private sealed class CashFiscalTransferFixture
    {
        public CashFiscalTransferFixture()
        {
            var state = new State
            {
                Id = StateIdConst.ACTIVE,
                ShortName = "Active",
                FullName = "Active"
            };
            var organization = new Organization
            {
                Id = 1,
                ShortName = "Test organization",
                FullName = "Test organization",
                Inn = "123456789",
                SetupStatus = "COMPLETED"
            };
            var currency = new Currency
            {
                Id = 1,
                Code = "UZS",
                Name = "Uzbek sum",
                StateId = state.Id,
                State = state
            };
            var cashBox = new CashBox
            {
                Id = 10,
                OrganizationId = organization.Id,
                Name = "Main cash box",
                Code = "MAIN",
                CurrencyId = currency.Id,
                StateId = state.Id,
                State = state,
                Organization = organization,
                Currency = currency,
                IsMain = true
            };
            var fiscalRegister = new FiscalCashRegister
            {
                Id = 11,
                OrganizationId = organization.Id,
                Name = "Fiscal register",
                StateId = state.Id,
                State = state,
                Organization = organization
            };
            var direction = new MovementDirection
            {
                Id = MovementDirectionIdConst.OUT,
                Code = "OUT",
                Name = "Out"
            };

            Document = new CashFiscalTransferDoc
            {
                Id = 2,
                OrganizationId = organization.Id,
                Organization = organization,
                DocNumber = "1",
                DocDate = new DateTime(2026, 8, 29, 11, 0, 0),
                FiscalCashRegisterId = fiscalRegister.Id,
                FiscalCashRegister = fiscalRegister,
                CashBoxId = cashBox.Id,
                CashBox = cashBox,
                DirectionId = direction.Id,
                Direction = direction,
                CurrencyId = currency.Id,
                Currency = currency,
                Amount = 500_000m,
                ExchangeRate = 1m,
                FiscalCashAccountId = 201,
                FiscalCashAccount = CashCollectionFixture.CreateAccount(201, "5011", organization, state),
                CashBoxAccountId = 202,
                CashBoxAccount = CashCollectionFixture.CreateAccount(202, "5010", organization, state),
                StatusId = DocumentStatusIdConst.DRAFT,
                Status = CashCollectionFixture.CreateStatus(DocumentStatusIdConst.DRAFT),
                StateId = state.Id,
                State = state,
                CreatedDate = new DateTime(2026, 8, 29, 10, 0, 0)
            };

            var userContext = new TestUserContext(organization.Id, userId: 7);
            var queryBuilder = new QueryBuilder(new ProjectionResolver());
            AuditLogs = new RecordingCommandRepository<AuditLog>();
            var auditService = new AuditLogService(userContext, AuditLogs, new UnusedAuditLogQueryCore());
            Service = new CashFiscalTransferLifecycleService(
                userContext,
                queryBuilder,
                new NoOpPostingLock(),
                new OpenAccountingPeriodValidator(),
                new SuccessfulAccountingDispatcher(),
                new SufficientCashFiscalTransferMoneyService(),
                auditService,
                new InMemoryQueryRepository<CashFiscalTransferDoc>(Document),
                new RecordingCommandRepository<CashFiscalTransferDoc>(),
                new InMemoryQueryRepository<PostingBatch>(),
                new RecordingCommandRepository<PostingBatch>(),
                new InMemoryQueryRepository<AccountingRegisterEntry>(),
                new RecordingCommandRepository<AccountingRegisterEntry>(),
                new InMemoryQueryRepository<MoneyRegisterBalance>(),
                NullLogger<CashFiscalTransferLifecycleService>.Instance,
                new RecordingUnitOfWork(() => { }));
        }

        public CashFiscalTransferLifecycleService Service { get; }
        public CashFiscalTransferDoc Document { get; }
        public RecordingCommandRepository<AuditLog> AuditLogs { get; }
    }

    private sealed class PaymentAcceptancePointOperationFixture
    {
        public PaymentAcceptancePointOperationFixture()
        {
            var state = new State
            {
                Id = StateIdConst.ACTIVE,
                ShortName = "Active",
                FullName = "Active"
            };
            var organization = new Organization
            {
                Id = 1,
                ShortName = "Test organization",
                FullName = "Test organization",
                Inn = "123456789",
                SetupStatus = "COMPLETED"
            };
            Currency = new Currency
            {
                Id = 1,
                Code = "UZS",
                Name = "Uzbek sum",
                StateId = state.Id,
                State = state
            };
            Point = new PaymentAcceptancePoint
            {
                Id = 10,
                OrganizationId = organization.Id,
                Organization = organization,
                Code = "PAP-1",
                Name = "Payment point",
                StateId = state.Id,
                State = state
            };
            DocumentDate = new DateTime(2026, 8, 29, 12, 0, 0);

            var operations = new List<PaymentAcceptancePointOperation>();
            var direction = new MovementDirection
            {
                Id = MovementDirectionIdConst.IN,
                Code = "IN",
                Name = "In"
            };
            var operationCommand = new RecordingCommandRepository<PaymentAcceptancePointOperation>(
                store: operations,
                afterCreate: operation =>
                {
                    operation.Organization = organization;
                    operation.PaymentAcceptancePoint = Point;
                    operation.Direction = direction;
                    operation.Currency = Currency;
                    operation.Status = CashCollectionFixture.CreateStatus(operation.StatusId);
                    operation.State = state;
                });
            var userContext = new TestUserContext(organization.Id, userId: 7);
            AuditLogs = new RecordingCommandRepository<AuditLog>();
            var auditService = new AuditLogService(userContext, AuditLogs, new UnusedAuditLogQueryCore());
            Service = new PaymentAcceptancePointOperationService(
                userContext,
                new QueryBuilder(new ProjectionResolver()),
                auditService,
                new StubDocumentNumberService(),
                new UnusedPaymentLifecycleService(),
                new UnusedPaymentMoneyRegisterService(),
                new InMemoryQueryRepository<PaymentAcceptancePointOperation>(operations),
                operationCommand,
                new InMemoryQueryRepository<PaymentAcceptancePoint>(Point),
                new InMemoryQueryRepository<Currency>(Currency),
                NullLogger<PaymentAcceptancePointOperationService>.Instance,
                new RecordingUnitOfWork(() => { }));
        }

        public PaymentAcceptancePointOperationService Service { get; }
        public PaymentAcceptancePoint Point { get; }
        public Currency Currency { get; }
        public DateTime DocumentDate { get; }
        public RecordingCommandRepository<AuditLog> AuditLogs { get; }
    }

    private sealed class ProjectionResolver : IQueryBuilderResolver
    {
        public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;

        public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>()
        {
            if (typeof(TEntity) == typeof(CashCollectionDoc) && typeof(TResult) == typeof(CashCollectionDto))
                return (IProjectionBuilder<TEntity, TResult>)(object)new CashCollectionDtoProjection();
            if (typeof(TEntity) == typeof(CashFiscalTransferDoc) && typeof(TResult) == typeof(CashFiscalTransferDto))
                return (IProjectionBuilder<TEntity, TResult>)(object)new CashFiscalTransferDtoProjection();
            if (typeof(TEntity) == typeof(PaymentAcceptancePointOperation) && typeof(TResult) == typeof(PaymentAcceptancePointOperationDto))
                return (IProjectionBuilder<TEntity, TResult>)(object)new PaymentAcceptancePointOperationDtoProjection();

            throw new InvalidOperationException($"Projection {typeof(TEntity).Name} -> {typeof(TResult).Name} is not configured for the test.");
        }

        public IOrderByBuilder<TEntity, TResult>? GetOrderByBuilder<TEntity, TResult>() => null;
    }

    private sealed class InMemoryQueryRepository<TEntity> : IQueryRepository<TEntity>
        where TEntity : class
    {
        private readonly List<TEntity> _items;

        public InMemoryQueryRepository() : this(new List<TEntity>())
        {
        }

        public InMemoryQueryRepository(params TEntity[] items) : this(new List<TEntity>(items))
        {
        }

        public InMemoryQueryRepository(List<TEntity> items)
        {
            _items = items;
        }

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
            Task.FromResult(_items.Any(predicate.Compile()));

        public Task<TEntity?> GetAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            Task.FromResult(_items.FirstOrDefault(specification.Criteria.Compile()));

        public Task<TResult?> GetAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default)
        {
            var entity = _items.FirstOrDefault(specification.Criteria.Compile());
            return Task.FromResult(entity is null ? default : specification.Selector.Compile()(entity));
        }

        public Task<List<TEntity>> GetAllAsync(QuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            Task.FromResult(_items.Where(specification.Criteria.Compile()).ToList());

        public Task<List<TResult>> GetAllAsync<TResult>(QuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            Task.FromResult(_items.Where(specification.Criteria.Compile()).Select(specification.Selector.Compile()).ToList());

        public Task<PagedList<TEntity>> GetPagedAsync(PagedQuerySpecification<TEntity> specification, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<PagedList<TResult>> GetPagedAsync<TResult>(PagedQuerySpecification<TEntity, TResult> specification, CancellationToken ct = default) =>
            throw new NotSupportedException();
    }

    private sealed class RecordingCommandRepository<TEntity>(
        Exception? createException = null,
        ICollection<TEntity>? store = null,
        Action<TEntity>? afterCreate = null) : ICommandRepository<TEntity>
        where TEntity : class
    {
        private long _nextId = 1;
        public List<TEntity> Items { get; } = [];
        public int CreateAttempts { get; private set; }

        public Task CreateAsync(TEntity entity, CancellationToken ct = default)
        {
            CreateAttempts++;
            if (createException is not null)
                throw createException;

            AssignId(entity);
            afterCreate?.Invoke(entity);
            Items.Add(entity);
            store?.Add(entity);
            return Task.CompletedTask;
        }

        public async Task CreateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
        {
            foreach (var entity in entities)
                await CreateAsync(entity, ct);
        }

        public Task UpdateAsync(TEntity entity, CancellationToken ct = default)
        {
            if (entity is CashCollectionDoc document)
                document.Status = CashCollectionFixture.CreateStatus(document.StatusId);
            if (entity is CashFiscalTransferDoc transfer)
                transfer.Status = CashCollectionFixture.CreateStatus(transfer.StatusId);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) => Task.CompletedTask;
        public Task ReloadAsync(TEntity entity, CancellationToken ct = default) => Task.CompletedTask;

        private void AssignId(TEntity entity)
        {
            var property = typeof(TEntity).GetProperty("Id");
            if (property is null || !property.CanWrite)
                return;

            var current = Convert.ToInt64(property.GetValue(entity) ?? 0);
            if (current != 0)
                return;

            property.SetValue(entity, Convert.ChangeType(_nextId++, property.PropertyType));
        }
    }

    private sealed class RecordingUnitOfWork(Action rollback) : IUnitOfWork
    {
        public bool CommitCalled { get; private set; }
        public bool RollbackCalled { get; private set; }

        public Task BeginAsync(CancellationToken ct = default) => Task.CompletedTask;

        public Task CommitAsync(CancellationToken ct = default)
        {
            CommitCalled = true;
            return Task.CompletedTask;
        }

        public Task RollbackAsync(CancellationToken ct = default)
        {
            RollbackCalled = true;
            rollback();
            return Task.CompletedTask;
        }
    }

    private sealed class TestUserContext(int organizationId, int userId) : IUserContext
    {
        public int? Id => userId;
        public int? RoleId => null;
        public CurrentUserKind UserKind => CurrentUserKind.TenantUser;
        public short? LanguageId => LanguageIdConst.RU;
        public int? TenantId => 1;
        public int? OrganizationId => organizationId;
        public List<int> AllowedOrganizationIds => [organizationId];
        public int? BranchId => null;
    }

    private sealed class NoOpPostingLock : IDocumentPostingLock
    {
        public Task AcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) => Task.CompletedTask;
        public Task AcquireInventoryAsync(int organizationId, int warehouseId, IReadOnlyCollection<int> productIds, IReadOnlyCollection<int> productTableIds, CancellationToken ct = default) => Task.CompletedTask;
        public Task AcquireMoneyAsync(int organizationId, string sourceType, int sourceId, CancellationToken ct = default) => Task.CompletedTask;
        public Task<bool> TryAcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default) => Task.FromResult(true);
    }

    private sealed class OpenAccountingPeriodValidator : IAccountingPeriodValidator
    {
        public Task<Result> EnsureOpenAsync(int organizationId, DateTime date, CancellationToken ct = default) =>
            Task.FromResult(Result.Success());
    }

    private sealed class SuccessfulAccountingDispatcher : IAccountingDispatcher
    {
        public Task<Result<List<AccountingRegisterEntry>>> ProcessAsync(object document, CancellationToken ct = default, long? postingBatchId = null) =>
            Task.FromResult(Result.Success(new List<AccountingRegisterEntry>()));
    }

    private sealed class SufficientCashCollectionMoneyService : ICashCollectionMoneyService
    {
        public Task<Result<List<MoneyRegisterBalance>>> PostAsync(CashCollectionDoc document, long postingBatchId, CancellationToken ct) =>
            Task.FromResult(Result.Success(new List<MoneyRegisterBalance>()));

        public Task<Result<List<MoneyRegisterBalance>>> ReverseAsync(CashCollectionDoc document, long postingBatchId, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<decimal> GetCashBoxBalanceAsync(int cashBoxId, DateTime asOfDate, CancellationToken ct) =>
            Task.FromResult(10_000_000m);
    }

    private sealed class SufficientCashFiscalTransferMoneyService : ICashFiscalTransferMoneyService
    {
        public Task<Result<List<MoneyRegisterBalance>>> PostAsync(CashFiscalTransferDoc document, long postingBatchId, CancellationToken ct) =>
            Task.FromResult(Result.Success(new List<MoneyRegisterBalance>()));

        public Task<Result<List<MoneyRegisterBalance>>> ReverseAsync(CashFiscalTransferDoc document, long postingBatchId, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<decimal> GetFiscalBalanceAsync(int fiscalCashRegisterId, short currencyId, DateTime asOfDate, CancellationToken ct) =>
            Task.FromResult(10_000_000m);

        public Task<decimal> GetCashBoxBalanceAsync(int cashBoxId, DateTime asOfDate, CancellationToken ct) =>
            Task.FromResult(10_000_000m);
    }

    private sealed class StubDocumentNumberService : IDocumentNumberService
    {
        public Task<Result<DocumentNumberResult>> GetNextAsync(int organizationId, short documentTypeId, DateTime documentDate, CancellationToken ct = default) =>
            Task.FromResult(Result.Success(new DocumentNumberResult(1, "1", documentDate)));

        public Task<Result<DocumentNumberResult>> GetNextHistoricalAsync(int organizationId, short documentTypeId, DateTime documentDate, CancellationToken ct = default) =>
            throw new NotSupportedException();
    }

    private sealed class UnusedPaymentLifecycleService : IPaymentAcceptancePointOperationLifecycleService
    {
        public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<Result> CancelAsync(long id, CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class UnusedPaymentMoneyRegisterService : IPaymentAcceptancePointMoneyRegisterService
    {
        public Task<Result<List<MoneyRegisterBalance>>> PostAsync(PaymentAcceptancePointOperation operation, long postingBatchId, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<Result<List<MoneyRegisterBalance>>> ReverseAsync(PaymentAcceptancePointOperation operation, long postingBatchId, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<decimal> GetBalanceAsync(int paymentAcceptancePointId, short currencyId, DateTime asOfDate, CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class UnusedRelatedDocumentService : IBankOperationRelatedDocumentService
    {
        public Task<Result<BankOperationRelatedDocumentLink?>> ResolveDraftAsync(long? relatedDocumentId, int organizationId, int bankAccountId, short directionId, short currencyId, decimal amount, short? classificationCategoryId, long? currentBankOperationId, CancellationToken ct) => throw new NotSupportedException();
        public Task<Result<CashCollectionDoc?>> ValidateCashCollectionForConfirmAsync(BankOperation operation, CancellationToken ct) => throw new NotSupportedException();
        public Task<Result<CashCollectionDoc?>> GetLinkedCashCollectionAsync(BankOperation operation, CancellationToken ct) => throw new NotSupportedException();
        public Task<bool> HasActiveCashCollectionLinkAsync(long cashCollectionDocId, long? excludedBankOperationId, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyDictionary<long, long>> GetActiveCashCollectionBankOperationIdsAsync(IReadOnlyCollection<long> cashCollectionDocIds, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyDictionary<long, long>> GetCashCollectionRegistryIdsAsync(IReadOnlyCollection<long> cashCollectionDocIds, CancellationToken ct) => throw new NotSupportedException();
    }

    private sealed class UnusedAuditLogQueryCore : IAuditLogQueryCore
    {
        public Task<Result<List<AuditLogQueryItem>>> QueryAsync(AuditLogQueryFilter filter, AuditLogQueryOptions options, CancellationToken ct = default) =>
            Task.FromResult(Result.Success(new List<AuditLogQueryItem>()));

        public Task<Result<PagedList<AuditLogQueryItem>>> QueryPagedAsync(AuditLogQueryFilter filter, AuditLogQueryOptions options, AuditLogQueryPagination pagination, CancellationToken ct = default) =>
            Task.FromResult(Result.Success(new PagedList<AuditLogQueryItem>([], 0)));
    }

    private sealed class AuditPersistenceException : Exception;
}

internal static class JsonElementTestExtensions
{
    public static DateTime? GetDateTimeOrNull(this JsonElement element) =>
        element.ValueKind == JsonValueKind.Null ? null : element.GetDateTime();
}
