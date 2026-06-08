using Application.Abstractions.Authentication;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Security;

public class PermissionChecker : IPermissionChecker
{
    private readonly AppDbContext _context;

    public PermissionChecker(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasPermissionAsync(int roleId, string permissionCode, CancellationToken ct = default)
    {
        if (roleId <= 0 || string.IsNullOrWhiteSpace(permissionCode))
            return false;

        return await _context.RoleModules
            .AsNoTracking()
            .AnyAsync(rm => rm.RoleId == roleId &&
                            rm.Module.Code == permissionCode.Trim(), ct);
    }

    public async Task<bool> HasAnyPermissionAsync(int roleId, IEnumerable<string> permissionCodes, CancellationToken ct = default)
    {
        if (roleId <= 0)
            return false;

        var codes = permissionCodes
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .Distinct()
            .ToList();

        if (codes.Count == 0)
            return false;

        return await _context.RoleModules
            .AsNoTracking()
            .AnyAsync(rm => rm.RoleId == roleId &&
                            codes.Contains(rm.Module.Code), ct);
    }
}
