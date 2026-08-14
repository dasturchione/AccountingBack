using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Users;

public class UserDtoProjection : IProjectionBuilder<User, UserDto>
{
    public Expression<Func<User, UserDto>> Build() =>
        user => new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            TenantId = user.TenantId,
            UserKindId = user.UserKindId,
            UserKindCode = user.UserKind.Code,
            EmailVerified = user.EmailVerified,
            EmailVerifiedAt = user.EmailVerifiedAt,
            LastLoginIp = user.LastLoginIp,
            Timezone = user.Timezone,
            LastAccessTime = user.LastAccessTime,
            StateId = user.StateId,
            CreatedDate = user.CreatedDate,
            StateName = user.State.FullName,
            Organizations = user.UserOrganizations.Select(uo => new UserOrganizationItemDto
            {
                OrganizationId = uo.OrganizationId,
                OrganizationName = uo.Organization.FullName,
                IsDefault = uo.IsDefault,
                BlockedAt = uo.BlockedAt,
                InvitedByUserId = uo.InvitedByUserId,
                IsOwner = uo.IsOwner,
                JoinedAt = uo.JoinedAt,
                LastAccessAt = uo.LastAccessAt,
                RoleId = uo.RoleId,
                RoleName = uo.Role != null ? uo.Role.FullName : null
            }).ToList()
        };
}