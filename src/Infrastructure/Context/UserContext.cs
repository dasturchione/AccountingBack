using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;
using SharedKernel.Constants;
using System.Security.Claims;

namespace Infrastructure.Context
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _accessor;
        public UserContext(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public int? Id
        {
            get
            {
                var value = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                return int.TryParse(value, out var id) ? id : null;
            }
        }

        public int? RoleId
        {
            get
            {
                var value = _accessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
                return int.TryParse(value, out var id) ? id : null;
            }
        }

        public int? OrganizationId => GetOrganizationId();

        public int? BranchId => GetBranchId();

        public short? LanguageId => GetLanguageId();

        private int? GetOrganizationId() => GetHeaderInt("X-OrganizationId");

        private int? GetBranchId() => GetHeaderInt("X-BranchId");

        private int? GetHeaderInt(string key)
        {
            var value = _accessor.HttpContext?.Request.Headers[key].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(value))
                return null;

            return int.TryParse(value, out var result)
                ? result
                : null;
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
}
