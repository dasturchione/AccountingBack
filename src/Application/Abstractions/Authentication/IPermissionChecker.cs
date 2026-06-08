namespace Application.Abstractions.Authentication;

public interface IPermissionChecker
{
    Task<bool> HasPermissionAsync(int roleId, string permissionCode, CancellationToken ct = default);
    Task<bool> HasAnyPermissionAsync(int roleId, IEnumerable<string> permissionCodes, CancellationToken ct = default);
}
