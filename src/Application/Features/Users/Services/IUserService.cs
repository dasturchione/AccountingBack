using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Users;

public interface IUserService
{
    Task<Result<PagedResponse<UserListDto>>> GetAllAsync(UserListFilter filter, CancellationToken ct = default);
    Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(UserCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, UserUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
