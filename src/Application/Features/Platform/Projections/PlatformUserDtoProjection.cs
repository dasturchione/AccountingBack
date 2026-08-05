using Domain.Entities;
using SharedKernel.Constants;
using System.Linq.Expressions;

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
        TenantId = user.TenantId,
        UserKindId = user.UserKindId,
        EmailVerified = user.EmailVerified,
        EmailVerifiedAt = user.EmailVerifiedAt,
        LastLoginIp = user.LastLoginIp,
        Timezone = user.Timezone,
        LastAccessTime = user.LastAccessTime,
        StateId = user.StateId,
        StateName = user.State.FullName,
        CreatedDate = user.CreatedDate,
        OrganizationsCount = user.UserOrganizations.Count(membership => membership.StateId == StateIdConst.ACTIVE)
    };
}