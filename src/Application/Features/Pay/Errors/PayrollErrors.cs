using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Pay;

public static class PayrollErrors
{
    public static Error NotFound(string entity, long id, short? languageId = null) =>
        Error.NotFound(
            $"Payroll.{entity}.NotFound",
            languageId switch
            {
                LanguageIdConst.UZ => $"{GetEntityName(entity, LanguageIdConst.UZ)} topilmadi (ID: {id}).",
                LanguageIdConst.UZ_CYRL => $"{GetEntityName(entity, LanguageIdConst.UZ_CYRL)} топилмади (ID: {id}).",
                LanguageIdConst.RU => $"{GetEntityName(entity, LanguageIdConst.RU)} не найден (ID: {id}).",
                _ => $"{GetEntityName(entity, null)} was not found (ID: {id})."
            });

    public static Error Conflict(string code, string uzMessage, short? languageId = null) =>
        Error.Conflict($"Payroll.{code}", KnownMessage(code, uzMessage, languageId));

    public static Error Business(string code, string uzMessage, short? languageId = null) =>
        Error.Business($"Payroll.{code}", KnownMessage(code, uzMessage, languageId));

    public static Error PeriodClosed(long periodId, short? languageId = null) =>
        B("PeriodClosed", languageId, $"Oylik hisoblash davri yopilgan (davr ID: {periodId}).", $"Ойлик ҳисоблаш даври ёпилган (давр ID: {periodId}).", $"Период расчёта зарплаты закрыт (ID: {periodId}).", $"Payroll period is closed (ID: {periodId}).");

    public static Error InvalidStatus(string entity, long id, short statusId, string operation, short? languageId = null) =>
        B("InvalidStatus", languageId,
            $"{GetEntityName(entity, LanguageIdConst.UZ)}ni {GetOperationName(operation, LanguageIdConst.UZ)} mumkin emas: joriy holat ID = {statusId} (ID: {id}).",
            $"{GetEntityName(entity, LanguageIdConst.UZ_CYRL)}ни {GetOperationName(operation, LanguageIdConst.UZ_CYRL)} мумкин эмас: жорий ҳолат ID = {statusId} (ID: {id}).",
            $"Нельзя {GetOperationName(operation, LanguageIdConst.RU)} {GetEntityName(entity, LanguageIdConst.RU).ToLowerInvariant()}: текущий статус {statusId} (ID: {id}).",
            $"Cannot {GetOperationName(operation, null)} {GetEntityName(entity, null).ToLowerInvariant()}: current status {statusId} (ID: {id}).");

    public static Error DuplicateEmployeeNumber(string employeeNumber, short? languageId = null) => C("EmployeeNumberConflict", languageId,
        $"'{employeeNumber}' xodim raqami allaqachon mavjud.", $"'{employeeNumber}' ходим рақами аллақачон мавжуд.",
        $"Табельный номер '{employeeNumber}' уже существует.", $"Employee number '{employeeNumber}' already exists.");

    public static Error DuplicatePinfl(string pinfl, short? languageId = null) => C("EmployeePinflConflict", languageId,
        $"'{pinfl}' JSHSHIR bilan xodim allaqachon mavjud.", $"'{pinfl}' ЖШШИР билан ходим аллақачон мавжуд.",
        $"Сотрудник с ПИНФЛ '{pinfl}' уже существует.", $"An employee with PINFL '{pinfl}' already exists.");

    public static Error EmploymentOverlap(long employeeId, short? languageId = null) => C("EmploymentOverlap", languageId,
        $"Tanlangan davrda xodimning boshqa ishga qabul yozuvi mavjud (xodim ID: {employeeId}).", $"Танланган даврда ходимнинг бошқа ишга қабул ёзуви мавжуд (ходим ID: {employeeId}).",
        $"В выбранном периоде уже существует другая запись трудоустройства сотрудника {employeeId}.", $"Another employment record exists for employee {employeeId} in the selected period.");

    public static Error ReferencedRecordNotFound(string entity, long id, short? languageId = null) => B("ReferenceNotFound", languageId,
        $"{GetEntityName(entity, LanguageIdConst.UZ)} joriy tashkilotda topilmadi (ID: {id}).", $"{GetEntityName(entity, LanguageIdConst.UZ_CYRL)} жорий ташкилотда топилмади (ID: {id}).",
        $"{GetEntityName(entity, LanguageIdConst.RU)} не найден в текущей организации (ID: {id}).", $"{GetEntityName(entity, null)} was not found in the current organization (ID: {id}).");

    public static Error ComponentAlreadyUsed(int componentId, short? languageId = null) => C("ComponentAlreadyUsed", languageId,
        $"Komponent tasdiqlangan oylik hujjatida ishlatilgan. Yangi versiya yarating (ID: {componentId}).", $"Компонент тасдиқланган ойлик ҳужжатида ишлатилган. Янги версия яратинг (ID: {componentId}).",
        $"Компонент использован в подтверждённом документе зарплаты. Создайте новую версию (ID: {componentId}).", $"Component is used in a confirmed payroll document. Create a new version (ID: {componentId}).");

    public static Error NoPostedTimesheet(long periodId, short? languageId = null) => B("NoPostedTimesheet", languageId, $"Oylikni hisoblash uchun tasdiqlangan tabel kerak (davr ID: {periodId}).", $"Ойликни ҳисоблаш учун тасдиқланган табель керак (давр ID: {periodId}).", $"Для расчёта зарплаты требуется подтверждённый табель (период ID: {periodId}).", $"A confirmed timesheet is required for payroll calculation (period ID: {periodId}).");

    public static Error NoActiveEmployment(long employeeId, short? languageId = null) => B("NoActiveEmployment", languageId, $"Ushbu davrda xodimning faol ishga qabul yozuvi yo'q (ID: {employeeId}).", $"Ушбу даврда ходимнинг фаол ишга қабул ёзуви йўқ (ID: {employeeId}).", $"В этом периоде нет активной записи трудоустройства сотрудника {employeeId}.", $"Employee {employeeId} has no active employment record in this payroll period.");

    public static Error NoCalculationComponents(long periodId, short? languageId = null) => B("NoCalculationComponents", languageId, $"Ushbu davr uchun faol hisoblash komponentlari topilmadi (ID: {periodId}).", $"Ушбу давр учун фаол ҳисоблаш компонентлари топилмади (ID: {periodId}).", $"Для периода {periodId} не найдены активные компоненты расчёта зарплаты.", $"No active payroll calculation components were found for period {periodId}.");

    public static Error MissingBaseSalaryComponent(short? languageId = null) => B("MissingBaseSalaryComponent", languageId, "Majburiy va faol SALARY_PRORATED ish haqi komponenti sozlanishi kerak.", "Мажбурий ва фаол SALARY_PRORATED иш ҳақи компоненти созланиши керак.", "Необходимо настроить обязательный активный компонент зарплаты SALARY_PRORATED.", "A mandatory active SALARY_PRORATED component must be configured.");

    public static Error RegularPayrollAlreadyExists(long periodId, short? languageId = null) => C("RegularPayrollAlreadyExists", languageId, $"Ushbu davr uchun odatiy oylik hujjati mavjud (ID: {periodId}).", $"Ушбу давр учун одатий ойлик ҳужжати мавжуд (ID: {periodId}).", $"Для периода {periodId} уже существует активный обычный документ зарплаты.", $"An active regular payroll document already exists for period {periodId}.");

    public static Error CorrectionSourceRequired(short? languageId = null) => B("CorrectionSourceRequired", languageId, "Tuzatish uchun tasdiqlangan oylik hujjati kerak.", "Тузатиш учун тасдиқланган ойлик ҳужжати керак.", "Для корректировки требуется исходный подтверждённый документ зарплаты.", "A confirmed source payroll document is required for a correction.");

    public static Error RecalculationRequiresPosted(long id, short statusId, short? languageId = null) => B(
        "RecalculationRequiresPosted",
        languageId,
        $"Qayta hisoblash faqat tasdiqlangan oylik hujjati uchun mumkin (hujjat ID: {id}, holat: {statusId}).",
        $"Қайта ҳисоблаш фақат тасдиқланган ойлик ҳужжати учун мумкин (ҳужжат ID: {id}, ҳолат: {statusId}).",
        $"Перерасчёт доступен только для проведённого документа зарплаты (ID: {id}, статус: {statusId}).",
        $"Recalculation is available only for a posted payroll document (ID: {id}, status: {statusId}).");

    public static Error RecalculationBlocksPeriodClose(long periodId, short? languageId = null) => B(
        "RecalculationBlocksPeriodClose",
        languageId,
        $"Oylik davrini yopishdan oldin qayta hisoblash navbatidagi xatoni tuzating yoki jarayon tugashini kuting (davr ID: {periodId}).",
        $"Ойлик даврини ёпишдан олдин қайта ҳисоблаш навбатидаги хатони тузатинг ёки жараён тугашини кутинг (давр ID: {periodId}).",
        $"Перед закрытием периода исправьте ошибку перерасчёта или дождитесь завершения процесса (период ID: {periodId}).",
        $"Resolve the payroll recalculation error or wait for it to finish before closing period {periodId}.");

    public static Error DuplicateAdjustment(long employeeId, int componentId, short? languageId = null) => B("DuplicateAdjustment", languageId, $"Xodim uchun komponent bo'yicha bir nechta tuzatish kiritilgan (xodim: {employeeId}, komponent: {componentId}).", $"Ходим учун компонент бўйича бир нечта тузатиш киритилган (ходим: {employeeId}, компонент: {componentId}).", $"Для сотрудника {employeeId} указано несколько корректировок компонента {componentId}.", $"Multiple adjustments for component {componentId} were supplied for employee {employeeId}.");

    public static Error ReclassificationAccountsRequired(int componentId, short? languageId = null) => B(
        "ReclassificationAccountsRequired",
        languageId,
        $"Qayta tasniflash komponenti uchun debet va kredit hisobvaraqlari ko‘rsatilishi kerak (ID: {componentId}).",
        $"Қайта таснифлаш компоненти учун дебет ва кредит ҳисобварақлари кўрсатилиши керак (ID: {componentId}).",
        $"Для компонента переклассификации необходимо указать дебетовый и кредитовый счета (ID: {componentId}).",
        $"Debit and credit accounts are required for reclassification component {componentId}.");

    public static Error StoredPostingAccountMissing(string accountName, long sourceLineId, short? languageId = null) => B(
        "StoredPostingAccountMissing",
        languageId,
        $"Oylik provodkasi uchun saqlangan '{accountName}' hisobvarag‘i mavjud emas (qator ID: {sourceLineId}).",
        $"Ойлик проводкаси учун сақланган '{accountName}' ҳисобварағи мавжуд эмас (қатор ID: {sourceLineId}).",
        $"Для проводки зарплаты отсутствует сохранённый счёт '{accountName}' (строка ID: {sourceLineId}).",
        $"Stored payroll posting account '{accountName}' is missing (line ID: {sourceLineId}).");

    public static Error PaymentExceedsOutstanding(long employeeId, decimal amount, decimal outstanding, short? languageId = null) => B("PaymentExceedsOutstanding", languageId, $"To'lov {amount:N2} xodim qoldig'i {outstanding:N2} dan katta (ID: {employeeId}).", $"Тўлов {amount:N2} ходим қолдиғи {outstanding:N2} дан катта (ID: {employeeId}).", $"Платёж {amount:N2} превышает задолженность {outstanding:N2} сотруднику {employeeId}.", $"Payment {amount:N2} exceeds outstanding {outstanding:N2} for employee {employeeId}.");

    public static Error PaymentSourceInvalid(short? languageId = null) => B("PaymentSourceInvalid", languageId, "Bank to'lovi uchun bank hisobvarag'i, naqd to'lov uchun kassa tanlanishi kerak.", "Банк тўлови учун банк ҳисобварағи, нақд тўлов учун касса танланиши керак.", "Для банковской выплаты выберите банковский счёт, для наличной — кассу.", "Select a bank account for bank payment or a cash box for cash payment.");

    public static Error LinkedOperationMissing(long batchId, short? languageId = null) => C("LinkedOperationMissing", languageId, $"To'lov hujjatiga bog'langan bank yoki kassa operatsiyasi topilmadi (ID: {batchId}).", $"Тўлов ҳужжатига боғланган банк ёки касса операцияси топилмади (ID: {batchId}).", $"Не найдена банковская или кассовая операция, связанная с платёжным документом {batchId}.", $"Bank or cash operation linked to payment document {batchId} was not found.");

    private static string GetEntityName(string entity, short? languageId) =>
        (entity, languageId) switch
        {
            ("Timesheet", LanguageIdConst.UZ) => "Tabel", ("Timesheet", LanguageIdConst.UZ_CYRL) => "Табель", ("Timesheet", LanguageIdConst.RU) => "Табель", ("Timesheet", _) => "Timesheet",
            ("Period", LanguageIdConst.UZ) => "Oylik davri", ("Period", LanguageIdConst.UZ_CYRL) => "Ойлик даври", ("Period", LanguageIdConst.RU) => "Период зарплаты", ("Period", _) => "Payroll period",
            ("PayrollDocument", LanguageIdConst.UZ) => "Oylik hujjati", ("PayrollDocument", LanguageIdConst.UZ_CYRL) => "Ойлик ҳужжати", ("PayrollDocument", LanguageIdConst.RU) => "Документ зарплаты", ("PayrollDocument", _) => "Payroll document",
            ("PaymentBatch", LanguageIdConst.UZ) => "To'lov hujjati", ("PaymentBatch", LanguageIdConst.UZ_CYRL) => "Тўлов ҳужжати", ("PaymentBatch", LanguageIdConst.RU) => "Платёжный документ", ("PaymentBatch", _) => "Payment document",
            ("Employee", LanguageIdConst.UZ) => "Xodim", ("Employee", LanguageIdConst.UZ_CYRL) => "Ходим", ("Employee", LanguageIdConst.RU) => "Сотрудник", ("Employee", _) => "Employee",
            ("Employment", LanguageIdConst.UZ) => "Ishga qabul yozuvi", ("Employment", LanguageIdConst.UZ_CYRL) => "Ишга қабул ёзуви", ("Employment", LanguageIdConst.RU) => "Запись трудоустройства", ("Employment", _) => "Employment record",
            ("EmployeeComponent", LanguageIdConst.UZ) => "Xodim komponenti", ("EmployeeComponent", LanguageIdConst.UZ_CYRL) => "Ходим компоненти", ("EmployeeComponent", LanguageIdConst.RU) => "Компонент сотрудника", ("EmployeeComponent", _) => "Employee component",
            ("Component", LanguageIdConst.UZ) => "Hisoblash komponenti", ("Component", LanguageIdConst.UZ_CYRL) => "Ҳисоблаш компоненти", ("Component", LanguageIdConst.RU) => "Компонент расчёта", ("Component", _) => "Payroll component",
            ("Department", LanguageIdConst.UZ) => "Bo'lim", ("Department", LanguageIdConst.UZ_CYRL) => "Бўлим", ("Department", LanguageIdConst.RU) => "Подразделение", ("Department", _) => "Department",
            ("Position", LanguageIdConst.UZ) => "Lavozim", ("Position", LanguageIdConst.UZ_CYRL) => "Лавозим", ("Position", LanguageIdConst.RU) => "Должность", ("Position", _) => "Position",
            ("Currency", LanguageIdConst.UZ) => "Valyuta", ("Currency", LanguageIdConst.UZ_CYRL) => "Валюта", ("Currency", LanguageIdConst.RU) => "Валюта", ("Currency", _) => "Currency",
            ("ChartAccount", LanguageIdConst.UZ) => "Hisobvaraq", ("ChartAccount", LanguageIdConst.UZ_CYRL) => "Ҳисобварақ", ("ChartAccount", LanguageIdConst.RU) => "Бухгалтерский счёт", ("ChartAccount", _) => "Chart account",
            ("BankAccount", LanguageIdConst.UZ) => "Bank hisobvarag'i", ("BankAccount", LanguageIdConst.UZ_CYRL) => "Банк ҳисобварағи", ("BankAccount", LanguageIdConst.RU) => "Банковский счёт", ("BankAccount", _) => "Bank account",
            ("CashBox", LanguageIdConst.UZ) => "Kassa", ("CashBox", LanguageIdConst.UZ_CYRL) => "Касса", ("CashBox", LanguageIdConst.RU) => "Касса", ("CashBox", _) => "Cash box",
            ("PayrollLine", LanguageIdConst.UZ) => "Oylik qatori", ("PayrollLine", LanguageIdConst.UZ_CYRL) => "Ойлик қатори", ("PayrollLine", LanguageIdConst.RU) => "Строка зарплаты", ("PayrollLine", _) => "Payroll line",
            (_, LanguageIdConst.UZ) => "Ma'lumot", (_, LanguageIdConst.UZ_CYRL) => "Маълумот", (_, LanguageIdConst.RU) => "Запись", _ => "Record"
        };

    private static string GetOperationName(string operation, short? languageId) =>
        (operation, languageId) switch
        {
            ("updated", LanguageIdConst.UZ) => "o'zgartirish", ("updated", LanguageIdConst.UZ_CYRL) => "ўзгартириш", ("updated", LanguageIdConst.RU) => "изменить", ("updated", _) => "update",
            ("confirmed", LanguageIdConst.UZ) => "tasdiqlash", ("confirmed", LanguageIdConst.UZ_CYRL) => "тасдиқлаш", ("confirmed", LanguageIdConst.RU) => "подтвердить", ("confirmed", _) => "confirm",
            ("deleted", LanguageIdConst.UZ) => "o'chirish", ("deleted", LanguageIdConst.UZ_CYRL) => "ўчириш", ("deleted", LanguageIdConst.RU) => "удалить", ("deleted", _) => "delete",
            (_, LanguageIdConst.UZ) => "amalni bajarish", (_, LanguageIdConst.UZ_CYRL) => "амални бажариш", (_, LanguageIdConst.RU) => "выполнить операцию", _ => "perform operation"
        };

    private static string KnownMessage(string code, string uzMessage, short? languageId)
    {
        if (languageId == LanguageIdConst.UZ)
            return uzMessage;

        var translations = code switch
        {
            "ComponentConflict" => ("Ҳисоблаш компоненти ушбу сана учун аллақачон мавжуд.", "Компонент расчёта уже существует на указанную дату.", "A payroll component already exists for the specified date."),
            "DuplicatePaymentEmployee" => ("Ходим тўлов ҳужжатида такрорланган.", "Сотрудник повторяется в платёжном документе.", "Employee is duplicated in the payment document."),
            "PayrollDocumentRequired" => ("Якуний тўлов учун тасдиқланган ойлик ҳужжати керак.", "Для окончательной выплаты требуется подтверждённый документ зарплаты.", "A confirmed payroll document is required for final payment."),
            "EmploymentLocked" => ("Ишга қабул ёзуви тасдиқланган ойлик ҳужжатида ишлатилган.", "Запись трудоустройства используется в подтверждённом документе зарплаты.", "Employment record is used by a confirmed payroll document."),
            "EmployeeComponentOverlap" => ("Танланган даврда ходим компоненти аллақачон мавжуд.", "Компонент сотрудника уже существует в выбранном периоде.", "Employee component already exists in the selected period."),
            "TimesheetConflict" => ("Ушбу давр учун фаол табель аллақачон мавжуд.", "Для этого периода уже существует активный табель.", "An active timesheet already exists for this period."),
            "EmptyTimesheet" => ("Табельда камида битта ходим бўлиши керак.", "Табель должен содержать хотя бы одного сотрудника.", "Timesheet must contain at least one employee."),
            "TimesheetUsedByPayroll" => ("Табель фаол ойлик ҳужжатида ишлатилган.", "Табель используется активным документом зарплаты.", "Timesheet is used by an active payroll document."),
            "DuplicateTimesheetEmployee" => ("Ходим табельда такрорланган.", "Сотрудник повторяется в табеле.", "Employee is duplicated in the timesheet."),
            "PeriodConflict" => ("Ойлик даври аллақачон мавжуд.", "Период зарплаты уже существует.", "Payroll period already exists."),
            "PeriodHasDraftDocuments" => ("Ойлик даврида якунланмаган ҳужжатлар мавжуд.", "В периоде зарплаты есть незавершённые документы.", "Payroll period contains unfinished documents."),
            "EmptyCorrection" => ("Тузатиш ҳужжатида камида битта тузатиш бўлиши керак.", "Документ корректировки должен содержать хотя бы одну корректировку.", "Correction document must contain at least one adjustment."),
            "NegativeRegularAdjustment" => ("Манфий сумма фақат тузатиш ҳужжатида киритилади.", "Отрицательная сумма разрешена только в документе корректировки.", "A negative amount is allowed only in a correction document."),
            "EmployeeMissingFromTimesheet" => ("Ходим тасдиқланган табелда йўқ.", "Сотрудник отсутствует в подтверждённом табеле.", "Employee is missing from the confirmed timesheet."),
            "NoPayrollLines" => ("Ойлик ҳисоблашда қаторлар ҳосил бўлмади.", "Расчёт зарплаты не сформировал строк.", "Payroll calculation produced no lines."),
            "MissingPostingBatch" => ("Ойлик ҳужжати учун ўтказмалар пакети топилмади.", "Для документа зарплаты не найден пакет проводок.", "Posting batch for payroll document was not found."),
            "EmptyPayroll" => ("Ойлик ҳужжатида ҳисоб-китоб қаторлари йўқ.", "Документ зарплаты не содержит строк расчёта.", "Payroll document has no calculation lines."),
            "BusinessEffectsExist" => ("Ойлик ҳужжати бўйича проводкалар аллақачон яратилган.", "Проводки по документу зарплаты уже созданы.", "Accounting entries already exist for the payroll document."),
            "PayrollHasPayments" => ("Аввал ойлик ҳужжати тўловларини бекор қилинг.", "Сначала отмените выплаты по документу зарплаты.", "Cancel payroll payments before cancelling the payroll document."),
            _ => ($"Ойлик операциясини бажариб бўлмади ({code}).", $"Не удалось выполнить операцию зарплаты ({code}).", $"Payroll operation failed ({code}).")
        };

        return languageId switch
        {
            LanguageIdConst.UZ_CYRL => translations.Item1,
            LanguageIdConst.RU => translations.Item2,
            _ => translations.Item3
        };
    }

    private static Error B(string code, short? languageId, string uz, string uzCyrl, string ru, string en) =>
        Error.Business($"Payroll.{code}", Message(languageId, uz, uzCyrl, ru, en));
    private static Error C(string code, short? languageId, string uz, string uzCyrl, string ru, string en) =>
        Error.Conflict($"Payroll.{code}", Message(languageId, uz, uzCyrl, ru, en));
    private static string Message(short? languageId, string uz, string uzCyrl, string ru, string en) => languageId switch
    {
        LanguageIdConst.UZ => uz, LanguageIdConst.UZ_CYRL => uzCyrl, LanguageIdConst.RU => ru, _ => en
    };
}
