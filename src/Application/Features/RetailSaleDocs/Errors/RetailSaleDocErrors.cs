using SharedKernel.Results;

namespace Application.Features.RetailSaleDocs;

public static class RetailSaleDocErrors
{
    public static Error NotFound(long id) =>
        Error.NotFound("RetailSaleDoc.NotFound", $"Retail sale document with id {id} was not found.");

    public static Error CannotUpdate(long id, short statusId) =>
        Error.Conflict("RetailSaleDoc.CannotUpdate", $"Retail sale document {id} cannot be changed in status {statusId}.");

    public static Error CannotConfirm(long id, short statusId) =>
        Error.Conflict("RetailSaleDoc.CannotConfirm", $"Retail sale document {id} cannot be confirmed in status {statusId}.");

    public static Error CannotCancel(long id, short statusId) =>
        Error.Conflict("RetailSaleDoc.CannotCancel", $"Retail sale document {id} cannot be cancelled in status {statusId}.");

    public static Error EmptyLines() =>
        Error.Business("RetailSaleDoc.EmptyLines", "Retail sale document must contain at least one product line.");

    public static Error InvalidLine(int productId) =>
        Error.Business("RetailSaleDoc.InvalidLine", $"Retail sale line for product {productId} is invalid.");

    public static Error ProductNotFound(int productId) =>
        Error.NotFound("RetailSaleDoc.ProductNotFound", $"Product {productId} was not found.");

    public static Error InvalidProductTable(int productTableId) =>
        Error.Conflict("RetailSaleDoc.InvalidProductTable", $"Product table {productTableId} does not match the retail sale line or warehouse.");

    public static Error InvalidPayment() =>
        Error.Business("RetailSaleDoc.InvalidPayment", "Retail sale payments are invalid.");

    public static Error PaymentTotalMismatch(decimal expected, decimal actual) =>
        Error.Business("RetailSaleDoc.PaymentTotalMismatch", $"Payment total {actual} must equal document total {expected}.");

    public static Error AccountRequired() =>
        Error.Business("RetailSaleDoc.AccountRequired", "Required accounting accounts are not specified for the retail sale.");

    public static Error MissingPostingBatch(long id) =>
        Error.Conflict("RetailSaleDoc.MissingPostingBatch", $"Posting batch for retail sale document {id} was not found.");

    public static Error MissingAccountingEntries(long id) =>
        Error.Conflict("RetailSaleDoc.MissingAccountingEntries", $"Accounting entries for retail sale document {id} were not found.");

    public static Error BusinessEffectsExist(long id) =>
        Error.Conflict("RetailSaleDoc.BusinessEffectsExist", $"Retail sale document {id} already has business effects.");

    public static Error DocumentRegistryNotFound(long id) =>
        Error.Conflict("RetailSaleDoc.DocumentRegistryNotFound", $"Document registry row for retail sale {id} was not found.");

    public static Error PaymentOperationsAlreadyExist(long id) =>
        Error.Conflict("RetailSaleDoc.PaymentOperationsAlreadyExist", $"Retail sale {id} already has payment acceptance point operations.");

    public static Error MissingPaymentOperationBatch(long operationId) =>
        Error.Conflict("RetailSaleDoc.MissingPaymentOperationBatch", $"Posting batch for payment acceptance point operation {operationId} was not found.");

    public static Error InsufficientPaymentPointBalance(decimal currentBalance, decimal balanceAfterReversal) =>
        Error.Business(
            "RetailSaleDoc.InsufficientPaymentPointBalance",
            $"Payment acceptance point balance {currentBalance} cannot be reversed because the resulting balance would be {balanceAfterReversal}.");
}
