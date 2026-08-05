using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;
using SharedKernel.Constants;

namespace Infrastructure.Context;

public class UserContext : IUserContext
{
    private const string AllowedOrgIdsKey = "AllowedOrgIds";
    private const string CurrentOrgIdKey = "CurrentOrgId";
    private const string CurrentRoleIdKey = "CurrentRoleId";

    private readonly IHttpContextAccessor _accessor;

    public UserContext(IHttpContextAccessor accessor) => _accessor = accessor;

    public int? Id => GetClaimInt(System.Security.Claims.ClaimTypes.NameIdentifier);

    public int? TenantId => GetClaimInt("TenantId");

    public int? RoleId => _accessor.HttpContext?.Items[CurrentRoleIdKey] is int roleId && roleId > 0
        ? roleId
        : null;

    public CurrentUserKind UserKind => GetClaimValue("UserKindCode") switch
    {
        UserKindCodeConst.SuperAdmin => CurrentUserKind.SuperAdmin,
        UserKindCodeConst.TenantOwner => CurrentUserKind.TenantAdmin,
        UserKindCodeConst.TenantUser => CurrentUserKind.TenantUser,
        _ => CurrentUserKind.None
    };

    public int? OrganizationId => _accessor.HttpContext?.Items[CurrentOrgIdKey] is int id && id > 0 ? id : null;

    public List<int> AllowedOrganizationIds =>
        _accessor.HttpContext?.Items[AllowedOrgIdsKey] is List<int> ids ? ids : [];

    public int? BranchId => GetHeaderInt("X-BranchId");

    public short? LanguageId => GetLanguageId();

    private int? GetClaimInt(string claimType)
    {
        var value = _accessor.HttpContext?.User.FindFirst(claimType)?.Value;
        return int.TryParse(value, out var result) && result > 0 ? result : null;
    }
    private string? GetClaimValue(string claimType) =>
        _accessor.HttpContext?.User.FindFirst(claimType)?.Value;

    private int? GetHeaderInt(string key)
    {
        var value = _accessor.HttpContext?.Request.Headers[key].FirstOrDefault();
        return int.TryParse(value, out var result) ? result : null;
    }

    private short? GetLanguageId()
    {
        var value = _accessor.HttpContext?.Request.Headers["X-Language"].FirstOrDefault();
        return value switch
        {
            LanguageCodeConst.UZ => LanguageIdConst.UZ,
            LanguageCodeConst.RU => LanguageIdConst.RU,
            LanguageCodeConst.EN => LanguageIdConst.EN,
            LanguageCodeConst.UZ_CYRL => LanguageIdConst.UZ_CYRL,
            _ => LanguageIdConst.EN
        };
    }
}