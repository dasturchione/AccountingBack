using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Rnt.RentalContracts;

public static class RentalContractErrors
{
    public static Error NotFound(long id, short? languageId) => NotFound("RentalContract.NotFound", languageId,
        $"Id-si {id} bo'lgan ijara shartnomasi topilmadi.", $"Id-си {id} бўлган ижара шартномаси топилмади.",
        $"Договор аренды {id} не найден.", $"Rental contract {id} was not found.");
    public static Error InvalidStatus(long id, short status, short? languageId) => Conflict("RentalContract.InvalidStatus", languageId,
        $"{id}-shartnomani {status} holatida o'zgartirib bo'lmaydi.", $"{id}-шартномани {status} ҳолатида ўзгартириб бўлмайди.",
        $"Договор {id} нельзя изменить в статусе {status}.", $"Contract {id} cannot be changed in status {status}.");
    public static Error MissingAccounts(short? languageId) => Business("RentalContract.MissingAccounts", languageId,
        "Barcha xarajat, ijaraga beruvchi va soliq hisobvaraqlarini ko'rsating.", "Барча харажат, ижарага берувчи ва солиқ ҳисобварақларини кўрсатинг.",
        "Укажите счета расходов, расчётов с арендодателем и налога.", "Specify all expense, lessor payable and tax payable accounts.");
    public static Error InvalidReference(short? languageId) => Business("RentalContract.InvalidReference", languageId,
        "Valyuta, obyekt turi, kommunal xizmat yoki hisobvaraq faol emas yoxud tashkilotga tegishli emas.", "Валюта, объект тури, коммунал хизмат ёки ҳисобварақ фаол эмас ёхуд ташкилотга тегишли эмас.",
        "Валюта, тип объекта, коммунальная услуга или счёт неактивны либо относятся к другой организации.", "Currency, object type, utility service or account is inactive or belongs to another organization.");
    public static Error InvalidLessor(short? languageId) => Business("RentalContract.InvalidLessor", languageId,
        "Ijaraga beruvchi turi yoki identifikatsiya ma'lumotlari noto'g'ri.", "Ижарага берувчи тури ёки идентификация маълумотлари нотўғри.",
        "Тип или идентификационные данные арендодателя указаны неверно.", "The lessor type or identification data is invalid.");
    public static Error ExistingAccruals(long id, short? languageId) => Conflict("RentalContract.ExistingAccruals", languageId,
        $"{id}-shartnoma bo'yicha hisoblashlar mavjud.", $"{id}-шартнома бўйича ҳисоблашлар мавжуд.",
        $"По договору {id} уже существуют начисления.", $"Contract {id} already has accruals.");
    public static Error DuplicateNumber(string number, int year, short? languageId) => Conflict("RentalContract.DuplicateNumber", languageId,
        $"{year}-yil uchun {number}-raqamli ijara shartnomasi mavjud.", $"{year}-йил учун {number}-рақамли ижара шартномаси мавжуд.",
        $"Договор аренды №{number} за {year} год уже существует.", $"Rental contract {number} for {year} already exists.");

    private static Error Business(string code, short? lang, string uz, string cyrl, string ru, string en) => Error.Business(code, Message(lang, uz, cyrl, ru, en));
    private static Error Conflict(string code, short? lang, string uz, string cyrl, string ru, string en) => Error.Conflict(code, Message(lang, uz, cyrl, ru, en));
    private static Error NotFound(string code, short? lang, string uz, string cyrl, string ru, string en) => Error.NotFound(code, Message(lang, uz, cyrl, ru, en));
    private static string Message(short? lang, string uz, string cyrl, string ru, string en) => lang switch
    {
        LanguageIdConst.UZ => uz, LanguageIdConst.UZ_CYRL => cyrl, LanguageIdConst.RU => ru, _ => en
    };
}
