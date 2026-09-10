using Application.Features.Pay.Periods;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace Infrastructure.Services;

public sealed class PayrollPeriodLock : IPayrollPeriodLock
{
    private readonly AppDbContext _context;

    public PayrollPeriodLock(AppDbContext context)
    {
        _context = context;
    }

    public async Task AcquireAsync(long periodId, CancellationToken ct = default)
    {
        var connection = _context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
            await connection.OpenAsync(ct);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT id FROM pay_period WHERE id = @periodId FOR UPDATE";

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@periodId";
            parameter.Value = periodId;
            command.Parameters.Add(parameter);

            var currentTransaction = _context.Database.CurrentTransaction?.GetDbTransaction();
            if (currentTransaction is not null)
                command.Transaction = currentTransaction;

            await command.ExecuteScalarAsync(ct);
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }
}
