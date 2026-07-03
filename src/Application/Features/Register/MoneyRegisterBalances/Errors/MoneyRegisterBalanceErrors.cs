using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.MoneyRegisterBalances;

public static class MoneyRegisterBalanceErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("MoneyRegisterBalance.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan pul registri topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-si {id} bo'lgan pul registri topilmadi.",
            LanguageIdConst.RU      => $"Денежный пулевого регистра по сидделке {id} не найден.",
            _                       => $"Money register balance with id {id} was not found."
        });

    public static Error MissingOriginalEntries(long documentId, short? languageId = null) =>
        Error.Conflict("MoneyRegisterBalance.MissingOriginalEntries", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {documentId} bo'lgan hujjat uchun asliy pul registri yozuvlari topilmadi.",
            _ => $"Original money register entries were not found for document with id {documentId}."
        });
}
