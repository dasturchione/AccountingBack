using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Imports;

public static class ImportErrors
{
    public static Error NoMatchingColumns(short? languageId = null) => Validation("Import.NoMatchingColumns", languageId,
        "Faylda mos ustunlar topilmadi.", "Файлда мос устунлар топилмади.", "В файле не найдены подходящие столбцы.", "No matching columns were found in the file.");

    public static Error EmptyFile(short? languageId = null) => Validation("Import.EmptyFile", languageId,
        "Fayl bo'sh.", "Файл бўш.", "Файл пуст.", "The file is empty.");

    public static Error ReadFailed(short? languageId = null) => Validation("Import.ReadFailed", languageId,
        "Import faylini o'qib bo'lmadi.", "Импорт файлини ўқиб бўлмади.", "Не удалось прочитать файл импорта.", "The import file could not be read.");

    public static Error SheetNotFound(string sheetName, short? languageId = null) => Validation("Import.SheetNotFound", languageId,
        $"Varaq topilmadi: {sheetName}.", $"Варақ топилмади: {sheetName}.", $"Лист не найден: {sheetName}.", $"Worksheet was not found: {sheetName}.");

    public static Error NoHeader(short? languageId = null) => Validation("Import.NoHeader", languageId,
        "Sarlavha qatori topilmadi.", "Сарлавҳа қатори топилмади.", "Строка заголовков не найдена.", "The header row was not found.");

    private static Error Validation(string code, short? languageId, string uz, string uzCyrl, string ru, string en) =>
        Error.Validation(code, languageId switch
        {
            LanguageIdConst.UZ => uz,
            LanguageIdConst.UZ_CYRL => uzCyrl,
            LanguageIdConst.RU => ru,
            _ => en
        });
}
