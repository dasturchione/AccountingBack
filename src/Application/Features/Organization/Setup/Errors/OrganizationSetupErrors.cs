using SharedKernel.Results;

namespace Application.Features.OrganizationSetup;

public static class OrganizationSetupErrors
{
    public static Error OrganizationContextRequired() =>
        Error.Business("OrganizationSetup.OrganizationContextRequired", "Organization context is required. Send X-OrganizationId header or use a token with default organization.");

    public static Error Forbidden() =>
        Error.Forbidden("OrganizationSetup.Forbidden", "You do not have access to manage this organization setup.");

    public static Error OrganizationNotFound(int id) =>
        Error.NotFound("OrganizationSetup.OrganizationNotFound", $"Organization with id {id} was not found.");

    public static Error InnConflict(string inn) =>
        Error.Conflict("OrganizationSetup.InnConflict", $"Organization with INN '{inn}' already exists.");

    public static Error TaxTypeNotFound(short id) =>
        Error.NotFound("OrganizationSetup.TaxTypeNotFound", $"Tax type with id {id} was not found.");

    public static Error AccountingPolicyNotFound(short id) =>
        Error.NotFound("OrganizationSetup.AccountingPolicyNotFound", $"Accounting policy with id {id} was not found.");

    public static Error CurrencyNotFound(short id) =>
        Error.NotFound("OrganizationSetup.CurrencyNotFound", $"Currency with id {id} was not found.");

    public static Error InvalidInventoryValuationMethod(string method) =>
        Error.Business("OrganizationSetup.InvalidInventoryValuationMethod", $"Inventory valuation method '{method}' is not supported.");

    public static Error BranchNotFound(int id) =>
        Error.NotFound("OrganizationSetup.BranchNotFound", $"Branch with id {id} was not found in current organization.");

    public static Error WarehouseNotFound(int id) =>
        Error.NotFound("OrganizationSetup.WarehouseNotFound", $"Warehouse with id {id} was not found in current organization.");

    public static Error CashBoxNotFound(int id) =>
        Error.NotFound("OrganizationSetup.CashBoxNotFound", $"Cash box with id {id} was not found in current organization.");

    public static Error BankAccountNotFound(int id) =>
        Error.NotFound("OrganizationSetup.BankAccountNotFound", $"Bank account with id {id} was not found in current organization.");

    public static Error ChartAccountNotFound(int id) =>
        Error.NotFound("OrganizationSetup.ChartAccountNotFound", $"Chart account with id {id} was not found.");

    public static Error SetupNotReady(string missingStep) =>
        Error.Business("OrganizationSetup.NotReady", $"Setup cannot be completed. Missing step: {missingStep}.");
}
