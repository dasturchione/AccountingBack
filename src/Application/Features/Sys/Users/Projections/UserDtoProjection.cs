using Domain.Entities;
﻿
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Users;

public class UserDtoProjection : IProjectionBuilder<User, UserDto>
{
    public Expression<Func<User, UserDto>> Build()
    {
        return x => new UserDto
        {
            Id = x.Id,
            UserName = x.UserName,
            PhoneNumber = x.PhoneNumber,
            Email = x.Email,
            FirstName = x.FirstName,
            LastName = x.LastName,
            RoleId = x.RoleId,
            EmailVerified = x.EmailVerified,
            EmailVerifiedAt = x.EmailVerifiedAt,
            LastLoginIp = x.LastLoginIp,
            IsPlatformAdmin = x.IsPlatformAdmin,
            Timezone = x.Timezone,
            LastAccessTime = x.LastAccessTime,
            StateId = x.StateId,
            CreatedDate = x.CreatedDate,
            RoleName = x.Role.FullName,
            HasGlobalAccess = x.Role.HasGlobalAccess,
            StateName = x.State.FullName
        };
    }
}
