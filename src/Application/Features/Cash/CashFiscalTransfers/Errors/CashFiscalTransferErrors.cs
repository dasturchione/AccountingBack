using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.CashFiscalTransfers;

public static class CashFiscalTransferErrors
{
    public static Error NotFound(long id, short? languageId = null) => NotFoundError("CashFiscalTransfer.NotFound", languageId,
        $"Id-si {id} bo'lgan fiskal kassa o'tkazmasi topilmadi.", $"Id-си {id} бўлган фискал касса ўтказмаси топилмади.",
        $"Перемещение денег между кассами с id {id} не найдено.", $"Cash fiscal transfer {id} was not found.");

    public static Error InvalidStatus(long id, short statusId, short? languageId = null) => Conflict("CashFiscalTransfer.InvalidStatus", languageId,
        $"Id-si {id} bo'lgan o'tkazmani {statusId} holatida bajarib bo'lmaydi.", $"Id-си {id} бўлган ўтказмани {statusId} ҳолатида бажариб бўлмайди.",
        $"Перемещение денег между кассами {id} нельзя обработать в статусе {statusId}.", $"Cash fiscal transfer {id} cannot be processed in status {statusId}.");

    public static Error AlreadyCancelled(long id, short? languageId = null) => Conflict("CashFiscalTransfer.AlreadyCancelled", languageId,
        $"Id-si {id} bo'lgan o'tkazma allaqachon bekor qilingan.", $"Id-си {id} бўлган ўтказма аллақачон бекор қилинган.",
        $"Перемещение денег между кассами {id} уже отменено.", $"Cash fiscal transfer {id} is already cancelled.");

    public static Error InvalidDirection(short? languageId = null) => InvalidConfiguration(languageId,
        "Yo'nalish -1 yoki 1 bo'lishi kerak.", "Йўналиш -1 ёки 1 бўлиши керак.",
        "Направление должно быть -1 или 1.", "Direction must be -1 or 1.");

    public static Error InvalidAmountOrRate(short? languageId = null) => InvalidConfiguration(languageId,
        "Summa va valyuta kursi noldan katta bo'lishi kerak.", "Сумма ва валюта курси нолдан катта бўлиши керак.",
        "Сумма и курс валюты должны быть больше нуля.", "Amount and exchange rate must be greater than zero.");

    public static Error FiscalRegisterInvalid(short? languageId = null) => InvalidConfiguration(languageId,
        "Fiskal kassa faol emas yoki boshqa tashkilotga tegishli.", "Фискал касса фаол эмас ёки бошқа ташкилотга тегишли.",
        "Фискальная касса неактивна или относится к другой организации.", "Fiscal cash register is inactive or belongs to another organization.");

    public static Error MainCashBoxInvalid(short? languageId = null) => InvalidConfiguration(languageId,
        "Kassa tashkilotning faol asosiy kassasi bo'lishi kerak.", "Касса ташкилотнинг фаол асосий кассаси бўлиши керак.",
        "Касса должна быть активной основной кассой организации.", "Cash box must be the active main cash box of the organization.");

    public static Error CurrencyMismatch(short? languageId = null) => InvalidConfiguration(languageId,
        "Kassa valyutasi hujjat valyutasiga mos kelmaydi.", "Касса валютаси ҳужжат валютасига мос келмайди.",
        "Валюта кассы не совпадает с валютой документа.", "Cash box currency does not match the document currency.");

    public static Error AccountsMustDiffer(short? languageId = null) => InvalidConfiguration(languageId,
        "Ikki xil buxgalteriya hisobvarag'i talab qilinadi.", "Икки хил бухгалтерия ҳисобварағи талаб қилинади.",
        "Требуются два разных бухгалтерских счёта.", "Two different accounting accounts are required.");

    public static Error AccountsInvalid(short? languageId = null) => InvalidConfiguration(languageId,
        "Buxgalteriya hisobvaraqlari faol emas yoki boshqa tashkilotga tegishli.",
        "Бухгалтерия ҳисобварақлари фаол эмас ёки бошқа ташкилотга тегишли.",
        "Бухгалтерские счета неактивны или относятся к другой организации.",
        "Accounting accounts are inactive or belong to another organization.");

    public static Error InsufficientBalance(string source, decimal available, decimal required, short? languageId = null) => Business(
        "CashFiscalTransfer.InsufficientBalance", languageId,
        $"{source} qoldig'i {available}, talab qilinadigan summa esa {required}.",
        $"{source} қолдиғи {available}, талаб қилинадиган сумма эса {required}.",
        $"Остаток источника {source}: {available}, требуемая сумма: {required}.",
        $"{source} balance {available} is less than required amount {required}.");

    public static Error BusinessEffectsAlreadyExist(long id, short? languageId = null) => Conflict("CashFiscalTransfer.BusinessEffectsAlreadyExist", languageId,
        $"Id-si {id} bo'lgan o'tkazmada moliyaviy harakatlar allaqachon mavjud.", $"Id-си {id} бўлган ўтказмада молиявий ҳаракатлар аллақачон мавжуд.",
        $"Перемещение денег между кассами {id} уже имеет финансовые движения.", $"Cash fiscal transfer {id} already has business effects.");

    public static Error MissingPostingBatch(long id, short? languageId = null) => Conflict("CashFiscalTransfer.MissingPostingBatch", languageId,
        $"Id-si {id} bo'lgan o'tkazma uchun o'tkazmalar paketi topilmadi.", $"Id-си {id} бўлган ўтказма учун ўтказмалар пакети топилмади.",
        $"Для перемещения денег между кассами {id} не найден пакет проводок.", $"Posting batch for cash fiscal transfer {id} was not found.");

    public static Error MissingAccountingEntries(long id, short? languageId = null) => Conflict("CashFiscalTransfer.MissingAccountingEntries", languageId,
        $"Id-si {id} bo'lgan o'tkazmaning buxgalteriya yozuvlari topilmadi.", $"Id-си {id} бўлган ўтказманинг бухгалтерия ёзувлари топилмади.",
        $"Бухгалтерские проводки перемещения денег между кассами {id} не найдены.", $"Accounting entries for cash fiscal transfer {id} were not found.");

    public static Error MissingMoneyEntries(long id, short? languageId = null) => Conflict("CashFiscalTransfer.MissingMoneyEntries", languageId,
        $"Id-si {id} bo'lgan o'tkazmaning pul registri yozuvlari topilmadi.", $"Id-си {id} бўлган ўтказманинг пул регистри ёзувлари топилмади.",
        $"Денежные движения перемещения между кассами {id} не найдены.", $"Money entries for cash fiscal transfer {id} were not found.");

    private static Error InvalidConfiguration(short? languageId, string uz, string uzCyrl, string ru, string en) =>
        Business("CashFiscalTransfer.InvalidConfiguration", languageId, uz, uzCyrl, ru, en);
    private static Error Business(string code, short? languageId, string uz, string uzCyrl, string ru, string en) =>
        Error.Business(code, Message(languageId, uz, uzCyrl, ru, en));
    private static Error Conflict(string code, short? languageId, string uz, string uzCyrl, string ru, string en) =>
        Error.Conflict(code, Message(languageId, uz, uzCyrl, ru, en));
    private static Error NotFoundError(string code, short? languageId, string uz, string uzCyrl, string ru, string en) =>
        Error.NotFound(code, Message(languageId, uz, uzCyrl, ru, en));
    private static string Message(short? languageId, string uz, string uzCyrl, string ru, string en) => languageId switch
    {
        LanguageIdConst.UZ => uz, LanguageIdConst.UZ_CYRL => uzCyrl, LanguageIdConst.RU => ru, _ => en
    };
}
