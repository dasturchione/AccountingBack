using SharedKernel.Results;

namespace Application.Features.Users.Services;

public interface IUserManagementCore
{
    Task<Result<UserManagementCreateResult>> CreateUserAsync(
        UserManagementCreateRequest request,
        UserManagementOptions options,
        CancellationToken ct = default);

    Task<Result> UpdateUserAsync(
        UserManagementUpdateRequest request,
        UserManagementOptions options,
        CancellationToken ct = default);

    Task SendWelcomeEmailSafeAsync(UserWelcomeEmailMessage message, CancellationToken ct = default);
}
