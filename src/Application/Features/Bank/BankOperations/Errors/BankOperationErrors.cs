using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.BankOperations;

public static class BankOperationErrors
{
    public static Error ChartAccountRequired(short? languageId = null) => B("ChartAccountRequired", languageId,
        "O'tkazma uchun bank va korrespondent hisobvaraqlari ko'rsatilishi kerak.", "Ўтказма учун банк ва корреспондент ҳисобварақлари кўрсатилиши керак.",
        "Для проведения операции необходимо указать банковский и корреспондирующий счета.", "Bank and offset chart accounts are required for posting.");

    public static Error InsufficientBalance(int bankAccountId, decimal amount, short? languageId = null) => B("InsufficientBalance", languageId,
        $"{bankAccountId} bank hisobvarag'ida {amount} summa uchun mablag' yetarli emas.", $"{bankAccountId} банк ҳисобварағида {amount} сумма учун маблағ етарли эмас.",
        $"На банковском счёте {bankAccountId} недостаточно средств для суммы {amount}.", $"Bank account {bankAccountId} has insufficient balance for amount {amount}.");

    public static Error RelatedDocumentNotFound(long id, short? languageId = null) => N("RelatedDocumentNotFound", languageId,
        $"Joriy tashkilotda faol bog'liq hujjat {id} topilmadi.", $"Жорий ташкилотда фаол боғлиқ ҳужжат {id} топилмади.",
        $"Активный связанный документ {id} не найден в текущей организации.", $"Active related document {id} was not found in the current organization.");

    public static Error RelatedDocumentOrganizationMismatch(long id, short? languageId = null) => B("RelatedDocumentOrganizationMismatch", languageId,
        $"Bog'liq hujjat {id} boshqa tashkilotga tegishli.", $"Боғлиқ ҳужжат {id} бошқа ташкилотга тегишли.",
        $"Связанный документ {id} относится к другой организации.", $"Related document {id} belongs to another organization.");

    public static Error RelatedDocumentInactive(long id, short? languageId = null) => B("RelatedDocumentInactive", languageId,
        $"Bog'liq hujjat {id} faol emas.", $"Боғлиқ ҳужжат {id} фаол эмас.",
        $"Связанный документ {id} неактивен.", $"Related document {id} is inactive.");

    public static Error ClassificationCategoryRequired(short? languageId = null) => B("ClassificationCategoryRequired", languageId,
        "Qoida ko'rsatilganda tasniflash kategoriyasi ham ko'rsatilishi kerak.", "Қоида кўрсатилганда таснифлаш категорияси ҳам кўрсатилиши керак.",
        "При указании правила требуется категория классификации.", "Classification category is required when a rule is specified.");

    public static Error ClassificationCategoryNotFound(short categoryId, short? languageId = null) => N("ClassificationCategoryNotFound", languageId,
        $"Faol tasniflash kategoriyasi {categoryId} topilmadi.", $"Фаол таснифлаш категорияси {categoryId} топилмади.",
        $"Активная категория классификации {categoryId} не найдена.", $"Active classification category {categoryId} was not found.");

    public static Error ClassificationRuleNotFound(int ruleId, short? languageId = null) => N("ClassificationRuleNotFound", languageId,
        $"Faol tasniflash qoidasi {ruleId} topilmadi.", $"Фаол таснифлаш қоидаси {ruleId} топилмади.",
        $"Активное правило классификации {ruleId} не найдено.", $"Active classification rule {ruleId} was not found.");

    public static Error ClassificationBankAccountNotFound(int bankAccountId, short? languageId = null) => N("ClassificationBankAccountNotFound", languageId,
        $"Joriy tashkilotda faol bank hisobvarag'i {bankAccountId} topilmadi.", $"Жорий ташкилотда фаол банк ҳисобварағи {bankAccountId} топилмади.",
        $"Активный банковский счёт {bankAccountId} не найден в текущей организации.", $"Active bank account {bankAccountId} was not found in the current organization.");

    public static Error ClassificationCategoryMismatch(int ruleId, short categoryId, short? languageId = null) => B("ClassificationCategoryMismatch", languageId,
        $"{ruleId}-qoida {categoryId}-kategoriyaga tegishli emas.", $"{ruleId}-қоида {categoryId}-категорияга тегишли эмас.",
        $"Правило {ruleId} не относится к категории {categoryId}.", $"Rule {ruleId} does not belong to category {categoryId}.");

    public static Error ClassificationBankMismatch(int ruleId, int bankId, short? languageId = null) => B("ClassificationBankMismatch", languageId,
        $"{ruleId}-qoida {bankId}-bankka tegishli emas.", $"{ruleId}-қоида {bankId}-банкка тегишли эмас.",
        $"Правило {ruleId} не относится к банку {bankId}.", $"Rule {ruleId} does not belong to bank {bankId}.");

    public static Error NotFound(long id, short? languageId = null) => N("NotFound", languageId,
        $"Id-si {id} bo'lgan bank operatsiyasi topilmadi.", $"Id-си {id} бўлган банк операцияси топилмади.",
        $"Банковская операция с id {id} не найдена.", $"Bank operation with id {id} was not found.");

    public static Error CannotUpdateInCurrentStatus(long id, short statusId, short? languageId = null) => InvalidStatus(id, statusId, "o'zgartirish", "ўзгартириш", "изменить", "update", languageId);
    public static Error CannotDeleteInCurrentStatus(long id, short statusId, short? languageId = null) => InvalidStatus(id, statusId, "o'chirish", "ўчириш", "удалить", "delete", languageId);
    public static Error CannotConfirmInCurrentStatus(long id, short statusId, short? languageId = null) => InvalidStatus(id, statusId, "tasdiqlash", "тасдиқлаш", "подтвердить", "confirm", languageId);
    public static Error CannotCancelInCurrentStatus(long id, short statusId, short? languageId = null) => InvalidStatus(id, statusId, "bekor qilish", "бекор қилиш", "отменить", "cancel", languageId);

    public static Error OrganizationMismatch(long id, short? languageId = null) => C("OrganizationMismatch", languageId,
        $"Bank operatsiyasi {id} joriy tashkilotga tegishli emas.", $"Банк операцияси {id} жорий ташкилотга тегишли эмас.",
        $"Банковская операция {id} не принадлежит текущей организации.", $"Bank operation {id} does not belong to the current organization.");

    public static Error AlreadyCancelled(long id, short? languageId = null) => C("AlreadyCancelled", languageId,
        $"Bank operatsiyasi {id} allaqachon bekor qilingan.", $"Банк операцияси {id} аллақачон бекор қилинган.",
        $"Банковская операция {id} уже отменена.", $"Bank operation {id} is already cancelled.");

    public static Error DuplicateBankDocumentNumber(
        string bankDocumentNumber,
        DateOnly documentDate,
        short? languageId = null) => C("DuplicateBankDocumentNumber", languageId,
        $"{documentDate:yyyy-MM-dd} sanadagi {bankDocumentNumber} bank hujjat raqamli operatsiya allaqachon mavjud.",
        $"{documentDate:yyyy-MM-dd} санадаги {bankDocumentNumber} банк ҳужжат рақамли операция аллақачон мавжуд.",
        $"Банковская операция с номером документа {bankDocumentNumber} за {documentDate:yyyy-MM-dd} уже существует.",
        $"Bank operation with document number {bankDocumentNumber} for {documentDate:yyyy-MM-dd} already exists.");

    public static Error BusinessEffectsAlreadyExist(long id, short? languageId = null) => C("BusinessEffectsAlreadyExist", languageId,
        $"Bank operatsiyasi {id} uchun o'tkazmalar yoki registr yozuvlari allaqachon yaratilgan.", $"Банк операцияси {id} учун ўтказмалар ёки регистр ёзувлари аллақачон яратилган.",
        $"По банковской операции {id} уже созданы проводки или записи регистров.", $"Bank operation {id} already has business effects.");

    public static Error MissingPostingBatch(long id, short? languageId = null) => C("MissingPostingBatch", languageId,
        $"Bank operatsiyasi {id} uchun o'tkazmalar paketi topilmadi.", $"Банк операцияси {id} учун ўтказмалар пакети топилмади.",
        $"Для банковской операции {id} не найден пакет проводок.", $"Posting batch was not found for bank operation {id}.");

    public static Error MissingAccountingRegisterEntries(long id, short? languageId = null) => C("MissingAccountingEntries", languageId,
        $"Bank operatsiyasi {id} uchun buxgalteriya yozuvlari topilmadi.", $"Банк операцияси {id} учун бухгалтерия ёзувлари топилмади.",
        $"Для банковской операции {id} не найдены бухгалтерские проводки.", $"Accounting register entries were not found for bank operation {id}.");

    public static Error MissingMoneyRegisterEntries(long id, short? languageId = null) => C("MissingMoneyEntries", languageId,
        $"Bank operatsiyasi {id} uchun pul registri yozuvlari topilmadi.", $"Банк операцияси {id} учун пул регистри ёзувлари топилмади.",
        $"Для банковской операции {id} не найдены записи денежного регистра.", $"Money register entries were not found for bank operation {id}.");

    public static Error MissingCounterpartyRegisterEntries(long id, short? languageId = null) => C("MissingCounterpartyEntries", languageId,
        $"Bank operatsiyasi {id} uchun kontragent registri yozuvlari topilmadi.", $"Банк операцияси {id} учун контрагент регистри ёзувлари топилмади.",
        $"Для банковской операции {id} не найдены записи регистра контрагента.", $"Counterparty register entries were not found for bank operation {id}.");

    public static Error InvalidAmount(long id, short? languageId = null) => B("InvalidAmount", languageId,
        $"Bank operatsiyasi {id} summasi noto'g'ri.", $"Банк операцияси {id} суммаси нотўғри.",
        $"У банковской операции {id} некорректная сумма.", $"Bank operation {id} has an invalid amount.");

    public static Error InvalidDirection(short directionId, short? languageId = null) => B("InvalidDirection", languageId,
        $"Bank operatsiyasi yo'nalishi {directionId} qo'llab-quvvatlanmaydi.", $"Банк операцияси йўналиши {directionId} қўллаб-қувватланмайди.",
        $"Направление банковской операции {directionId} не поддерживается.", $"Bank movement direction {directionId} is not supported.");

    public static Error InvalidLineConfiguration(long id, short? languageId = null) => B("InvalidLineConfiguration", languageId,
        $"Bank operatsiyasi {id} qatorlari noto'g'ri sozlangan.", $"Банк операцияси {id} қаторлари нотўғри созланган.",
        $"У банковской операции {id} некорректная конфигурация строк.", $"Bank operation {id} has an invalid line configuration.");

    public static Error InvalidOrganizationReference(string referenceName, short? languageId = null) => B("InvalidOrganizationReference", languageId,
        $"Bog'liq {referenceName} joriy tashkilotga tegishli emas yoki faol emas.", $"Боғлиқ {referenceName} жорий ташкилотга тегишли эмас ёки фаол эмас.",
        $"Связанный справочник {referenceName} не принадлежит текущей организации или неактивен.", $"Reference {referenceName} does not belong to the current organization or is inactive.");

    public static Error InvalidCurrencyMismatch(short? languageId = null) => B("InvalidCurrencyMismatch", languageId,
        "Hujjat valyutasi bank hisobvarag'i valyutasiga mos kelmaydi.", "Ҳужжат валютаси банк ҳисобварағи валютасига мос келмайди.",
        "Валюта документа не соответствует валюте банковского счёта.", "Document currency does not match the bank account currency.");

    private static Error InvalidStatus(long id, short statusId, string uzOp, string uzCyrlOp, string ruOp, string enOp, short? languageId) =>
        C("InvalidStatus", languageId,
            $"Bank operatsiyasi {id}ni {statusId} holatida {uzOp} mumkin emas.", $"Банк операцияси {id}ни {statusId} ҳолатида {uzCyrlOp} мумкин эмас.",
            $"Банковскую операцию {id} нельзя {ruOp} в статусе {statusId}.", $"Bank operation {id} cannot {enOp} in status {statusId}.");

    private static Error B(string code, short? languageId, string uz, string uzCyrl, string ru, string en) => Error.Business($"BankOperation.{code}", Message(languageId, uz, uzCyrl, ru, en));
    private static Error C(string code, short? languageId, string uz, string uzCyrl, string ru, string en) => Error.Conflict($"BankOperation.{code}", Message(languageId, uz, uzCyrl, ru, en));
    private static Error N(string code, short? languageId, string uz, string uzCyrl, string ru, string en) => Error.NotFound($"BankOperation.{code}", Message(languageId, uz, uzCyrl, ru, en));
    private static string Message(short? languageId, string uz, string uzCyrl, string ru, string en) => languageId switch
    {
        LanguageIdConst.UZ => uz,
        LanguageIdConst.UZ_CYRL => uzCyrl,
        LanguageIdConst.RU => ru,
        _ => en
    };
}
