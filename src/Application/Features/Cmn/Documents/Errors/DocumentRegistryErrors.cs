using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Cmn.Documents;

public static class DocumentRegistryErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("DocumentRegistry.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Umumiy hujjatlar reyestrida {id}-raqamli yozuv topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Умумий ҳужжатлар реестрида {id}-рақамли ёзув топилмади.",
            LanguageIdConst.RU => $"Запись {id} в общем реестре документов не найдена.",
            _ => $"Document registry entry {id} was not found."
        });
}
