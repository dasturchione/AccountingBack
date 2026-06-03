using SharedKernel.Constants;

namespace SharedKernel.Results
{
    public static class CommonErrors
    {
        public static Error Unauthorized(short? languageId) =>
            Error.Unauthorized("Common.Unauthorized", GetUnauthorizedMessage(languageId));

        public static Error Forbidden(short? languageId) =>
            Error.Forbidden("Common.Forbidden", GetForbiddenMessage(languageId));

        public static Error Problem(short? languageId) =>
            Error.Problem("Common.Problem", GetProblemMessage(languageId));

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
    }
}
