using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SharedKernel.Exceptions;

namespace Integration.Edo.Persistence;

public sealed class OrganizationEdoProviderStore(
    AppDbContext context,
    IUserContext userContext,
    Application.Abstractions.IUnitOfWork unitOfWork) : IActiveEdoProviderStore
{
    public async Task<EdoProviderCode?> GetAsync(
        int organizationId,
        CancellationToken ct = default)
    {
        EnsureCurrentOrganization(organizationId);

        var entity = await context.OrganizationEdoProviders
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.OrganizationId == organizationId, ct);

        return entity is null ? null : ParseProvider(entity.Provider);
    }

    public async Task SetAsync(
        int organizationId,
        EdoProviderCode providerCode,
        CancellationToken ct = default)
    {
        EnsureCurrentOrganization(organizationId);
        EnsureKnownProvider(providerCode);

        await unitOfWork.BeginAsync(ct);
        try
        {
            var entity = await context.OrganizationEdoProviders
                .SingleOrDefaultAsync(item => item.OrganizationId == organizationId, ct);

            var provider = providerCode.ToString();
            if (entity is null)
            {
                context.OrganizationEdoProviders.Add(new OrganizationEdoProvider
                {
                    OrganizationId = organizationId,
                    Provider = provider,
                    CreatedDate = DateTime.UtcNow
                });
            }
            else if (!string.Equals(entity.Provider, provider, StringComparison.Ordinal))
            {
                entity.Provider = provider;
                entity.UpdatedDate = DateTime.UtcNow;
            }

            await unitOfWork.CommitAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await RollbackAsync(ct);
            throw new OptimisticConcurrencyException(
                "The active EDO provider was modified by another transaction. Please retry.", ex);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            await RollbackAsync(ct);
            throw new UniqueConstraintViolationException(
                "An active EDO provider already exists for this organization.", ex);
        }
        catch
        {
            await RollbackAsync(ct);
            throw;
        }
    }

    private void EnsureCurrentOrganization(int organizationId)
    {
        if (userContext.OrganizationId != organizationId)
            throw new InvalidOperationException(
                "The requested organization is outside the current organization scope.");
    }

    private static void EnsureKnownProvider(EdoProviderCode providerCode)
    {
        if (!Enum.IsDefined(typeof(EdoProviderCode), providerCode))
            throw new ArgumentOutOfRangeException(nameof(providerCode), providerCode, null);
    }

    private static EdoProviderCode ParseProvider(string provider)
    {
        if (Enum.TryParse<EdoProviderCode>(provider, ignoreCase: false, out var providerCode)
            && Enum.IsDefined(typeof(EdoProviderCode), providerCode))
            return providerCode;

        throw new InvalidOperationException(
            $"The stored EDO provider value '{provider}' is invalid.");
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException postgresException
        && postgresException.SqlState == PostgresErrorCodes.UniqueViolation;

    private async Task RollbackAsync(CancellationToken ct)
    {
        try
        {
            await unitOfWork.RollbackAsync(ct);
        }
        catch
        {
            // Original database exception is more useful to the caller.
        }
    }
}
