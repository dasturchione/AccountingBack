using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Users;

public class UserListDtoProjection : IProjectionBuilder<User, UserListDto>
{
    public Expression<Func<User, UserListDto>> Build() =>
        user => new UserListDto
        {
            Id = user.Id,
            UserName = user.UserName,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            TenantId = user.TenantId,
            UserKindId = user.UserKindId,
            EmailVerified = user.EmailVerified,
            EmailVerifiedAt = user.EmailVerifiedAt,
            LastLoginIp = user.LastLoginIp,
            Timezone = user.Timezone,
            LastAccessTime = user.LastAccessTime,
            StateId = user.StateId,
            CreatedDate = user.CreatedDate,
            StateName = user.State.FullName
        };
}