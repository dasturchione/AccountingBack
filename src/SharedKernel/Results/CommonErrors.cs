using SharedKernel.Constants;

namespace SharedKernel.Results
{
    public static class CommonErrors
    {
        public static Error Unauthorized(short? languageId = null) =>
            Error.Unauthorized("Common.Unauthorized", GetUnauthorizedMessage(languageId));

        public static Error Forbidden(short? languageId = null) =>
            Error.Forbidden("Common.Forbidden", GetForbiddenMessage(languageId));

        public static Error Problem(short? languageId = null) =>
            Error.Problem("Common.Problem", GetProblemMessage(languageId));

        public static Error UserHasNoOrganization(short? languageId = null) =>
            Error.Business("Common.UserHasNoOrganization", GetUserHasNoOrganizationMessage(languageId));

        public static Error WarehouseBlockedByInventoryCount(int warehouseId, string operationName, short? languageId = null) =>
            Error.Business("Common.WarehouseBlockedByInventoryCount", GetWarehouseBlockedByInventoryCountMessage(warehouseId, operationName, languageId));

        private static string GetUnauthorizedMessage(short? languageId)
        {
            return languageId switch
            {
                LanguageIdConst.UZ =>
                    "Siz ushbu amalni bajarish uchun avtorizatsiyadan o'tmagansiz.",
                LanguageIdConst.UZ_CYRL =>
                    "Сиз ушбу амални бажариш учун авторизациядан ўтмагансиз.",
                LanguageIdConst.RU =>
                    "Вы не авторизованы для выполнения этого действия.",
                _ =>
                    "You are not authorized to perform this action."
            };
        }

        private static string GetForbiddenMessage(short? languageId)
        {
            return languageId switch
            {
                LanguageIdConst.UZ =>
                    "Siz ushbu amalni bajarish uchun ruxsatga ega emassiz.",
                LanguageIdConst.UZ_CYRL =>
                    "Сиз ушбу амални бажариш учун рухсатга эга эмассиз.",
                LanguageIdConst.RU =>
                    "У вас нет разрешения на выполнение этого действия.",
                _ =>
                    "You don't have permission to perform this action."
            };
        }

        private static string GetProblemMessage(short? languageId)
        {
            return languageId switch
            {
                LanguageIdConst.UZ =>
                    "So'rovni bajarishda muammo yuz berdi. Iltimos, keyinroq qayta urinib ko'ring.",
                LanguageIdConst.UZ_CYRL =>
                    "Сўровни бажаришда муаммо юз берди. Илтимос, кейинроқ қайта уриниб кўринг.",
                LanguageIdConst.RU =>
                    "Произошла проблема при выполнении запроса. Пожалуйста, попробуйте позже.",
                _ =>
                    "An issue occurred while processing the request. Please try again later."
            };
        }

        private static string GetUserHasNoOrganizationMessage(short? languageId)
        {
            return languageId switch
            {
                LanguageIdConst.UZ =>
                    "Joriy foydalanuvchi uchun tashkilot belgilanmagan.",
                LanguageIdConst.UZ_CYRL =>
                    "Жорий фойдаланувчи учун ташкилот белгиланмаган.",
                LanguageIdConst.RU =>
                    "Для текущего пользователя не указана организация.",
                _ =>
                    "No organization is assigned to the current user."
            };
        }

        private static string GetWarehouseBlockedByInventoryCountMessage(int warehouseId, string operationName, short? languageId)
        {
            return languageId switch
            {
                LanguageIdConst.UZ =>
                    $"Warehouse {warehouseId} is blocked by an active inventory count. Operation '{operationName}' is not allowed.",
                LanguageIdConst.UZ_CYRL =>
                    $"Омбор {warehouseId} бўйича фаол инвентар санаш мавжуд. '{operationName}' амалига рухсат йўқ.",
                LanguageIdConst.RU =>
                    $"Склад {warehouseId} заблокирован активной инвентаризацией. Операция '{operationName}' недоступна.",
                _ =>
                    $"Warehouse {warehouseId} is blocked by an active inventory count. Operation '{operationName}' is not allowed."
            };
        }
    }
}
