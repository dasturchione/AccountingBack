using SharedKernel.Results;

namespace Application.Features.Cmn.CurrencyRevaluations;

public static class CurrencyRevaluationErrors
{
    public static Error NotFound(long id, short languageId) => Error.NotFound("CurrencyRevaluation.NotFound", $"Currency revaluation with id '{id}' not found.");
    public static Error InvalidDate(short languageId) => Error.Business("CurrencyRevaluation.InvalidDate", "Revaluation date is invalid.");
    public static Error NoOrganization(short languageId) => Error.Business("CurrencyRevaluation.NoOrganization", "Current user has no organization.");
    public static Error AlreadyConfirmed(long id, short languageId) => Error.Conflict("CurrencyRevaluation.AlreadyConfirmed", $"Currency revaluation '{id}' is already confirmed.");
    public static Error AlreadyCancelled(long id, short languageId) => Error.Conflict("CurrencyRevaluation.AlreadyCancelled", $"Currency revaluation '{id}' is already cancelled.");
    public static Error DuplicateForDate(short languageId) => Error.Conflict("CurrencyRevaluation.DuplicateForDate", "Currency revaluation for the specified date already exists.");
    public static Error NoRevaluationLines(short languageId) => Error.Business("CurrencyRevaluation.NoLines", "Currency revaluation produced no eligible lines.");
    public static Error MissingPostingBatch(long id, short languageId) => Error.Conflict("CurrencyRevaluation.MissingPostingBatch", $"Posting batch was not found for currency revaluation '{id}'.");
    public static Error CannotConfirmInCurrentStatus(long id, short statusId, short languageId) => Error.Conflict("CurrencyRevaluation.CannotConfirmInCurrentStatus", $"Currency revaluation '{id}' cannot be confirmed in status '{statusId}'.");
    public static Error CannotCancelInCurrentStatus(long id, short statusId, short languageId) => Error.Conflict("CurrencyRevaluation.CannotCancelInCurrentStatus", $"Currency revaluation '{id}' cannot be cancelled in status '{statusId}'.");
    public static Error BusinessEffectsAlreadyExist(long id, short languageId) => Error.Conflict("CurrencyRevaluation.BusinessEffectsAlreadyExist", $"Currency revaluation '{id}' already has accounting effects.");
    public static Error MissingAccountingRegisterEntries(long id, short languageId) => Error.Conflict("CurrencyRevaluation.MissingAccountingRegisterEntries", $"Accounting register entries were not found for currency revaluation '{id}'.");
    public static Error NoAccountingEntries(long id, short languageId) => Error.Conflict("CurrencyRevaluation.NoAccountingEntries", $"Currency revaluation '{id}' has no accounting entries.");
}
