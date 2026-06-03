using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Factory;
using Application.Common.Pagination;
using Application.Options;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Users.Services
{
    public class UserService : IUserService
    {
        private readonly IUserContext _userContext;
        private readonly IQueryRepository<User> _userQuery;
        private readonly ICommandRepository<User> _userCommand;
        private readonly ISpecificationFactory<User> _userSpecification;
        public UserService(IUserContext userContext,
                           IQueryRepository<User> userQuery,
                           ICommandRepository<User> userCommand,
                           ISpecificationFactory<User> userSpecification)
        {
            _userQuery = userQuery;
            _userCommand = userCommand;
            _userContext = userContext;
            _userSpecification = userSpecification;
        }

        public async Task<Result<int>> CreateAsync(UserCreateDto dto, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
        {
            var spec = _userSpecification.Build(new GetByIdOptions<int>(id));
            var entity = await _userQuery.GetAsync(spec, ct);
            if (entity == null)
                return Result.Failure(UserErrors.NotFound(id, _userContext.LanguageId));

            entity.StateId = StateIdConst.PASSIVE;

            await _userCommand.UpdateAsync(entity, ct);
            return Result.Success();
        }

        public async Task<Result<PagedResponse<UserListDto>>> GetAllAsync(UserListFilter filter, CancellationToken ct = default)
        {
            var spec = _userSpecification.BuildPaged<UserListDto, UserListFilter>(filter);
            var pagedList = await _userQuery.GetPagedAsync(spec, ct);
            return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
        }

        public async Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var spec = _userSpecification.Build<UserDto, GetByIdOptions<int>>(new GetByIdOptions<int>(id));
            var entity = await _userQuery.GetAsync(spec, ct);
            if (entity == null)
                return Result.Failure<UserDto>(UserErrors.NotFound(id, _userContext.LanguageId));
            return entity;
        }

        public async Task<Result> UpdateAsync(int id, UserUpdateDto dto, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
