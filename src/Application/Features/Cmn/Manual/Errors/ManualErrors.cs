using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Manual;

public static class ManualErrors
{
    public static Error InvalidContractDate(short? languageId = null) =>
        Error.Validation("Manual.InvalidContractDate", languageId switch
        {
            LanguageIdConst.UZ => "Shartnomani tanlash sanasi noto'g'ri.",
            LanguageIdConst.UZ_CYRL => "Шартномани танлаш санаси нотўғри.",
            LanguageIdConst.RU => "Дата выбора договора указана неверно.",
            _ => "Contract selection date is invalid."
        });
}
