using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.PurchaseServices;

public static class PurchaseServiceErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("PurchaseService.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan xarid xizmati topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-si {id} bo'lgan xarid xizmati topilmadi.",
            LanguageIdConst.RU      => $"Услуга закупки с id {id} не найдена.",
            _                       => $"Purchase service with id {id} was not found."
        });

    public static Error ServiceTypeNotFound(int id, short? languageId = null) =>
        Error.NotFound("PurchaseService.ServiceTypeNotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan xizmat turi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-si {id} bo'lgan xizmat turi topilmadi.",
            LanguageIdConst.RU      => $"Тип услуги с id {id} не найден.",
            _                       => $"Purchase service type with id {id} was not found."
        });
}
