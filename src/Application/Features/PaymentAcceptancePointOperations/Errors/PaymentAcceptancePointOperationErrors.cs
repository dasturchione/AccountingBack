using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.PaymentAcceptancePointOperations;

public static class PaymentAcceptancePointOperationErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("PaymentAcceptancePointOperation.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan to'lov qabul qilish nuqtasi operatsiyasi topilmadi.",
            LanguageIdConst.RU => $"Операция точки приёма платежей с id {id} не найдена.",
            _ => $"Payment acceptance point operation with id {id} was not found."
        });

    public static Error PointNotFound(int id, short? languageId = null) =>
        Error.NotFound("PaymentAcceptancePointOperation.PointNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan faol to'lov qabul qilish nuqtasi topilmadi.",
            LanguageIdConst.RU => $"Активная точка приёма платежей с id {id} не найдена.",
            _ => $"Active payment acceptance point with id {id} was not found."
        });

    public static Error CurrencyNotFound(short id, short? languageId = null) =>
        Error.NotFound("PaymentAcceptancePointOperation.CurrencyNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan faol valyuta topilmadi.",
            LanguageIdConst.RU => $"Активная валюта с id {id} не найдена.",
            _ => $"Active currency with id {id} was not found."
        });

    public static Error OrganizationMismatch(long id, short? languageId = null) =>
        Error.Forbidden("PaymentAcceptancePointOperation.OrganizationMismatch", languageId switch
        {
            LanguageIdConst.UZ => $"{id} operatsiya boshqa tashkilotga tegishli.",
            LanguageIdConst.RU => $"Операция {id} принадлежит другой организации.",
            _ => $"Operation {id} belongs to another organization."
        });

    public static Error InvalidStatus(long id, short statusId, string action) =>
        Error.Business(
            "PaymentAcceptancePointOperation.InvalidStatus",
            $"Operation {id} with status {statusId} cannot be {action}.");

    public static Error InvalidConfiguration(string detail) =>
        Error.Business("PaymentAcceptancePointOperation.InvalidConfiguration", detail);

    public static Error InsufficientBalance(decimal available, decimal requested) =>
        Error.Business(
            "PaymentAcceptancePointOperation.InsufficientBalance",
            $"Available payment acceptance point balance is {available}; requested outgoing amount is {requested}.");

    public static Error BusinessEffectsAlreadyExist(long id) =>
        Error.Conflict(
            "PaymentAcceptancePointOperation.BusinessEffectsAlreadyExist",
            $"Operation {id} already has active money register effects.");

    public static Error MissingPostingBatch(long id) =>
        Error.Conflict(
            "PaymentAcceptancePointOperation.MissingPostingBatch",
            $"Posted operation {id} has no active posting batch.");

    public static Error MissingMoneyEntries(long id) =>
        Error.Conflict(
            "PaymentAcceptancePointOperation.MissingMoneyEntries",
            $"Posted operation {id} has no money register entries to reverse.");
}
