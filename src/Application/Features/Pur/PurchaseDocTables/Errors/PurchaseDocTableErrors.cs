using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.PurchaseDocTables;

public static class PurchaseDocTableErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("PurchaseDocTable.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan xarid hujjati qatori topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган харид ҳужжати қатори топилмади.",
            LanguageIdConst.RU      => $"Строка документа закупки с id {id} не найдена.",
            _                       => $"Purchase document line with id {id} was not found."
        });
}
