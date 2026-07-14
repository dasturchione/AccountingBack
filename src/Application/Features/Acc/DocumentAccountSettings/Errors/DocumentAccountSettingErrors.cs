using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Acc.DocumentAccountSettings;

public static class DocumentAccountSettingErrors
{
    public static Error DocumentTypeNotFound(short documentTypeId, short? languageId = null) =>
        Error.NotFound("DocumentAccountSetting.DocumentTypeNotFound", GetDocumentTypeNotFoundMessage(documentTypeId, languageId));

    public static Error TypeRoleNotFound(int documentAccountTypeRoleId, short? languageId = null) =>
        Error.NotFound("DocumentAccountSetting.TypeRoleNotFound", GetTypeRoleNotFoundMessage(documentAccountTypeRoleId, languageId));

    public static Error TypeRoleNotFound(short documentTypeId, short? documentRoleId, string? documentRoleCode, short? languageId = null) =>
        Error.NotFound("DocumentAccountSetting.TypeRoleNotFound", GetTypeRoleNotFoundMessage(documentTypeId, documentRoleId, documentRoleCode, languageId));

    public static Error RoleFilterRequired(short? languageId = null) =>
        Error.Business("DocumentAccountSetting.RoleFilterRequired", GetRoleFilterRequiredMessage(languageId));

    public static Error SettingNotFound(long id, short? languageId = null) =>
        Error.NotFound("DocumentAccountSetting.NotFound", GetSettingNotFoundMessage(id, languageId));

    public static Error DuplicateAccounts(short? languageId = null) =>
        Error.Business("DocumentAccountSetting.DuplicateAccounts", GetDuplicateAccountsMessage(languageId));

    public static Error MultipleDefaults(short? languageId = null) =>
        Error.Business("DocumentAccountSetting.MultipleDefaults", GetMultipleDefaultsMessage(languageId));

    public static Error ChartAccountsNotFound(IReadOnlyCollection<int> chartAccountIds, short? languageId = null) =>
        Error.NotFound("DocumentAccountSetting.ChartAccountsNotFound", GetChartAccountsNotFoundMessage(chartAccountIds, languageId));

    private static string GetDocumentTypeNotFoundMessage(short documentTypeId, short? languageId) => languageId switch
    {
        LanguageIdConst.UZ => $"Document account turi {documentTypeId} topilmadi.",
        LanguageIdConst.RU => $"Тип настройки счетов документа {documentTypeId} не найден.",
        _ => $"Document account type {documentTypeId} was not found."
    };

    private static string GetTypeRoleNotFoundMessage(int documentAccountTypeRoleId, short? languageId) => languageId switch
    {
        LanguageIdConst.UZ => $"Document account role sozlamasi {documentAccountTypeRoleId} topilmadi.",
        LanguageIdConst.RU => $"Роль настройки счетов документа {documentAccountTypeRoleId} не найдена.",
        _ => $"Document account type role {documentAccountTypeRoleId} was not found."
    };

    private static string GetTypeRoleNotFoundMessage(short documentTypeId, short? documentRoleId, string? documentRoleCode, short? languageId)
    {
        var role = documentRoleId.HasValue
            ? $"roleId={documentRoleId.Value}"
            : $"roleCode={documentRoleCode}";

        return languageId switch
        {
            LanguageIdConst.UZ => $"Document turi {documentTypeId} uchun role sozlamasi topilmadi ({role}).",
            LanguageIdConst.RU => $"Роль настройки счетов для типа документа {documentTypeId} не найдена ({role}).",
            _ => $"Document account role setting for document type {documentTypeId} was not found ({role})."
        };
    }

    private static string GetRoleFilterRequiredMessage(short? languageId) => languageId switch
    {
        LanguageIdConst.UZ => "documentRoleId yoki documentRoleCode dan biri berilishi kerak.",
        LanguageIdConst.RU => "Нужно передать documentRoleId или documentRoleCode.",
        _ => "Either documentRoleId or documentRoleCode must be provided."
    };

    private static string GetSettingNotFoundMessage(long id, short? languageId) => languageId switch
    {
        LanguageIdConst.UZ => $"Document account setting {id} topilmadi.",
        LanguageIdConst.RU => $"Настройка счета документа {id} не найдена.",
        _ => $"Document account setting {id} was not found."
    };

    private static string GetDuplicateAccountsMessage(short? languageId) => languageId switch
    {
        LanguageIdConst.UZ => "Bir rol ichida bitta hisob bir martadan ko'p berilmasligi kerak.",
        LanguageIdConst.RU => "Один счёт нельзя указывать больше одного раза внутри одной роли.",
        _ => "One chart account cannot be specified more than once inside one role."
    };

    private static string GetMultipleDefaultsMessage(short? languageId) => languageId switch
    {
        LanguageIdConst.UZ => "Bir rol uchun faqat bitta asosiy hisob tanlanishi mumkin.",
        LanguageIdConst.RU => "Для одной роли можно выбрать только один счёт по умолчанию.",
        _ => "Only one default account can be selected for one role."
    };

    private static string GetChartAccountsNotFoundMessage(IReadOnlyCollection<int> chartAccountIds, short? languageId)
    {
        var ids = string.Join(", ", chartAccountIds);
        return languageId switch
        {
            LanguageIdConst.UZ => $"Hisoblar rejasi topilmadi: {ids}.",
            LanguageIdConst.RU => $"Счета плана счетов не найдены: {ids}.",
            _ => $"Chart accounts were not found: {ids}."
        };
    }
}