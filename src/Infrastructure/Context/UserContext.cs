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

        public int? RoleId => throw new NotImplementedException();

        public short? LanguageId => GetLanguageId();

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
