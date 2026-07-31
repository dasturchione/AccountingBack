using SharedKernel.Results;

namespace Application.Features.Inv.OpeningInventories;

public static class OpeningInventoryErrors
{
    public static Error NotFound(long id) =>
        Error.NotFound("OpeningInventory.NotFound", $"Opening inventory document '{id}' was not found.");

    public static Error ReferenceNotFound(string reference, long id) =>
        Error.NotFound($"OpeningInventory.{reference}NotFound", $"{reference} '{id}' was not found.");

    public static Error ChartAccountNotFound(int id) =>
        Error.NotFound("OpeningInventory.ChartAccountNotFound", $"Active non-group chart account '{id}' was not found.");

    public static Error OpeningBalanceNotFound(int organizationId) =>
        Error.Business("OpeningInventory.OpeningBalanceNotFound",
            $"An active opening balance must exist for organization '{organizationId}'.");

    public static Error BaseCurrencyNotConfigured(int organizationId) =>
        Error.Business("OpeningInventory.BaseCurrencyNotConfigured",
            $"A valid base currency must be configured for organization '{organizationId}'.");

    public static Error DuplicateProduct(int productId) =>
        Error.Business("OpeningInventory.DuplicateProduct", $"Product '{productId}' occurs more than once.");

    public static Error LinesRequired() =>
        Error.Business("OpeningInventory.LinesRequired", "At least one product line is required.");

    public static Error InvalidQuantity(int productId, decimal quantity) =>
        Error.Business("OpeningInventory.InvalidQuantity",
            $"Product '{productId}' quantity must be greater than zero; supplied value is {quantity}.");

    public static Error InvalidUnitPrice(int productId, decimal unitPrice) =>
        Error.Business("OpeningInventory.InvalidUnitPrice",
            $"Product '{productId}' unit price cannot be negative; supplied value is {unitPrice}.");

    public static Error InvalidAmount(int productId, decimal expected, decimal actual) =>
        Error.Business("OpeningInventory.InvalidAmount",
            $"Product '{productId}' amount must equal quantity × unit price ({expected}); supplied value is {actual}.");

    public static Error InvalidTotal(decimal expected, decimal actual) =>
        Error.Business("OpeningInventory.InvalidTotal",
            $"Document total must equal the sum of line amounts ({expected}); supplied value is {actual}.");

    public static Error ItemsNotAllowed(int productId) =>
        Error.Business("OpeningInventory.ItemsNotAllowed",
            $"Items are not allowed for non-piece-tracked product '{productId}'.");

    public static Error ItemsRequired(int productId) =>
        Error.Business("OpeningInventory.ItemsRequired",
            $"Items are required for piece-tracked product '{productId}'.");

    public static Error ItemsQuantityMismatch(int productId, decimal quantity, int count) =>
        Error.Business("OpeningInventory.ItemsQuantityMismatch",
            $"Product '{productId}' quantity ({quantity}) must equal its item count ({count}).");

    public static Error DuplicateMarkingNumber(string number) =>
        Error.Business("OpeningInventory.DuplicateMarkingNumber",
            $"Marking number '{number}' occurs more than once.");

    public static Error MarkingNumberRequired(int productId) =>
        Error.Business("OpeningInventory.MarkingNumberRequired",
            $"A marking number is required for every item of product '{productId}'.");

    public static Error EffectsAlreadyUsed(long id) =>
        Error.Conflict("OpeningInventory.EffectsAlreadyUsed",
            $"Opening inventory document '{id}' cannot be changed because its stock or tracked items are already used by another document.");

    public static Error SubkontoValueUnavailable(int accountId, short subkontoTypeId) =>
        Error.Business("OpeningInventory.SubkontoValueUnavailable",
            $"Configured subkonto type '{subkontoTypeId}' for account '{accountId}' cannot be derived from an opening inventory line.");
}
