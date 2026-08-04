using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.BankParsers;

public static class BankStatementParserErrors
{
    public static Error InvalidBankType(short? languageId = null) =>
        Error.Business("BankStatement.InvalidBankType", languageId switch
        {
            LanguageIdConst.UZ => "Bank ko'chirma shabloni noto'g'ri tanlangan.",
            LanguageIdConst.RU => "Выбран некорректный шаблон банковской выписки.",
            _ => "The selected bank statement template is invalid."
        });

    public static Error StatementNotFound(BankStatementBankType bankType, short? languageId = null) =>
        Error.Business("BankStatement.StatementNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"{bankType} uchun bank ko'chirmasi fayldan topilmadi.",
            LanguageIdConst.RU => $"В файле не найдена выписка для шаблона {bankType}.",
            _ => $"No bank statement for the {bankType} template was found in the file."
        });
}