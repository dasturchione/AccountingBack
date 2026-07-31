using SharedKernel.Results;

namespace Application.Features.Pay;

public static class PayrollErrors
{
    public static Error NotFound(string entity, long id, short? languageId = null) =>
        Error.NotFound(
            $"Payroll.{entity}.NotFound",
            $"{GetEntityName(entity)} topilmadi (ID: {id}).");

    public static Error Conflict(string code, string message) =>
        Error.Conflict($"Payroll.{code}", message);

    public static Error Business(string code, string message) =>
        Error.Business($"Payroll.{code}", message);

    public static Error PeriodClosed(long periodId) =>
        Business("PeriodClosed", $"Oylik hisoblash davri yopilgan (davr ID: {periodId}).");

    public static Error InvalidStatus(string entity, long id, short statusId, string operation) =>
        Business(
            "InvalidStatus",
            $"{GetEntityName(entity)}ni {GetOperationName(operation)} mumkin emas: joriy holat ID = {statusId} (ID: {id}).");

    public static Error DuplicateEmployeeNumber(string employeeNumber) =>
        Conflict("EmployeeNumberConflict", $"'{employeeNumber}' xodim raqami allaqachon mavjud.");

    public static Error DuplicatePinfl(string pinfl) =>
        Conflict("EmployeePinflConflict", $"'{pinfl}' JSHSHIR bilan xodim allaqachon mavjud.");

    public static Error EmploymentOverlap(long employeeId) =>
        Conflict("EmploymentOverlap", $"Tanlangan davrda xodimning boshqa ishga qabul yozuvi mavjud (xodim ID: {employeeId}).");

    public static Error ReferencedRecordNotFound(string entity, long id) =>
        Business("ReferenceNotFound", $"{GetEntityName(entity)} joriy tashkilotda topilmadi (ID: {id}).");

    public static Error ComponentAlreadyUsed(int componentId) =>
        Conflict("ComponentAlreadyUsed", $"Hisoblash komponenti tasdiqlangan oylik hujjatida ishlatilgan. Yangi sanadan boshlanadigan versiya yarating (komponent ID: {componentId}).");

    public static Error NoPostedTimesheet(long periodId) =>
        Business("NoPostedTimesheet", $"Oylikni hisoblash uchun tasdiqlangan tabel kerak (davr ID: {periodId}).");

    public static Error NoActiveEmployment(long employeeId) =>
        Business("NoActiveEmployment", $"Ushbu oylik davrida xodimning amaldagi ishga qabul yozuvi mavjud emas (xodim ID: {employeeId}).");

    public static Error NoCalculationComponents(long periodId) =>
        Business("NoCalculationComponents", $"Ushbu davr uchun faol oylik hisoblash komponentlari topilmadi (davr ID: {periodId}).");

    public static Error MissingBaseSalaryComponent() =>
        Business("MissingBaseSalaryComponent", "Majburiy va faol SALARY_PRORATED ish haqi komponenti sozlanishi kerak.");

    public static Error RegularPayrollAlreadyExists(long periodId) =>
        Conflict("RegularPayrollAlreadyExists", $"Ushbu davr uchun faol odatiy oylik hisoblash hujjati allaqachon mavjud (davr ID: {periodId}).");

    public static Error CorrectionSourceRequired() =>
        Business("CorrectionSourceRequired", "Tuzatish kiritish uchun asos bo‘ladigan tasdiqlangan oylik hisoblash hujjati kerak.");

    public static Error DuplicateAdjustment(long employeeId, int componentId) =>
        Business("DuplicateAdjustment", $"Xodim uchun bitta hisoblash komponenti bo‘yicha bir nechta tuzatish kiritilgan (xodim ID: {employeeId}, komponent ID: {componentId}).");

    public static Error AccountRoleNotConfigured(string roleCode) =>
        Business("AccountRoleNotConfigured", $"Oylik hisoblashdagi '{roleCode}' roli uchun standart hisobvaraq sozlanmagan.");

    public static Error PaymentExceedsOutstanding(long employeeId, decimal amount, decimal outstanding) =>
        Business("PaymentExceedsOutstanding", $"To‘lov summasi ({amount:N2}) xodimga to‘lanishi kerak bo‘lgan qoldiqdan ({outstanding:N2}) oshib ketdi (xodim ID: {employeeId}).");

    public static Error PaymentSourceInvalid() =>
        Business("PaymentSourceInvalid", "Bank orqali to‘lov uchun bank hisob raqami, naqd to‘lov uchun esa kassa tanlanishi kerak.");

    public static Error LinkedOperationMissing(long batchId) =>
        Conflict("LinkedOperationMissing", $"To‘lov hujjatiga bog‘langan bank yoki kassa operatsiyasi topilmadi (hujjat ID: {batchId}).");

    private static string GetEntityName(string entity) =>
        entity switch
        {
            "Timesheet" => "Tabel",
            "Period" => "Oylik hisoblash davri",
            "PayrollDocument" => "Oylik hisoblash hujjati",
            "PaymentBatch" => "To‘lov hujjati",
            "Employee" => "Xodim",
            "Employment" => "Ishga qabul yozuvi",
            "EmployeeComponent" => "Xodimning hisoblash komponenti",
            "Component" => "Hisoblash komponenti",
            "Department" => "Bo‘lim",
            "Position" => "Lavozim",
            "Currency" => "Valyuta",
            "ChartAccount" => "Hisobvaraq",
            "BankAccount" => "Bank hisob raqami",
            "CashBox" => "Kassa",
            "PayrollLine" => "Oylik hisoblash qatori",
            _ => "Ma’lumot"
        };

    private static string GetOperationName(string operation) =>
        operation switch
        {
            "updated" => "o‘zgartirish",
            "confirmed" => "tasdiqlash",
            "deleted" => "o‘chirish",
            _ => "ushbu amalni bajarish"
        };
}
