using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.PurchaseDocs;

public static class EdoImportPreflightErrors
{
    public static Error OrganizationContextRequired(bool userRequired = false, short? languageId = null) =>
        Forbidden("EdoImport.OrganizationContextRequired", languageId,
            userRequired
                ? "Autentifikatsiyadan o'tgan tashkilot va foydalanuvchi konteksti talab qilinadi."
                : "Autentifikatsiyadan o'tgan tashkilot konteksti talab qilinadi.",
            userRequired
                ? "Аутентификациядан ўтган ташкилот ва фойдаланувчи контексти талаб қилинади."
                : "Аутентификациядан ўтган ташкилот контексти талаб қилинади.",
            userRequired
                ? "Требуется контекст аутентифицированной организации и пользователя."
                : "Требуется контекст аутентифицированной организации.",
            userRequired
                ? "An authenticated organization and user context are required."
                : "An authenticated organization context is required.");

    public static Error AccountingStartDateRequired(short? languageId = null) => Business(
        "EdoImport.AccountingStartDateRequired", languageId,
        "dateFrom berilmagan bo'lsa, OrganizationConfig.AccountingStartDate sozlanishi kerak.",
        "dateFrom берилмаган бўлса, OrganizationConfig.AccountingStartDate созланиши керак.",
        "Если dateFrom не передан, необходимо настроить OrganizationConfig.AccountingStartDate.",
        "OrganizationConfig.AccountingStartDate must be configured when dateFrom is omitted.");

    public static Error InvalidDateRange(short? languageId = null) => Business(
        "EdoImport.InvalidDateRange", languageId,
        "DateFrom DateTo dan kech bo'lishi mumkin emas.",
        "DateFrom DateTo дан кеч бўлиши мумкин эмас.",
        "DateFrom не может быть позже DateTo.",
        "DateFrom must not be later than DateTo.");

    public static Error ActiveJobExists(short? languageId = null) => Conflict(
        "EdoImport.ActiveJobExists", languageId,
        "Ushbu tashkilot uchun faol EDO import vazifasi allaqachon mavjud.",
        "Ушбу ташкилот учун фаол EDO импорт вазифаси аллақачон мавжуд.",
        "Для этой организации уже существует активная задача импорта EDO.",
        "An active EDO import job already exists for this organization.");

    public static Error ActiveProviderUnsupported(short? languageId = null) => Business(
        "EdoImport.ActiveProviderUnsupported", languageId,
        "Faol EDO provayderi tarixiy xarid tekshiruvini qo'llab-quvvatlamaydi.",
        "Фаол EDO провайдери тарихий харид текширувини қўллаб-қувватламайди.",
        "Активный провайдер EDO не поддерживает предварительную проверку исторических закупок.",
        "Historical Purchase preflight is not supported for the active EDO provider.");

    public static Error JobNotFound(long jobId, short? languageId = null) => NotFound(
        "EdoImport.JobNotFound", languageId,
        $"Ushbu tashkilotda {jobId}-raqamli EDO import vazifasi topilmadi.",
        $"Ушбу ташкилотда {jobId}-рақамли EDO импорт вазифаси топилмади.",
        $"Задача импорта EDO {jobId} не найдена в этой организации.",
        $"EDO import job {jobId} was not found in this organization.");

    public static Error CandidateNotFound(short? languageId = null) => NotFound(
        "EdoImport.CandidateNotFound", languageId,
        "Ushbu tashkilot vazifasida EDO import nomzodi topilmadi.",
        "Ушбу ташкилот вазифасида EDO импорт номзоди топилмади.",
        "Кандидат импорта EDO не найден в задаче этой организации.",
        "The EDO import candidate was not found in this organization job.");

    public static Error DraftConfirmationRequired(short? languageId = null) => Business(
        "DRAFT_IMPORT_CONFIRMATION_REQUIRED", languageId,
        "Xarid qoralamalarini import qilishni aniq tasdiqlash kerak.",
        "Харид қораламаларини импорт қилишни аниқ тасдиқлаш керак.",
        "Необходимо явно подтвердить импорт черновиков закупок.",
        "Explicit Draft Purchase import confirmation is required.");

    public static Error DraftRequestInvalid(short? languageId = null) => Business(
        "DRAFT_IMPORT_REQUEST_INVALID", languageId,
        "Paket hajmi 1 dan 50 gacha bo'lishi va import rejasi xeshi ko'rsatilishi kerak.",
        "Пакет ҳажми 1 дан 50 гача бўлиши ва импорт режаси хеши кўрсатилиши керак.",
        "Размер пакета должен быть от 1 до 50, также требуется хеш плана импорта.",
        "Batch size must be between 1 and 50 and the import plan hash is required.");

    public static Error DraftFactoryUnavailable(short? languageId = null) => Conflict(
        "DRAFT_IMPORT_FACTORY_UNAVAILABLE", languageId,
        "Tarixiy xarid qoralamasini yaratish xizmati mavjud emas.",
        "Тарихий харид қораламасини яратиш хизмати мавжуд эмас.",
        "Сервис создания исторического черновика закупки недоступен.",
        "The historical Draft Purchase factory is unavailable.");

    public static Error JobNotImportable(string operation, short? languageId = null) => Conflict(
        "EdoImport.JobNotImportable", languageId,
        $"{operation} faqat PREFLIGHT_READY yoki PARTIAL holatidagi vazifalar uchun ruxsat etiladi.",
        $"{operation} фақат PREFLIGHT_READY ёки PARTIAL ҳолатидаги вазифалар учун рухсат этилади.",
        $"Операция «{operation}» разрешена только для задач в статусе PREFLIGHT_READY или PARTIAL.",
        $"{operation} is allowed only for PREFLIGHT_READY or PARTIAL jobs.");

    public static Error StaleImportPlan(short? languageId = null) => Conflict(
        "STALE_IMPORT_PLAN", languageId,
        "Xarid qoralamalarini import qilish rejasi o'zgargan, uni qayta ko'rib chiqing.",
        "Харид қораламаларини импорт қилиш режаси ўзгарган, уни қайта кўриб чиқинг.",
        "План импорта черновиков закупок изменился; проверьте его повторно.",
        "The Draft Purchase import plan changed and must be reviewed again.");

    public static Error BulkRequestInvalid(short? languageId = null) => Business(
        "BULK_DRAFT_IMPORT_REQUEST_INVALID", languageId,
        "Ommaviy import uchun tasdiq, joriy reja xeshi va qo'llab-quvvatlanadigan siyosatlar kerak.",
        "Оммавий импорт учун тасдиқ, жорий режа хеши ва қўллаб-қувватланадиган сиёсатлар керак.",
        "Для массового импорта требуются подтверждение, текущий хеш плана и поддерживаемые политики.",
        "Bulk Draft import requires explicit confirmation, current plan hash and supported policies.");

    public static Error BulkAlreadyActive(short? languageId = null) => Conflict(
        "BULK_DRAFT_IMPORT_ACTIVE", languageId,
        "Ushbu tashkilot uchun ommaviy import allaqachon faol.",
        "Ушбу ташкилот учун оммавий импорт аллақачон фаол.",
        "Для этой организации уже выполняется массовый импорт черновиков.",
        "A bulk Draft import is already active for this organization.");

    public static Error BulkNotActive(short? languageId = null) => Conflict(
        "BULK_DRAFT_IMPORT_NOT_ACTIVE", languageId,
        "Faol ommaviy import mavjud emas.",
        "Фаол оммавий импорт мавжуд эмас.",
        "Активный массовый импорт черновиков отсутствует.",
        "No active bulk Draft import exists.");

    public static Error DraftFailureConfirmationRequired(short? languageId = null) => Business(
        "DRAFT_IMPORT_FAILURE_CONFIRMATION_REQUIRED", languageId,
        "Import xatolarini hal qilishni aniq tasdiqlash kerak.",
        "Импорт хатоларини ҳал қилишни аниқ тасдиқлаш керак.",
        "Необходимо явно подтвердить обработку ошибок импорта черновиков.",
        "Explicit Draft import failure confirmation is required.");

    public static Error InvalidDraftFailureSelection(short? languageId = null) => Conflict(
        "DRAFT_IMPORT_FAILURE_SELECTION_INVALID", languageId,
        "Faqat joriy, xaridga bog'lanmagan va qayta urinib bo'lmaydigan tarixiy tekshiruv xatolarini o'tkazib yuborish mumkin.",
        "Фақат жорий, харидга боғланмаган ва қайта уриниб бўлмайдиган тарихий текширув хатоларини ўтказиб юбориш мумкин.",
        "Пропускать можно только текущие, не связанные с закупкой и неповторяемые ошибки исторической проверки.",
        "Only current unlinked non-retryable historical validation failures can be skipped.");

    public static Error StaleDraftFailurePlan(short? languageId = null) => Conflict(
        "STALE_DRAFT_IMPORT_FAILURE_PLAN", languageId,
        "Import xatolari rejasi o'zgargan, uni qayta ko'rib chiqing.",
        "Импорт хатолари режаси ўзгарган, уни қайта кўриб чиқинг.",
        "План ошибок импорта черновиков изменился; проверьте его повторно.",
        "The Draft import failure plan changed and must be reviewed again.");

    public static Error DraftRequeueConfirmationRequired(short? languageId = null) => Business(
        "DRAFT_IMPORT_REQUEUE_CONFIRMATION_REQUIRED", languageId,
        "Nomzodni qayta navbatga qo'yishni aniq tasdiqlash kerak.",
        "Номзодни қайта навбатга қўйишни аниқ тасдиқлаш керак.",
        "Необходимо явно подтвердить повторную постановку кандидата в очередь.",
        "Explicit Draft import requeue confirmation is required.");

    public static Error DraftCandidateNotFound(short? languageId = null) => NotFound(
        "DRAFT_IMPORT_CANDIDATE_NOT_FOUND", languageId,
        "Ushbu tashkilot vazifasida import nomzodi topilmadi.",
        "Ушбу ташкилот вазифасида импорт номзоди топилмади.",
        "Кандидат импорта черновика не найден в задаче этой организации.",
        "The Draft import candidate was not found in this organization job.");

    public static Error DraftRequeueNotAllowed(short? languageId = null) => Conflict(
        "DRAFT_IMPORT_REQUEUE_NOT_ALLOWED", languageId,
        "Faqat xaridga bog'lanmagan va tuzatilgan tekshiruvdan o'tmagan nomzodni qayta navbatga qo'yish mumkin.",
        "Фақат харидга боғланмаган ва тузатилган текширувдан ўтмаган номзодни қайта навбатга қўйиш мумкин.",
        "Повторно поставить в очередь можно только не связанного с закупкой кандидата после исправления проверки.",
        "Only an unlinked candidate failed by a corrected Draft validation can be requeued.");

    public static Error DraftRequeueMappingInvalid(short? languageId = null) => Conflict(
        "DRAFT_IMPORT_REQUEUE_MAPPING_INVALID", languageId,
        "Nomzod mosliklari yoki markirovka holati endi import uchun yaroqsiz.",
        "Номзод мосликлари ёки маркировка ҳолати энди импорт учун яроқсиз.",
        "Сопоставления кандидата или состояние маркировок больше не подходят для импорта.",
        "The candidate mappings or marking state are no longer valid for Draft import.");

    public static Error DraftRequeuePurchaseLinked(short? languageId = null) => Conflict(
        "DRAFT_IMPORT_REQUEUE_PURCHASE_LINKED", languageId,
        "Xaridga bog'langan nomzodni qayta navbatga qo'yib bo'lmaydi.",
        "Харидга боғланган номзодни қайта навбатга қўйиб бўлмайди.",
        "Кандидата, связанного с закупкой, нельзя повторно поставить в очередь.",
        "A candidate linked to a Purchase cannot be requeued.");

    public static Error DraftImportSourceInvalid(short? languageId = null) => Business(
        "DRAFT_IMPORT_SOURCE_INVALID", languageId,
        "Tarixiy nomzod manbasi yoki saqlangan mosliklar to'liq emas.",
        "Тарихий номзод манбаси ёки сақланган мосликлар тўлиқ эмас.",
        "Источник исторического кандидата или сохранённые сопоставления неполны.",
        "The historical candidate source or persisted mappings are incomplete.");

    public static Error PieceTrackingConfirmationRequired(short? languageId = null) => Business(
        "PIECE_TRACKING_CONFIRMATION_REQUIRED", languageId,
        "Donabay kuzatuvni yoqishni aniq tasdiqlash kerak.",
        "Донабай кузатувни ёқишни аниқ тасдиқлаш керак.",
        "Необходимо явно подтвердить включение поштучного учёта.",
        "Explicit piece-tracking confirmation is required.");

    public static Error PieceTrackingSelectionInvalid(short? languageId = null) => Business(
        "PIECE_TRACKING_SELECTION_INVALID", languageId,
        "Mahsulot identifikatorlari musbat va takrorlanmas bo'lishi kerak.",
        "Маҳсулот идентификаторлари мусбат ва такрорланмас бўлиши керак.",
        "Идентификаторы товаров должны быть положительными и уникальными.",
        "Product IDs must be positive and unique.");

    public static Error PieceTrackingPlanInvalid(bool stale, short? languageId = null) => Conflict(
        stale ? "STALE_PIECE_TRACKING_PLAN" : "PIECE_TRACKING_SELECTION_INVALID", languageId,
        stale
            ? "Donabay kuzatuv rejasi o'zgargan; o'zgarishlarni qo'llashdan oldin uni yangilang."
            : "Faqat donabay kuzatuv uchun taklif qilingan mahsulotlarni tanlash mumkin.",
        stale
            ? "Донабай кузатув режаси ўзгарган; ўзгаришларни қўллашдан олдин уни янгиланг."
            : "Фақат донабай кузатув учун таклиф қилинган маҳсулотларни танлаш мумкин.",
        stale
            ? "План поштучного учёта изменился; обновите его перед применением."
            : "Можно выбрать только товары, явно предложенные для поштучного учёта.",
        stale
            ? "The piece-tracking plan has changed; refresh it before applying changes."
            : "Only products explicitly offered for piece-tracking can be selected.");

    public static Error PieceTrackingCannotBeEnabled(string code, short? languageId = null) => Conflict(
        code, languageId,
        "Tanlangan mahsulot uchun donabay kuzatuvni xavfsiz yoqib bo'lmaydi.",
        "Танланган маҳсулот учун донабай кузатувни хавфсиз ёқиб бўлмайди.",
        "Для выбранного товара нельзя безопасно включить поштучный учёт.",
        "The selected product cannot be safely enabled for piece tracking.");

    public static Error MasterDataConfirmationRequired(bool productDefaults = false, short? languageId = null) => Business(
        "MASTER_DATA_CONFIRMATION_REQUIRED", languageId,
        productDefaults ? "Mahsulot standartlarini aniq tasdiqlash kerak." : "Asosiy ma'lumotlarni qo'llashni aniq tasdiqlash kerak.",
        productDefaults ? "Маҳсулот стандартларини аниқ тасдиқлаш керак." : "Асосий маълумотларни қўллашни аниқ тасдиқлаш керак.",
        productDefaults ? "Необходимо явно подтвердить настройки товаров." : "Необходимо явно подтвердить применение справочных данных.",
        productDefaults ? "Explicit product defaults confirmation is required." : "Explicit master-data apply confirmation is required.");

    public static Error MasterDataCannotBeApplied(string code, short? languageId = null) => Conflict(
        code, languageId,
        "Tanlangan asosiy ma'lumot elementini xavfsiz qo'llab bo'lmaydi.",
        "Танланган асосий маълумот элементини хавфсиз қўллаб бўлмайди.",
        "Выбранный элемент справочных данных нельзя безопасно применить.",
        "The selected master-data item cannot be applied safely.");

    public static Error MarkingPolicyInvalid(short? languageId = null) => Business(
        "MARKING_POLICY_INVALID", languageId,
        "Markirovka siyosati provayder markirovkasi zarur bo'lganda donabay kuzatuvni saqlashi kerak.",
        "Маркировка сиёсати провайдер маркировкаси зарур бўлганда донабай кузатувни сақлаши керак.",
        "Политика маркировки должна сохранять поштучный учёт, когда требуются маркировки провайдера.",
        "The marking policy must explicitly preserve piece tracking when provider markings are required.");

    public static Error PackageUnitMappingInvalid(bool invalidUnit = false, short? languageId = null) => Business(
        "PACKAGE_UNIT_MAPPING_INVALID", languageId,
        invalidUnit
            ? "Har bir qadoq mosligi musbat o'lchov birligi identifikatoriga murojaat qilishi kerak."
            : "Qadoq va o'lchov birligi mosliklari bo'sh bo'lmasligi, aniq va takrorlanmas bo'lishi kerak.",
        invalidUnit
            ? "Ҳар бир қадоқ мослиги мусбат ўлчов бирлиги идентификаторига мурожаат қилиши керак."
            : "Қадоқ ва ўлчов бирлиги мосликлари бўш бўлмаслиги, аниқ ва такрорланмас бўлиши керак.",
        invalidUnit
            ? "Каждое сопоставление упаковки должно ссылаться на положительный идентификатор единицы измерения."
            : "Сопоставления упаковок и единиц должны быть непустыми, точными и уникальными.",
        invalidUnit
            ? "Every package mapping must reference a positive unit ID."
            : "Package to unit mappings must be non-empty, exact and unique.");

    public static Error ProductConflictConfirmationRequired(short? languageId = null) => Business(
        "PRODUCT_CONFLICT_CONFIRMATION_REQUIRED", languageId,
        "Provayder mahsuloti mosligini aniq tasdiqlash kerak.",
        "Провайдер маҳсулоти мослигини аниқ тасдиқлаш керак.",
        "Необходимо явно подтвердить сопоставление товара провайдера.",
        "Explicit provider product mapping confirmation is required.");

    public static Error ProductConflictSelectionInvalid(short? languageId = null) => Business(
        "PRODUCT_CONFLICT_SELECTION_INVALID", languageId,
        "Mahsulot nizosi tanlovlari bo'sh bo'lmasligi va takrorlanmas bo'lishi kerak.",
        "Маҳсулот низоси танловлари бўш бўлмаслиги ва такрорланмас бўлиши керак.",
        "Выбор конфликтов товаров провайдера должен быть непустым и уникальным.",
        "Provider product conflict selections must be non-empty and unique.");

    public static Error ProductConflictSelectionDoesNotMatch(short? languageId = null) => Business(
        "PRODUCT_CONFLICT_SELECTION_INVALID", languageId,
        "Har bir tanlov aynan bitta joriy, hal qilinmagan mahsulot identifikatoriga mos kelishi kerak.",
        "Ҳар бир танлов айнан битта жорий, ҳал қилинмаган маҳсулот идентификаторига мос келиши керак.",
        "Каждый выбор должен точно соответствовать одной текущей неразрешённой идентичности товара провайдера.",
        "Every selection must match one current unresolved provider product identity exactly.");

    public static Error ProductMarkingSelectionInvalid(short? languageId = null) => Conflict(
        "PRODUCT_MARKING_SELECTION_INVALID", languageId,
        "Donabay kuzatuv provayder markirovkasi talablarini saqlashi kerak va xizmatlar uchun yoqilmaydi.",
        "Донабай кузатув провайдер маркировкаси талабларини сақлаши керак ва хизматлар учун ёқилмайди.",
        "Поштучный учёт должен сохранять требования маркировки провайдера и не может включаться для услуг.",
        "Piece tracking must preserve provider marking requirements and cannot be enabled for services.");

    public static Error ProductConflictCannotBeApplied(string code, short? languageId = null) => Conflict(
        code, languageId,
        "Provayder mahsuloti mosligini xavfsiz qo'llab bo'lmaydi.",
        "Провайдер маҳсулоти мослигини хавфсиз қўллаб бўлмайди.",
        "Сопоставление товара провайдера нельзя безопасно применить.",
        "The provider product mapping cannot be applied safely.");

    public static Error MarkingConflictConfirmationRequired(short? languageId = null) => Business(
        "MARKING_CONFLICT_CONFIRMATION_REQUIRED", languageId,
        "Markirovka nizolarini hal qilishni aniq tasdiqlash kerak.",
        "Маркировка низоларини ҳал қилишни аниқ тасдиқлаш керак.",
        "Необходимо явно подтвердить обработку конфликтов маркировки.",
        "Explicit marking conflict confirmation is required.");

    public static Error MarkingConflictSelectionInvalid(bool currentOnly = false, short? languageId = null) =>
        currentOnly
            ? Conflict("MARKING_CONFLICT_SELECTION_INVALID", languageId,
                "Faqat joriy va qo'llab-quvvatlanadigan markirovka nizolarini o'tkazib yuborish mumkin.",
                "Фақат жорий ва қўллаб-қувватланадиган маркировка низоларини ўтказиб юбориш мумкин.",
                "Пропускать можно только текущие поддерживаемые конфликты маркировки.",
                "Only current supported marking conflict candidates can be skipped.")
            : Business("MARKING_CONFLICT_SELECTION_INVALID", languageId,
                "Tanlov bo'sh bo'lmasligi, takrorlanmasligi va SKIP amalidan foydalanishi kerak.",
                "Танлов бўш бўлмаслиги, такрорланмаслиги ва SKIP амалидан фойдаланиши керак.",
                "Выбор должен быть непустым, уникальным и использовать действие SKIP.",
                "Marking conflict selections must be non-empty, unique and use the SKIP action.");

    public static Error StaleMasterDataPlan(short? languageId = null) => Conflict(
        "STALE_MASTER_DATA_PLAN", languageId,
        "Asosiy ma'lumotlar rejasi o'zgargan, uni qayta ko'rib chiqing.",
        "Асосий маълумотлар режаси ўзгарган, уни қайта кўриб чиқинг.",
        "План справочных данных изменился; проверьте его повторно.",
        "The master-data plan changed and must be reviewed again.");

    public static Error StaleProductConflictPlan(short? languageId = null) => Conflict(
        "STALE_PRODUCT_CONFLICT_PLAN", languageId,
        "Mahsulot nizolari rejasi o'zgargan, uni qayta ko'rib chiqing.",
        "Маҳсулот низолари режаси ўзгарган, уни қайта кўриб чиқинг.",
        "План конфликтов товаров провайдера изменился; проверьте его повторно.",
        "The provider product conflict plan changed and must be reviewed again.");

    public static Error StaleMarkingConflictPlan(short? languageId = null) => Conflict(
        "STALE_MARKING_CONFLICT_PLAN", languageId,
        "Markirovka nizolari rejasi o'zgargan, uni qayta ko'rib chiqing.",
        "Маркировка низолари режаси ўзгарган, уни қайта кўриб чиқинг.",
        "План конфликтов маркировки изменился; проверьте его повторно.",
        "The marking conflict plan changed and must be reviewed again.");

    public static Error JobNotMappable(short? languageId = null) => Conflict(
        "EdoImport.JobNotMappable", languageId,
        "Mosliklarni faqat PREFLIGHT_READY yoki PARTIAL holatida o'zgartirish mumkin.",
        "Мосликларни фақат PREFLIGHT_READY ёки PARTIAL ҳолатида ўзгартириш мумкин.",
        "Сопоставления можно менять только для задач в статусе PREFLIGHT_READY или PARTIAL.",
        "Mappings can be changed only for PREFLIGHT_READY or PARTIAL jobs.");

    public static Error CandidateNotMappable(short? languageId = null) => Conflict(
        "EdoImport.CandidateNotMappable", languageId,
        "Faqat MAPPING_REQUIRED yoki READY holatidagi nomzodlarni moslashtirish mumkin.",
        "Фақат MAPPING_REQUIRED ёки READY ҳолатидаги номзодларни мослаштириш мумкин.",
        "Сопоставлять можно только кандидатов в статусе MAPPING_REQUIRED или READY.",
        "Only MAPPING_REQUIRED or READY candidates can be mapped.");

    public static Error InvalidLineCoverage(short? languageId = null) => Business(
        "EdoImport.MappingLineCoverageInvalid", languageId,
        "So'rov nomzodning har bir qatorini aynan bir marta o'z ichiga olishi kerak.",
        "Сўров номзоднинг ҳар бир қаторини айнан бир марта ўз ичига олиши керак.",
        "Запрос сопоставления должен содержать каждую строку кандидата ровно один раз.",
        "The mapping request must contain every candidate line exactly once.");

    public static Error CandidateSourceInvalid(short? languageId = null) => Business(
        "EdoImport.CandidateSourceInvalid", languageId,
        "Nomzodda moslashtirish uchun zarur manba qiymatlari yo'q.",
        "Номзодда мослаштириш учун зарур манба қийматлари йўқ.",
        "В кандидате отсутствуют исходные значения, необходимые для сопоставления.",
        "The candidate does not contain the source values required for mapping.");

    public static Error JobNotActive(short? languageId = null) => Conflict(
        "EdoImport.JobNotActive", languageId,
        "Faqat faol EDO import vazifasini bekor qilish mumkin.",
        "Фақат фаол EDO импорт вазифасини бекор қилиш мумкин.",
        "Отменить можно только активную задачу импорта EDO.",
        "Only an active EDO import job can be cancelled.");

    public static Error BackgroundRecoveryMappingInvalid(short? languageId = null) => Business(
        "BACKGROUND_EDO_SCOPE_RECOVERY_MAPPING_INVALID", languageId,
        "Nomzod fon rejimida tiklash uchun yaroqsiz.",
        "Номзод фон режимида тиклаш учун яроқсиз.",
        "Кандидат недействителен для фонового восстановления импорта черновика.",
        "The candidate is not valid for background Draft import recovery.");

    public static Error DraftMarkingAlreadyUsed(short? languageId = null) => Conflict(
        "DRAFT_IMPORT_MARKING_ALREADY_USED", languageId,
        "Provayder markirovkasi tashkilotdagi mavjud ombor pozitsiyasiga allaqachon bog'langan.",
        "Провайдер маркировкаси ташкилотдаги мавжуд омбор позициясига аллақачон боғланган.",
        "Маркировка провайдера уже связана с существующей складской позицией организации.",
        "A provider marking is already linked to an existing organization inventory item.");

    public static Error ProductPieceTrackingRequired(string code, short? languageId = null) => Business(
        code, languageId,
        "Markirovkalangan tovarlar importdan oldin donabay kuzatuvi yoqilgan mahalliy mahsulotni talab qiladi.",
        "Маркировкаланган товарлар импортдан олдин донабай кузатуви ёқилган маҳаллий маҳсулотни талаб қилади.",
        "Для маркированных товаров до импорта требуется локальный товар с включённым поштучным учётом.",
        "Marked goods require an active piece-tracked local product before Draft import.");

    public static Error ProductMasterDataResolutionRequired(short? languageId = null) => Business(
        "PRODUCT_MASTER_DATA_RESOLUTION_REQUIRED", languageId,
        "Mahsulot yaratish uchun turi, o'lchov birligi, QQS va markirovka bo'yicha aniq, nizosiz tanlov kerak.",
        "Маҳсулот яратиш учун тури, ўлчов бирлиги, ҚҚС ва маркировка бўйича аниқ, низосиз танлов керак.",
        "Для создания товара требуется явный непротиворечивый выбор типа, единицы, НДС и маркировки.",
        "Product creation requires an explicit non-conflicting item type, unit, VAT and marking choice.");

    public static Error MasterDataSelectionInvalid(short? languageId = null) => Business(
        "MASTER_DATA_SELECTION_INVALID", languageId,
        "Har bir tanlangan element aynan bitta joriy asosiy ma'lumotlar rejasi elementiga mos kelishi kerak.",
        "Ҳар бир танланган элемент айнан битта жорий асосий маълумотлар режаси элементига мос келиши керак.",
        "Каждый выбранный элемент должен точно соответствовать одному текущему элементу плана справочных данных.",
        "Every selected item must match one current master-data plan item exactly.");

    private static Error Business(string code, short? languageId, string uz, string uzCyrl, string ru, string en) =>
        Error.Business(code, Message(languageId, uz, uzCyrl, ru, en));

    private static Error Conflict(string code, short? languageId, string uz, string uzCyrl, string ru, string en) =>
        Error.Conflict(code, Message(languageId, uz, uzCyrl, ru, en));

    private static Error Forbidden(string code, short? languageId, string uz, string uzCyrl, string ru, string en) =>
        Error.Forbidden(code, Message(languageId, uz, uzCyrl, ru, en));

    private static Error NotFound(string code, short? languageId, string uz, string uzCyrl, string ru, string en) =>
        Error.NotFound(code, Message(languageId, uz, uzCyrl, ru, en));

    private static string Message(short? languageId, string uz, string uzCyrl, string ru, string en) => languageId switch
    {
        LanguageIdConst.UZ => uz,
        LanguageIdConst.UZ_CYRL => uzCyrl,
        LanguageIdConst.RU => ru,
        _ => en
    };
}
