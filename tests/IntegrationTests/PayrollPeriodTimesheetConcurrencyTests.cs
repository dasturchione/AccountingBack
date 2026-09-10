using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.AuditLogs;
using Application.Features.Pay.Periods;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Query;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using SharedKernel.Constants;
using SharedKernel.Query;
using Testcontainers.PostgreSql;

namespace IntegrationTests;

public sealed class PayrollPeriodTimesheetConcurrencyTests
{
    [PostgreSqlFact]
    public async Task PeriodRowLock_SerializesTimesheetCreationAndRechecksUsageBeforeUpdate()
    {
        await using var database = new PostgreSqlBuilder("postgres:16-alpine").Build();
        await database.StartAsync();

        var connectionString = database.GetConnectionString();
        await InitializeSchemaAsync(connectionString);

        await using var connectionA = new NpgsqlConnection(connectionString);
        await connectionA.OpenAsync();

        await using (var transactionA = await connectionA.BeginTransactionAsync())
        {
            var firstLock = await LockPeriodAsync(connectionA, transactionA, 1);
            Assert.Equal(1L, firstLock);

            await using var timesheetLockContext = CreateContext(connectionString);
            await using var transactionB = await timesheetLockContext.Database.BeginTransactionAsync();
            var secondLockTask = new PayrollPeriodLock(timesheetLockContext).AcquireAsync(1);
            await Task.Delay(200);
            Assert.False(secondLockTask.IsCompleted);

            await transactionA.CommitAsync();
            await secondLockTask.WaitAsync(TimeSpan.FromSeconds(5));
            await transactionB.RollbackAsync();
        }

        await using (var transactionA = await connectionA.BeginTransactionAsync())
        {
            await LockPeriodAsync(connectionA, transactionA, 1);

            await using var context = CreateContext(connectionString);
            var service = CreatePeriodService(context);
            var updateTask = service.UpdateAsync(1, new PayrollPeriodUpdateDto
            {
                Year = 2026,
                Month = 9,
                DailyWorkHours = 8m,
                WorkDates = [new DateOnly(2026, 9, 1)]
            });

            await Task.Delay(200);
            Assert.False(updateTask.IsCompleted);

            await ExecuteAsync(
                connectionA,
                transactionA,
                """
                insert into pay_timesheet (id, organization_id, period_id, state_id, status_id)
                values (100, 1, 1, 1, 1)
                """);
            await transactionA.CommitAsync();

            var result = await updateTask.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(result.IsSuccess);
            Assert.Equal("Payroll.PeriodUsedInTimesheet", result.Error.Code);
        }

        await using var verificationConnection = new NpgsqlConnection(connectionString);
        await verificationConnection.OpenAsync();
        await using var verificationCommand = verificationConnection.CreateCommand();
        verificationCommand.CommandText = "select work_date from pay_period_work_day where period_id = 1";
        var workDate = (DateOnly?)await verificationCommand.ExecuteScalarAsync();
        Assert.Equal(new DateOnly(2026, 9, 1), workDate);
    }

    private static AppDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        var context = new AppDbContext(options);
        context.SetUserContext(new TestUserContext());
        return context;
    }

    private static PayrollPeriodService CreatePeriodService(AppDbContext context)
    {
        var queryBuilder = new QueryBuilder(new TestQueryBuilderResolver());
        return new PayrollPeriodService(
            new TestUserContext(),
            queryBuilder,
            new NoopAuditLogService(),
            new QueryRepository<PayPeriod>(context),
            new CommandRepository<PayPeriod>(context),
            new CommandRepository<PayPeriodWorkDay>(context),
            new QueryRepository<PayTimesheet>(context),
            new QueryRepository<PayPayrollDoc>(context),
            new QueryRepository<PayPayrollRecalculation>(context),
            new QueryRepository<PayPaymentBatch>(context),
            new PayrollPeriodLock(context),
            NullLogger<PayrollPeriodService>.Instance,
            new UnitOfWork(context));
    }

    private static async Task InitializeSchemaAsync(string connectionString)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await ExecuteAsync(connection, null, """
            create table pay_period
            (
                id bigint primary key,
                organization_id integer not null,
                period_year smallint not null,
                period_month smallint not null,
                start_date date not null,
                end_date date not null,
                norm_work_days numeric(6,2) not null,
                norm_work_hours numeric(8,2) not null,
                daily_work_hours numeric(8,4) not null,
                status varchar(20) not null,
                created_date timestamp without time zone not null,
                closed_date timestamp without time zone null,
                closed_by_user_id integer null
            );

            create table pay_period_work_day
            (
                id bigint primary key,
                organization_id integer not null,
                period_id bigint not null,
                work_date date not null
            );

            create table pay_timesheet
            (
                id bigint primary key,
                organization_id integer not null,
                period_id bigint not null,
                state_id smallint not null,
                status_id smallint not null
            );

            insert into pay_period (
                id, organization_id, period_year, period_month, start_date, end_date,
                norm_work_days, norm_work_hours, daily_work_hours, status, created_date)
            values (1, 1, 2026, 9, date '2026-09-01', date '2026-09-30', 1, 8, 8, 'OPEN', timestamp '2026-09-01 00:00:00');

            insert into pay_period_work_day (id, organization_id, period_id, work_date)
            values (1, 1, 1, date '2026-09-01');
            """);
    }

    private static async Task<long> LockPeriodAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        long periodId)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT id FROM pay_period WHERE id = @periodId FOR UPDATE";
        command.Parameters.AddWithValue("periodId", periodId);
        return (long)(await command.ExecuteScalarAsync())!;
    }

    private static async Task ExecuteAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction? transaction,
        string sql)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private sealed class TestUserContext : IUserContext
    {
        public int? Id => 1;
        public int? RoleId => null;
        public CurrentUserKind UserKind => CurrentUserKind.SuperAdmin;
        public short? LanguageId => LanguageIdConst.EN;
        public int? TenantId => null;
        public int? OrganizationId => 1;
        public List<int> AllowedOrganizationIds { get; } = [1];
        public int? BranchId => null;
    }

    private sealed class TestQueryBuilderResolver : IQueryBuilderResolver
    {
        public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() => null;

        public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() =>
            throw new NotSupportedException();

        public IOrderByBuilder<TEntity, TResult>? GetOrderByBuilder<TEntity, TResult>() => null;
    }

    private sealed class NoopAuditLogService : IAuditLogService
    {
        public void SetOldValues(object oldValues) { }
        public void SetNewValues(object newValues) { }
        public Task CreateAsync(string tableName, string recordId, string operationType, string? comment = null, int? organizationId = null) =>
            Task.CompletedTask;
        public Task<List<AuditLogDto>> GetByRecordAsync(AuditLogFilter filter) =>
            Task.FromResult(new List<AuditLogDto>());
    }
}

public sealed class PostgreSqlFactAttribute : FactAttribute
{
    public PostgreSqlFactAttribute()
    {
        if (!IsDockerAvailable())
            Skip = "Requires a reachable Docker daemon for the PostgreSQL integration test.";
    }

    private static bool IsDockerAvailable()
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DOCKER_HOST")))
            return true;

        if (!OperatingSystem.IsWindows())
            return File.Exists("/var/run/docker.sock");

        try
        {
            using var pipe = new System.IO.Pipes.NamedPipeClientStream(
                ".",
                "docker_engine",
                System.IO.Pipes.PipeDirection.InOut);
            pipe.Connect(100);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
