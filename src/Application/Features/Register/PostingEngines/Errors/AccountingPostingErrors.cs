using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Register.PostingEngines;

public static class AccountingPostingErrors
{
    public static Error Empty(short? languageId = null) => Business("AccountingPosting.Empty", languageId,
        "O'tkazma shabloni buxgalteriya yozuvlarini yaratmadi.", "Ўтказма шаблони бухгалтерия ёзувларини яратмади.",
        "Шаблон проводки не создал бухгалтерских записей.", "Posting template produced no accounting entries.");

    public static Error InvalidDocument(short? languageId = null) => Business("AccountingPosting.InvalidDocument", languageId,
        "Buxgalteriya yozuvi haqiqiy hujjatga bog'lanishi kerak.", "Бухгалтерия ёзуви ҳақиқий ҳужжатга боғланиши керак.",
        "Бухгалтерская запись должна быть связана с действительным документом.", "Accounting entry must be linked to a valid document.");

    public static Error InvalidCurrency(short? languageId = null) => Business("AccountingPosting.InvalidCurrency", languageId,
        "Buxgalteriya yozuvida haqiqiy valyuta bo'lishi kerak.", "Бухгалтерия ёзувида ҳақиқий валюта бўлиши керак.",
        "В бухгалтерской записи должна быть указана корректная валюта.", "Accounting entry must contain a valid currency.");

    public static Error InvalidAmount(short? languageId = null) => Business("AccountingPosting.InvalidAmount", languageId,
        "Buxgalteriya yozuvi summasi noldan katta bo'lishi kerak.", "Бухгалтерия ёзуви суммаси нолдан катта бўлиши керак.",
        "Сумма бухгалтерской записи должна быть больше нуля.", "Accounting entry amount must be greater than zero.");

    public static Error InvalidDate(short? languageId = null) => Business("AccountingPosting.InvalidDate", languageId,
        "Buxgalteriya yozuvida hujjat va yaratilgan sana to'g'ri bo'lishi kerak.", "Бухгалтерия ёзувида ҳужжат ва яратилган сана тўғри бўлиши керак.",
        "В бухгалтерской записи должны быть корректные даты документа и создания.", "Accounting entry must contain valid document and created dates.");

    public static Error MissingAccount(short? languageId = null) => Business("AccountingPosting.MissingAccount", languageId,
        "Buxgalteriya yozuvida debet va kredit hisobvaraqlari bo'lishi kerak.", "Бухгалтерия ёзувида дебет ва кредит ҳисобварақлари бўлиши керак.",
        "В бухгалтерской записи должны быть указаны дебетовый и кредитовый счета.", "Accounting entry must contain both debit and credit accounts.");

    public static Error SameAccount(short? languageId = null) => Business("AccountingPosting.SameAccount", languageId,
        "Debet va kredit hisobvaraqlari har xil bo'lishi kerak.", "Дебет ва кредит ҳисобварақлари ҳар хил бўлиши керак.",
        "Дебетовый и кредитовый счета должны различаться.", "Debit and credit accounts must be different.");

    public static Error InvalidQuantity(short? languageId = null) => Business("AccountingPosting.InvalidQuantity", languageId,
        "Ko'rsatilgan miqdor noldan katta bo'lishi kerak.", "Кўрсатилган миқдор нолдан катта бўлиши керак.",
        "Указанное количество должно быть больше нуля.", "Accounting entry quantity must be greater than zero when provided.");

    public static Error BalanceMismatch(short? languageId = null) => Business("AccountingPosting.BalanceMismatch", languageId,
        "Buxgalteriya o'tkazmasi balanslanmagan: debet va kredit teng emas.", "Бухгалтерия ўтказмаси балансланмаган: дебет ва кредит тенг эмас.",
        "Бухгалтерская проводка не сбалансирована: дебет не равен кредиту.", "Accounting posting is not balanced: total debit does not match total credit.");

    private static Error Business(string code, short? languageId, string uz, string uzCyrl, string ru, string en) =>
        Error.Business(code, languageId switch
        {
            LanguageIdConst.UZ => uz,
            LanguageIdConst.UZ_CYRL => uzCyrl,
            LanguageIdConst.RU => ru,
            _ => en
        });
}
