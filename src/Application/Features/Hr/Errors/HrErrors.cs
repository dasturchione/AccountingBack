using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Hr;

public static class HrErrors
{
    public static Error NotFound(string entity, long id, short? languageId = null) =>
        Error.NotFound(
            $"Hr.{entity}.NotFound",
            languageId switch
            {
                LanguageIdConst.UZ => $"{GetEntityName(entity, LanguageIdConst.UZ)} topilmadi (ID: {id}).",
                LanguageIdConst.UZ_CYRL => $"{GetEntityName(entity, LanguageIdConst.UZ_CYRL)} топилмади (ID: {id}).",
                LanguageIdConst.RU => $"{GetEntityName(entity, LanguageIdConst.RU)} не найден (ID: {id}).",
                _ => $"{GetEntityName(entity, null)} was not found (ID: {id})."
            });

    public static Error Business(string code, string uzMessage, short? languageId = null) =>
        Error.Business($"Hr.{code}", KnownMessage(code, uzMessage, languageId));

    public static Error Conflict(string code, string uzMessage, short? languageId = null) =>
        Error.Conflict($"Hr.{code}", KnownMessage(code, uzMessage, languageId));

    public static Error FileStorage(string uzMessage, short? languageId = null) =>
        Error.Problem("Hr.FileStorage", languageId switch
        {
            LanguageIdConst.UZ => uzMessage,
            LanguageIdConst.UZ_CYRL => "Файлни сақлаш, топиш ёки қайта тиклашда хатолик юз берди.",
            LanguageIdConst.RU => "Произошла ошибка при сохранении, поиске или восстановлении файла.",
            _ => "A file could not be stored, found, or restored."
        });

    private static string GetEntityName(string entity, short? languageId) =>
        (entity, languageId) switch
        {
            ("Employee", LanguageIdConst.UZ) => "Xodim", ("Employee", LanguageIdConst.UZ_CYRL) => "Ходим", ("Employee", LanguageIdConst.RU) => "Сотрудник", ("Employee", _) => "Employee",
            ("WorkSchedule", LanguageIdConst.UZ) => "Ish grafigi", ("WorkSchedule", LanguageIdConst.UZ_CYRL) => "Иш графиги", ("WorkSchedule", LanguageIdConst.RU) => "Рабочий график", ("WorkSchedule", _) => "Work schedule",
            ("Absence", LanguageIdConst.UZ) => "Yo'qlik hujjati", ("Absence", LanguageIdConst.UZ_CYRL) => "Йўқлик ҳужжати", ("Absence", LanguageIdConst.RU) => "Документ отсутствия", ("Absence", _) => "Absence document",
            ("AbsenceType", LanguageIdConst.UZ) => "Yo'qlik turi", ("AbsenceType", LanguageIdConst.UZ_CYRL) => "Йўқлик тури", ("AbsenceType", LanguageIdConst.RU) => "Тип отсутствия", ("AbsenceType", _) => "Absence type",
            ("AbsenceAttachment", LanguageIdConst.UZ) => "Biriktirilgan fayl", ("AbsenceAttachment", LanguageIdConst.UZ_CYRL) => "Бириктирилган файл", ("AbsenceAttachment", LanguageIdConst.RU) => "Вложение отсутствия", ("AbsenceAttachment", _) => "Absence attachment",
            (_, LanguageIdConst.UZ) => "Ma'lumot", (_, LanguageIdConst.UZ_CYRL) => "Маълумот", (_, LanguageIdConst.RU) => "Запись", _ => "Record"
        };

    private static string KnownMessage(string code, string uzMessage, short? languageId)
    {
        if (languageId == LanguageIdConst.UZ)
            return uzMessage;

        var messages = code switch
        {
            "InvalidScheduleName" => ("График номи киритилиши ва 200 белгидан ошмаслиги керак.", "Название графика обязательно и не должно превышать 200 символов.", "Schedule name is required and must not exceed 200 characters."),
            "InvalidScheduleDates" => ("График тугаш санаси бошланиш санасидан олдин бўлиши мумкин эмас.", "Дата окончания графика не может быть раньше даты начала.", "Schedule end date cannot be earlier than start date."),
            "EmptySchedule" => ("Камида битта иш куни киритилиши керак.", "Необходимо указать хотя бы один рабочий день.", "At least one work day is required."),
            "InvalidScheduleDay" => ("Ҳафта куни ёки иш соатлари нотўғри.", "Недопустимый день недели или количество рабочих часов.", "Week day or work hours are invalid."),
            "DuplicateScheduleDay" => ("Ҳафта куни графикда такрорланмаслиги керак.", "День недели не должен повторяться в графике.", "A week day cannot be duplicated in a schedule."),
            "InvalidWeeklyHours" => ("Ҳафталик иш соати 168 соатдан ошмаслиги керак.", "Недельное рабочее время не должно превышать 168 часов.", "Weekly work hours must not exceed 168."),
            "EmptyAttachments" => ("Камида битта файл танланиши керак.", "Необходимо выбрать хотя бы один файл.", "At least one file must be selected."),
            "InvalidAbsenceDates" => ("Тугаш санаси бошланиш санасидан олдин бўлиши мумкин эмас.", "Дата окончания не может быть раньше даты начала.", "End date cannot be earlier than start date."),
            "AbsenceRangeTooLarge" => ("Йўқлик даври икки йилдан ошмаслиги керак.", "Период отсутствия не должен превышать два года.", "Absence period must not exceed two years."),
            "AbsenceNoteTooLong" => ("Изоҳ 1000 белгидан ошмаслиги керак.", "Примечание не должно превышать 1000 символов.", "Note must not exceed 1000 characters."),
            "InvalidCalendarRange" => ("Календар тугаш санаси бошланиш санасидан олдин бўлиши мумкин эмас.", "Дата окончания календаря не может быть раньше даты начала.", "Calendar end date cannot be earlier than start date."),
            "CalendarRangeTooLarge" => ("Календар даври икки йилдан ошмаслиги керак.", "Период календаря не должен превышать два года.", "Calendar period must not exceed two years."),
            _ => ($"Кадрлар операциясини бажариб бўлмади ({code}).", $"Не удалось выполнить кадровую операцию ({code}).", $"HR operation failed ({code}).")
        };

        return languageId switch
        {
            LanguageIdConst.UZ_CYRL => messages.Item1,
            LanguageIdConst.RU => messages.Item2,
            _ => messages.Item3
        };
    }
}
