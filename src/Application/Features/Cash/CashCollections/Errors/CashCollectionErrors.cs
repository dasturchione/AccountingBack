using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.CashCollections;

public static class CashCollectionErrors
{
    public static Error NotFound(long id, short? languageId = null) => NotFoundError("CashCollection.NotFound", languageId,
        $"Id-si {id} bo'lgan inkassatsiya hujjati topilmadi.", $"Id-си {id} бўлган инкассация ҳужжати топилмади.",
        $"Документ инкассации с id {id} не найден.", $"Cash collection document {id} was not found.");

    public static Error InvalidStatus(long id, short statusId, short? languageId = null) => Conflict("CashCollection.InvalidStatus", languageId,
        $"Id-si {id} bo'lgan inkassatsiyani {statusId} holatida bajarib bo'lmaydi.", $"Id-си {id} бўлган инкассацияни {statusId} ҳолатида бажариб бўлмайди.",
        $"Документ инкассации {id} нельзя обработать в статусе {statusId}.", $"Cash collection document {id} cannot be processed in status {statusId}.");

    public static Error AlreadyCancelled(long id, short? languageId = null) => Conflict("CashCollection.AlreadyCancelled", languageId,
        $"Id-si {id} bo'lgan inkassatsiya allaqachon bekor qilingan.", $"Id-си {id} бўлган инкассация аллақачон бекор қилинган.",
        $"Документ инкассации {id} уже отменён.", $"Cash collection document {id} is already cancelled.");

    public static Error OrganizationMismatch(long id, short? languageId = null) => Business("CashCollection.OrganizationMismatch", languageId,
        $"Id-si {id} bo'lgan inkassatsiya boshqa tashkilotga tegishli.", $"Id-си {id} бўлган инкассация бошқа ташкилотга тегишли.",
        $"Документ инкассации {id} относится к другой организации.", $"Cash collection document {id} belongs to another organization.");

    public static Error NotInTransit(long id, short? languageId = null) => Business("CashCollection.NotInTransit", languageId,
        $"Id-si {id} bo'lgan inkassatsiya yo'lda holatida emas.", $"Id-си {id} бўлган инкассация йўлда ҳолатида эмас.",
        $"Документ инкассации {id} не находится в пути.", $"Cash collection document {id} is not in transit.");

    public static Error BankAccountMismatch(long id, short? languageId = null) => Business("CashCollection.BankAccountMismatch", languageId,
        $"Bank hisobvarag'i {id}-inkassatsiya hujjatiga mos kelmaydi.", $"Банк ҳисобварағи {id}-инкассация ҳужжатига мос келмайди.",
        $"Банковский счёт не соответствует документу инкассации {id}.", $"Bank account does not match cash collection document {id}.");

    public static Error DirectionMustBeIn(short? languageId = null) => Business("CashCollection.DirectionMustBeIn", languageId,
        "Inkassatsiyaga bog'langan bank operatsiyasi kirim bo'lishi kerak.", "Инкассацияга боғланган банк операцияси кирим бўлиши керак.",
        "Банковская операция, связанная с инкассацией, должна быть входящей.", "A bank operation linked to cash collection must be incoming.");

    public static Error CurrencyMismatch(long id, short? languageId = null) => Business("CashCollection.CurrencyMismatch", languageId,
        $"Valyuta {id}-inkassatsiya hujjatiga mos kelmaydi.", $"Валюта {id}-инкассация ҳужжатига мос келмайди.",
        $"Валюта не соответствует документу инкассации {id}.", $"Currency does not match cash collection document {id}.");

    public static Error AmountMismatch(long id, short? languageId = null) => Business("CashCollection.AmountMismatch", languageId,
        $"Summa {id}-inkassatsiya hujjatiga mos kelmaydi.", $"Сумма {id}-инкассация ҳужжатига мос келмайди.",
        $"Сумма не соответствует документу инкассации {id}.", $"Amount does not match cash collection document {id}.");

    public static Error MissingAccounts(long id, short? languageId = null) => Business("CashCollection.MissingAccounts", languageId,
        $"Id-si {id} bo'lgan inkassatsiya hisobvaraqlari to'liq ko'rsatilmagan.", $"Id-си {id} бўлган инкассация ҳисобварақлари тўлиқ кўрсатилмаган.",
        $"В документе инкассации {id} указаны не все бухгалтерские счета.", $"Cash collection document {id} has incomplete accounting accounts.");

    public static Error AlreadyLinked(long id, short? languageId = null) => Conflict("CashCollection.AlreadyLinked", languageId,
        $"Id-si {id} bo'lgan inkassatsiya faol bank operatsiyasiga allaqachon bog'langan.", $"Id-си {id} бўлган инкассация фаол банк операциясига аллақачон боғланган.",
        $"Документ инкассации {id} уже связан с активной банковской операцией.", $"Cash collection document {id} is already linked to an active bank operation.");

    public static Error ActiveBankOperation(long id, short? languageId = null) => Conflict("CashCollection.ActiveBankOperation", languageId,
        $"Avval {id}-inkassatsiyaga bog'langan faol bank operatsiyasini o'chiring yoki bekor qiling.",
        $"Аввал {id}-инкассацияга боғланган фаол банк операциясини ўчиринг ёки бекор қилинг.",
        $"Сначала удалите или отмените активную банковскую операцию, связанную с инкассацией {id}.",
        $"Remove or cancel the active bank operation linked to cash collection document {id} first.");

    public static Error CategoryNotFound(short? languageId = null) => NotFoundError("CashCollection.CategoryNotFound", languageId,
        "Faol CASH_COLLECTION bank operatsiyasi kategoriyasi topilmadi.", "Фаол CASH_COLLECTION банк операцияси категорияси топилмади.",
        "Активная категория банковских операций CASH_COLLECTION не найдена.", "Active CASH_COLLECTION bank operation category was not found.");

    public static Error CategoryMismatch(short categoryId, short? languageId = null) => Business("CashCollection.CategoryMismatch", languageId,
        $"{categoryId}-kategoriya CASH_COLLECTION emas.", $"{categoryId}-категория CASH_COLLECTION эмас.",
        $"Категория {categoryId} не является CASH_COLLECTION.", $"Classification category {categoryId} is not CASH_COLLECTION.");

    public static Error InvalidAmountOrRate(short? languageId = null) => InvalidConfiguration(languageId,
        "Summa va valyuta kursi noldan katta bo'lishi kerak.", "Сумма ва валюта курси нолдан катта бўлиши керак.",
        "Сумма и курс валюты должны быть больше нуля.", "Amount and exchange rate must be greater than zero.");
    public static Error CashBoxInvalid(short? languageId = null) => InvalidConfiguration(languageId,
        "Kassa faol emas yoki boshqa tashkilotga tegishli.", "Касса фаол эмас ёки бошқа ташкилотга тегишли.",
        "Касса неактивна или относится к другой организации.", "Cash box is inactive or belongs to another organization.");
    public static Error BankAccountInvalid(short? languageId = null) => InvalidConfiguration(languageId,
        "Bank hisobvarag'i faol emas yoki boshqa tashkilotga tegishli.", "Банк ҳисобварағи фаол эмас ёки бошқа ташкилотга тегишли.",
        "Банковский счёт неактивен или относится к другой организации.", "Bank account is inactive or belongs to another organization.");
    public static Error DocumentCurrenciesMismatch(short? languageId = null) => InvalidConfiguration(languageId,
        "Kassa, bank hisobvarag'i va hujjat valyutalari mos kelishi kerak.", "Касса, банк ҳисобварағи ва ҳужжат валюталари мос келиши керак.",
        "Валюты кассы, банковского счёта и документа должны совпадать.", "Cash box, bank account, and document currencies must match.");
    public static Error AccountsMustDiffer(short? languageId = null) => InvalidConfiguration(languageId,
        "Uchta turli buxgalteriya hisobvarag'i talab qilinadi.", "Учта турли бухгалтерия ҳисобварағи талаб қилинади.",
        "Требуются три разных бухгалтерских счёта.", "Three different accounting accounts are required.");
    public static Error AccountsInvalid(short? languageId = null) => InvalidConfiguration(languageId,
        "Buxgalteriya hisobvaraqlari faol emas yoki boshqa tashkilotga tegishli.", "Бухгалтерия ҳисобварақлари фаол эмас ёки бошқа ташкилотга тегишли.",
        "Бухгалтерские счета неактивны или относятся к другой организации.", "Accounting accounts are inactive or belong to another organization.");
    public static Error LinkedBankOperationInvalid(short? languageId = null) => InvalidConfiguration(languageId,
        "Bog'langan bank operatsiyasida inkassatsiya kategoriyasi va bank/yo'ldagi pul hisobvaraqlari kontragent hisob-kitoblarisiz ishlatilishi kerak.",
        "Боғланган банк операциясида инкассация категорияси ва банк/йўлдаги пул ҳисобварақлари контрагент ҳисоб-китобларисиз ишлатилиши керак.",
        "Связанная банковская операция должна использовать категорию инкассации и счета банка/денег в пути без расчётов с контрагентом.",
        "Linked bank operation must use cash-collection category and the bank/cash-in-transit accounts without counterparty settlement references.");

    public static Error InsufficientBalance(decimal available, decimal required, short? languageId = null) => Business("CashCollection.InsufficientBalance", languageId,
        $"Kassa qoldig'i {available}, talab qilinadigan summa esa {required}.", $"Касса қолдиғи {available}, талаб қилинадиган сумма эса {required}.",
        $"Остаток кассы {available} меньше требуемой суммы {required}.", $"Cash box balance {available} is less than required amount {required}.");

    public static Error BusinessEffectsAlreadyExist(long id, short? languageId = null) => Conflict("CashCollection.BusinessEffectsAlreadyExist", languageId,
        $"Id-si {id} bo'lgan inkassatsiyada moliyaviy harakatlar allaqachon mavjud.", $"Id-си {id} бўлган инкассацияда молиявий ҳаракатлар аллақачон мавжуд.",
        $"Документ инкассации {id} уже имеет финансовые движения.", $"Cash collection document {id} already has business effects.");
    public static Error MissingPostingBatch(long id, short? languageId = null) => Conflict("CashCollection.MissingPostingBatch", languageId,
        $"Id-si {id} bo'lgan inkassatsiya uchun o'tkazmalar paketi topilmadi.", $"Id-си {id} бўлган инкассация учун ўтказмалар пакети топилмади.",
        $"Для документа инкассации {id} не найден пакет проводок.", $"Posting batch for cash collection document {id} was not found.");
    public static Error MissingAccountingEntries(long id, short? languageId = null) => Conflict("CashCollection.MissingAccountingEntries", languageId,
        $"Id-si {id} bo'lgan inkassatsiya buxgalteriya yozuvlari topilmadi.", $"Id-си {id} бўлган инкассация бухгалтерия ёзувлари топилмади.",
        $"Бухгалтерские проводки документа инкассации {id} не найдены.", $"Accounting entries for cash collection document {id} were not found.");
    public static Error MissingMoneyEntries(long id, short? languageId = null) => Conflict("CashCollection.MissingMoneyEntries", languageId,
        $"Id-si {id} bo'lgan inkassatsiya pul registri yozuvlari topilmadi.", $"Id-си {id} бўлган инкассация пул регистри ёзувлари топилмади.",
        $"Денежные движения документа инкассации {id} не найдены.", $"Money entries for cash collection document {id} were not found.");
    public static Error CompletedBankOperationMustBeCancelled(long id, short? languageId = null) => Conflict("CashCollection.CancelBankOperationFirst", languageId,
        $"Avval {id}-inkassatsiyaga bog'langan o'tkazilgan bank operatsiyasini bekor qiling.", $"Аввал {id}-инкассацияга боғланган ўтказилган банк операциясини бекор қилинг.",
        $"Сначала отмените проведённую банковскую операцию, связанную с инкассацией {id}.", $"Cancel the posted bank operation linked to cash collection document {id} first.");

    private static Error InvalidConfiguration(short? languageId, string uz, string uzCyrl, string ru, string en) => Business("CashCollection.InvalidConfiguration", languageId, uz, uzCyrl, ru, en);
    private static Error Business(string code, short? languageId, string uz, string uzCyrl, string ru, string en) => Error.Business(code, Message(languageId, uz, uzCyrl, ru, en));
    private static Error Conflict(string code, short? languageId, string uz, string uzCyrl, string ru, string en) => Error.Conflict(code, Message(languageId, uz, uzCyrl, ru, en));
    private static Error NotFoundError(string code, short? languageId, string uz, string uzCyrl, string ru, string en) => Error.NotFound(code, Message(languageId, uz, uzCyrl, ru, en));
    private static string Message(short? languageId, string uz, string uzCyrl, string ru, string en) => languageId switch
    {
        LanguageIdConst.UZ => uz, LanguageIdConst.UZ_CYRL => uzCyrl, LanguageIdConst.RU => ru, _ => en
    };
}
