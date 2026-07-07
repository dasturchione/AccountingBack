using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.FaDepreciations;

public static class FaDepreciationErrors
{
    public static Error InvalidPeriod(short? languageId = null) =>
        Error.Business("FaDepreciation.InvalidPeriod", languageId switch
        {
            LanguageIdConst.UZ => "Davr YYYY-MM formatida bo'lishi shart.",
            LanguageIdConst.RU => "Period dolzhen byt v formate YYYY-MM.",
            _ => "Period must be in YYYY-MM format."
        });

    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("FaDepreciation.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan amortizatsiya run hujjati topilmadi.",
            LanguageIdConst.RU => $"Dokument run amortizatsii s id {id} ne nayden.",
            _ => $"Depreciation run document with id {id} was not found."
        });

    public static Error AlreadyRun(DateTime periodMonth, short? languageId = null) =>
        Error.Conflict("FaDepreciation.AlreadyRun", languageId switch
        {
            LanguageIdConst.UZ => $"{periodMonth:yyyy-MM} davri uchun amortizatsiya allaqachon ishga tushirilgan.",
            LanguageIdConst.RU => $"Amortizatsiya za period {periodMonth:yyyy-MM} uzhe zapushchena.",
            _ => $"Depreciation has already been run for period {periodMonth:yyyy-MM}."
        });

    public static Error NoDepreciationLines(DateTime periodMonth, short? languageId = null) =>
        Error.Business("FaDepreciation.NoDepreciationLines", languageId switch
        {
            LanguageIdConst.UZ => $"{periodMonth:yyyy-MM} davri uchun amortizatsiya hisoblanadigan asset topilmadi.",
            LanguageIdConst.RU => $"Dlya perioda {periodMonth:yyyy-MM} ne naydeny osnovnyye sredstva dlya amortizatsii.",
            _ => $"No depreciable fixed assets were found for period {periodMonth:yyyy-MM}."
        });

    public static Error CannotCancelInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaDepreciation.CannotCancelInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan amortizatsiya run hujjatini {statusId} holatida bekor qilib bo'lmaydi.",
            LanguageIdConst.RU => $"Dokument run amortizatsii s id {id} nelzya otmenit v statuse {statusId}.",
            _ => $"Depreciation run document with id {id} cannot be cancelled in status {statusId}."
        });

    public static Error MissingPostingBatch(long id, short? languageId = null) =>
        Error.Conflict("FaDepreciation.MissingPostingBatch", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan amortizatsiya run uchun posting batch topilmadi.",
            LanguageIdConst.RU => $"Dlya run amortizatsii s id {id} ne nayden posting batch.",
            _ => $"Posting batch was not found for depreciation run with id {id}."
        });
}
