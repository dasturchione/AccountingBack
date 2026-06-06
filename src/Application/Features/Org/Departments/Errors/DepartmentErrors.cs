using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Departments;

public static class DepartmentErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("Department.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan bo'lim topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган бўлим топилмади.",
            LanguageIdConst.RU      => $"Отдел с id {id} не найден.",
            _                       => $"Department with id {id} was not found."
        });

    public static Error CodeConflict(string code, short? languageId = null) =>
        Error.Conflict("Department.CodeConflict", languageId switch
        {
            LanguageIdConst.UZ      => $"Kodi '{code}' bo'lgan bo'lim allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Коди '{code}' бўлган бўлим аллақачон мавжуд.",
            LanguageIdConst.RU      => $"Отдел с кодом '{code}' уже существует.",
            _                       => $"Department with code '{code}' already exists."
        });
}
