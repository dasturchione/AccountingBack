using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.FaDepreciations;

public static class FaDepreciationErrors
{
    public static Error InvalidPeriod(short? languageId = null) =>
        Error.Business("FaDepreciation.InvalidPeriod", languageId switch
        {
            LanguageIdConst.UZ => "Davr YYYY-MM formatida bo'lishi shart.",
            LanguageIdConst.UZ_CYRL => "Давр YYYY-MM форматида бўлиши шарт.",
            LanguageIdConst.RU => "Период должен быть в формате YYYY-MM.",
            _ => "Period must be in YYYY-MM format."
        });

    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("FaDepreciation.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan amortizatsiya run hujjati topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган амортизация ҳисоблаш ҳужжати топилмади.",
            LanguageIdConst.RU => $"Документ начисления амортизации с id {id} не найден.",
            _ => $"Depreciation run document with id {id} was not found."
        });

    public static Error AlreadyRun(DateTime periodMonth, short? languageId = null) =>
        Error.Conflict("FaDepreciation.AlreadyRun", languageId switch
        {
            LanguageIdConst.UZ => $"{periodMonth:yyyy-MM} davri uchun amortizatsiya allaqachon ishga tushirilgan.",
            LanguageIdConst.UZ_CYRL => $"{periodMonth:yyyy-MM} даври учун амортизация аллақачон ишга туширилган.",
            LanguageIdConst.RU => $"Амортизация за период {periodMonth:yyyy-MM} уже начислена.",
            _ => $"Depreciation has already been run for period {periodMonth:yyyy-MM}."
        });

    public static Error NoDepreciationLines(DateTime periodMonth, short? languageId = null) =>
        Error.Business("FaDepreciation.NoDepreciationLines", languageId switch
        {
            LanguageIdConst.UZ => $"{periodMonth:yyyy-MM} davri uchun amortizatsiya hisoblanadigan asset topilmadi.",
            LanguageIdConst.UZ_CYRL => $"{periodMonth:yyyy-MM} даври учун амортизация ҳисобланадиган асосий восита топилмади.",
            LanguageIdConst.RU => $"За период {periodMonth:yyyy-MM} не найдены основные средства для начисления амортизации.",
            _ => $"No depreciable fixed assets were found for period {periodMonth:yyyy-MM}."
        });

    public static Error CannotCancelInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaDepreciation.CannotCancelInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan amortizatsiya run hujjatini {statusId} holatida bekor qilib bo'lmaydi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган амортизация ҳисоблаш ҳужжатини {statusId} ҳолатида бекор қилиб бўлмайди.",
            LanguageIdConst.RU => $"Документ начисления амортизации с id {id} нельзя отменить в статусе {statusId}.",
            _ => $"Depreciation run document with id {id} cannot be cancelled in status {statusId}."
        });

    public static Error MissingPostingBatch(long id, short? languageId = null) =>
        Error.Conflict("FaDepreciation.MissingPostingBatch", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan amortizatsiya run uchun posting batch topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган амортизация ҳисоблаш учун ўтказмалар пакети топилмади.",
            LanguageIdConst.RU => $"Для начисления амортизации с id {id} не найден пакет проводок.",
            _ => $"Posting batch was not found for depreciation run with id {id}."
        });
}
