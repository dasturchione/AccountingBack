using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Pay;

public static class PayrollErrors
{
    public static Error NotFound(string entity, long id, short? languageId = null) =>
        Error.NotFound($"Payroll.{entity}.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"{entity} (id: {id}) topilmadi.",
            LanguageIdConst.RU => $"{entity} (id: {id}) ne nayden.",
            _ => $"{entity} with id {id} was not found."
        });

    public static Error Conflict(string code, string message) =>
        Error.Conflict($"Payroll.{code}", message);

    public static Error Business(string code, string message) =>
        Error.Business($"Payroll.{code}", message);

    public static Error PeriodClosed(long periodId) =>
        Business("PeriodClosed", $"Payroll period {periodId} is closed.");

    public static Error InvalidStatus(string entity, long id, short statusId, string operation) =>
        Business("InvalidStatus", $"{entity} {id} cannot be {operation} in status {statusId}.");

    public static Error DuplicateEmployeeNumber(string employeeNumber) =>
        Conflict("EmployeeNumberConflict", $"Employee number '{employeeNumber}' already exists.");

    public static Error DuplicatePinfl(string pinfl) =>
        Conflict("EmployeePinflConflict", $"Employee PINFL '{pinfl}' already exists.");

    public static Error EmploymentOverlap(long employeeId) =>
        Conflict("EmploymentOverlap", $"Employee {employeeId} already has an overlapping employment period.");

    public static Error ReferencedRecordNotFound(string entity, long id) =>
        Business("ReferenceNotFound", $"{entity} {id} was not found in the current organization.");

    public static Error ComponentAlreadyUsed(int componentId) =>
        Conflict("ComponentAlreadyUsed", $"Component {componentId} is already used by a posted payroll document; create a new effective version.");

    public static Error NoPostedTimesheet(long periodId) =>
        Business("NoPostedTimesheet", $"A posted timesheet is required for payroll period {periodId}.");

    public static Error NoActiveEmployment(long employeeId) =>
        Business("NoActiveEmployment", $"Employee {employeeId} has no active employment for the payroll period.");

    public static Error NoCalculationComponents(long periodId) =>
        Business("NoCalculationComponents", $"No active payroll calculation components were found for period {periodId}.");

    public static Error MissingBaseSalaryComponent() =>
        Business("MissingBaseSalaryComponent", "An active mandatory SALARY_PRORATED earning component is required.");

    public static Error RegularPayrollAlreadyExists(long periodId) =>
        Conflict("RegularPayrollAlreadyExists", $"An active regular payroll document already exists for period {periodId}.");

    public static Error CorrectionSourceRequired() =>
        Business("CorrectionSourceRequired", "A posted source payroll document is required for a correction.");

    public static Error DuplicateAdjustment(long employeeId, int componentId) =>
        Business("DuplicateAdjustment", $"Employee {employeeId} has more than one adjustment for component {componentId}.");

    public static Error AccountRoleNotConfigured(string roleCode) =>
        Business("AccountRoleNotConfigured", $"Default chart account for payroll role '{roleCode}' is not configured.");

    public static Error PaymentExceedsOutstanding(long employeeId, decimal amount, decimal outstanding) =>
        Business("PaymentExceedsOutstanding", $"Payment {amount:N2} for employee {employeeId} exceeds outstanding amount {outstanding:N2}.");

    public static Error PaymentSourceInvalid() =>
        Business("PaymentSourceInvalid", "BANK requires bankAccountId; CASH requires cashBoxId.");

    public static Error LinkedOperationMissing(long batchId) =>
        Conflict("LinkedOperationMissing", $"Payment batch {batchId} has no linked bank or cash operation.");
}
