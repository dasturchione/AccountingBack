using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Rnt.RentalAccruals;

public static class RentalAccrualErrors
{
    public static Error NotFound(long id, short? languageId) => NotFound("RentalAccrual.NotFound", languageId,
        $"Id-si {id} bo'lgan ijara hisoblash hujjati topilmadi.", $"Id-си {id} бўлган ижара ҳисоблаш ҳужжати топилмади.",
        $"Документ начисления аренды {id} не найден.", $"Rental accrual document {id} was not found.");
    public static Error InvalidStatus(long id, short status, short? languageId) => Conflict("RentalAccrual.InvalidStatus", languageId,
        $"{id}-hujjatni {status} holatida bajarib bo'lmaydi.", $"{id}-ҳужжатни {status} ҳолатида бажариб бўлмайди.",
        $"Документ {id} нельзя обработать в статусе {status}.", $"Document {id} cannot be processed in status {status}.");
    public static Error MissingAccounts(long id, short? languageId) => Business("RentalAccrual.MissingAccounts", languageId,
        $"{id}-hujjatda barcha hisobvaraqlar ko'rsatilmagan.", $"{id}-ҳужжатда барча ҳисобварақлар кўрсатилмаган.",
        $"В документе {id} указаны не все бухгалтерские счета.", $"Document {id} has incomplete accounting accounts.");
    public static Error InvalidAccounts(short? languageId) => Business("RentalAccrual.InvalidAccounts", languageId,
        "Hisobvaraqlar faol emas yoki boshqa tashkilotga tegishli.", "Ҳисобварақлар фаол эмас ёки бошқа ташкилотга тегишли.",
        "Счета неактивны или относятся к другой организации.", "Accounts are inactive or belong to another organization.");
    public static Error ItemMismatch(short? languageId) => Business("RentalAccrual.ItemMismatch", languageId,
        "Yuborilgan satrlar hujjat satrlariga mos kelmaydi.", "Юборилган сатрлар ҳужжат сатрларига мос келмайди.",
        "Переданные строки не соответствуют строкам документа.", "Submitted items do not match document items.");
    public static Error MissingPostingBatch(long id, short? languageId) => Conflict("RentalAccrual.MissingPostingBatch", languageId,
        $"{id}-hujjat uchun o'tkazmalar paketi topilmadi.", $"{id}-ҳужжат учун ўтказмалар пакети топилмади.",
        $"Для документа {id} не найден пакет проводок.", $"Posting batch for document {id} was not found.");
    public static Error MissingAccountingEntries(long id, short? languageId) => Conflict("RentalAccrual.MissingEntries", languageId,
        $"{id}-hujjatning buxgalteriya yozuvlari topilmadi.", $"{id}-ҳужжатнинг бухгалтерия ёзувлари топилмади.",
        $"Проводки документа {id} не найдены.", $"Accounting entries for document {id} were not found.");
    public static Error EffectsAlreadyExist(long id, short? languageId) => Conflict("RentalAccrual.EffectsAlreadyExist", languageId,
        $"{id}-hujjat uchun o'tkazmalar allaqachon mavjud.", $"{id}-ҳужжат учун ўтказмалар аллақачон мавжуд.",
        $"Для документа {id} проводки уже существуют.", $"Accounting entries for document {id} already exist.");
    public static Error OrganizationScopeMismatch(short? languageId) => Conflict("RentalAccrual.OrganizationScopeMismatch", languageId,
        "Fon jarayonining tashkilot doirasi mos kelmadi.", "Фон жараёнининг ташкилот доираси мос келмади.",
        "Область организации фоновой операции не совпала.", "Background organization scope did not match.");
    public static Error InvalidAmounts(long id, short? languageId) => Business("RentalAccrual.InvalidAmounts", languageId,
        $"{id}-hujjat summalari shartnoma formulalariga mos kelmaydi.", $"{id}-ҳужжат суммалари шартнома формулаларига мос келмайди.",
        $"Суммы документа {id} не соответствуют формулам договора.", $"Document {id} amounts do not match the contract formulas.");

    private static Error Business(string code, short? lang, string uz, string cyrl, string ru, string en) => Error.Business(code, Message(lang, uz, cyrl, ru, en));
    private static Error Conflict(string code, short? lang, string uz, string cyrl, string ru, string en) => Error.Conflict(code, Message(lang, uz, cyrl, ru, en));
    private static Error NotFound(string code, short? lang, string uz, string cyrl, string ru, string en) => Error.NotFound(code, Message(lang, uz, cyrl, ru, en));
    private static string Message(short? lang, string uz, string cyrl, string ru, string en) => lang switch
    {
        LanguageIdConst.UZ => uz, LanguageIdConst.UZ_CYRL => cyrl, LanguageIdConst.RU => ru, _ => en
    };
}
