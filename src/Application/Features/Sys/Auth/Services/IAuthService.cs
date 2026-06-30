using SharedKernel.Results;

namespace Application.Features.Auth;

public interface IAuthService
{
    ValueTask<Result<LoginResponseDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);

    ValueTask<Result<LoginResponseDto>> SuperAdminLoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
}
