using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;
using SharedKernel.Constants;
using System.Security.Claims;

namespace Infrastructure.Context
{
    public class UserContext : IUserContext
    {
        private const string AllowedOrgIdsKey = "AllowedOrgIds";

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

        // Faqat X-OrganizationId header berilgan bo'lsa qaytaradi
        // Berilmasa null — ya'ni "barcha ruxsat berilgan tashkilotlar" rejimi
        public int? OrganizationId
        {
            get
            {
                var headerVal = _accessor.HttpContext?.Request.Headers["X-OrganizationId"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(headerVal) && int.TryParse(headerVal, out var headerId))
                    return headerId;

                return null;
            }
        }

        // Middleware tomonidan to'ldiriladi — user ruxsat berilgan barcha org IDlar
        public List<int> AllowedOrganizationIds
        {
            get
            {
                if (_accessor.HttpContext?.Items[AllowedOrgIdsKey] is List<int> ids)
                    return ids;

                // Fallback: tokendan default org
                var claimVal = _accessor.HttpContext?.User.FindFirst("OrganizationId")?.Value;
                if (int.TryParse(claimVal, out var claimId) && claimId > 0)
                    return [claimId];

                return [];
            }
        }

        public int? BranchId => GetHeaderInt("X-BranchId");

        public short? LanguageId => GetLanguageId();

        private int? GetHeaderInt(string key)
        {
            var value = _accessor.HttpContext?.Request.Headers[key].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(value)) return null;
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
}
