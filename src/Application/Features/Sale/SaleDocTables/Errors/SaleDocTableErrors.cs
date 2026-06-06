using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.SaleDocTables;

public static class SaleDocTableErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("SaleDocTable.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan sotuv hujjati qatori topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган сотув ҳужжати қатори топилмади.",
            LanguageIdConst.RU      => $"Строка документа продажи с id {id} не найдена.",
            _                       => $"Sale document line with id {id} was not found."
        });
}
