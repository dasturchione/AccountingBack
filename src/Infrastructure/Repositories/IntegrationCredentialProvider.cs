using Application.Abstractions.Integration;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class IntegrationCredentialProvider(AppDbContext context) : IIntegrationCredentialProvider
{
    public Task<IntegrationCredential?> GetAsync(int organizationId, string provider, CancellationToken ct = default) =>
        context.IntegrationCredentials.IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.Provider == provider && x.IsActive)
            .SingleOrDefaultAsync(ct);
}
