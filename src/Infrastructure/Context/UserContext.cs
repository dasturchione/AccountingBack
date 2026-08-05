using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;
using SharedKernel.Constants;

namespace Infrastructure.Context;

public class UserContext : IUserContext
{
    private const string AllowedOrgIdsKey = "AllowedOrgIds";
    private const string CurrentOrgIdKey = "CurrentOrgId";
    private const string CurrentRoleIdKey = "CurrentRoleId";
    private const string TrustedGlobalAccessKey = "TrustedGlobalAccess";

    private readonly IHttpContextAccessor _accessor;

    public UserContext(IHttpContextAccessor accessor) => _accessor = accessor;

    public int? Id => GetClaimInt(System.Security.Claims.ClaimTypes.NameIdentifier);

    public int? TenantId => GetClaimInt("TenantId");

    public int? RoleId => _accessor.HttpContext?.Items[CurrentRoleIdKey] is int roleId && roleId > 0
        ? roleId
        : null;

    public short? UserKindId => GetClaimShort("UserKindId");

    public int? OrganizationId => _accessor.HttpContext?.Items[CurrentOrgIdKey] is int id && id > 0 ? id : null;

    public List<int> AllowedOrganizationIds =>
        _accessor.HttpContext?.Items[AllowedOrgIdsKey] is List<int> ids ? ids : [];

    public bool HasGlobalAccess => _accessor.HttpContext?.Items[TrustedGlobalAccessKey] is true;

    public int? BranchId => GetHeaderInt("X-BranchId");

    public short? LanguageId => GetLanguageId();

    private int? GetClaimInt(string claimType)
    {
        var value = _accessor.HttpContext?.User.FindFirst(claimType)?.Value;
        return int.TryParse(value, out var result) && result > 0 ? result : null;
    }

    private short? GetClaimShort(string claimType)
    {
        var value = _accessor.HttpContext?.User.FindFirst(claimType)?.Value;
        return short.TryParse(value, out var result) && result > 0 ? result : null;
    }

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