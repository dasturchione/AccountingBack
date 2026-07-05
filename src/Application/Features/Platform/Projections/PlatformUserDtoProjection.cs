using Domain.Entities;
using System.Linq.Expressions;
using SharedKernel.Constants;

namespace Application.Features.Platform;

public static class PlatformUserDtoProjection
{
    public static readonly Expression<Func<User, PlatformUserDto>> Summary = user => new PlatformUserDto
    {
        Id = user.Id,
        UserName = user.UserName,
        PhoneNumber = user.PhoneNumber,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        RoleId = user.RoleId,
        RoleName = user.Role.FullName,
        HasGlobalAccess = user.Role.HasGlobalAccess,
        EmailVerified = user.EmailVerified,
        EmailVerifiedAt = user.EmailVerifiedAt,
        LastLoginIp = user.LastLoginIp,
        IsPlatformAdmin = user.IsPlatformAdmin,
        Timezone = user.Timezone,
        LastAccessTime = user.LastAccessTime,
        StateId = user.StateId,
        StateName = user.State.FullName,
        CreatedDate = user.CreatedDate,
        OrganizationsCount = user.UserOrganizations.Count(uo => uo.StateId == StateIdConst.ACTIVE)
    };
}
