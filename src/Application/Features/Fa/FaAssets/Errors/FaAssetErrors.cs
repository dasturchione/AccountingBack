using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.FaAssets;

public static class FaAssetErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("FaAsset.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган асосий восита топилмади.",
            LanguageIdConst.RU => $"Основное средство с id {id} не найдено.",
            _ => $"Fixed asset with id {id} was not found."
        });

    public static Error InventoryNumberConflict(string inventoryNumber, short? languageId = null) =>
        Error.Conflict("FaAsset.InventoryNumberConflict", languageId switch
        {
            LanguageIdConst.UZ => $"Inventar raqami '{inventoryNumber}' bo'lgan asosiy vosita allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Инвентар рақами '{inventoryNumber}' бўлган асосий восита аллақачон мавжуд.",
            LanguageIdConst.RU => $"Основное средство с инвентарным номером '{inventoryNumber}' уже существует.",
            _ => $"Fixed asset with inventory number '{inventoryNumber}' already exists."
        });

    public static Error FaGroupNotFound(int id, short? languageId = null) =>
        Error.NotFound("FaAsset.FaGroupNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita guruhi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган асосий восита гуруҳи топилмади.",
            LanguageIdConst.RU => $"Группа основных средств с id {id} не найдена.",
            _ => $"Fixed asset group with id {id} was not found."
        });

    public static Error OkofNotFound(short id, short? languageId = null) =>
        Error.NotFound("FaAsset.OkofNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan OKOF kodi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ОКОФ коди топилмади.",
            LanguageIdConst.RU => $"Код ОКОФ с id {id} не найден.",
            _ => $"OKOF with id {id} was not found."
        });

    public static Error DepreciationMethodNotFound(short id, short? languageId = null) =>
        Error.NotFound("FaAsset.DepreciationMethodNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan amortizatsiya usuli topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган амортизация усули топилмади.",
            LanguageIdConst.RU => $"Метод амортизации с id {id} не найден.",
            _ => $"Depreciation method with id {id} was not found."
        });

    public static Error DepartmentNotFound(int id, short? languageId = null) =>
        Error.NotFound("FaAsset.DepartmentNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan bo'lim topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган бўлим топилмади.",
            LanguageIdConst.RU => $"Подразделение с id {id} не найдено.",
            _ => $"Department with id {id} was not found."
        });

    public static Error ResponsibleUserNotFound(int id, short? languageId = null) =>
        Error.NotFound("FaAsset.ResponsibleUserNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan mas'ul foydalanuvchi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган масъул фойдаланувчи топилмади.",
            LanguageIdConst.RU => $"Ответственный пользователь с id {id} не найден.",
            _ => $"Responsible user with id {id} was not found."
        });

    public static Error StatusNotFound(short id, short? languageId = null) =>
        Error.NotFound("FaAsset.StatusNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita holati topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган асосий восита ҳолати топилмади.",
            LanguageIdConst.RU => $"Статус основного средства с id {id} не найден.",
            _ => $"Fixed asset status with id {id} was not found."
        });

    public static Error PlannedUnitsRequired(short? languageId = null) =>
        Error.Business("FaAsset.PlannedUnitsRequired", languageId switch
        {
            LanguageIdConst.UZ => "Ishlab chiqarish hajmi bo'yicha usul uchun rejalashtirilgan birliklar soni kiritilishi shart.",
            LanguageIdConst.UZ_CYRL => "Ишлаб чиқариш ҳажми бўйича усул учун режалаштирилган бирликлар сони киритилиши шарт.",
            LanguageIdConst.RU => "Для производственного метода необходимо указать плановое количество единиц.",
            _ => "Planned units total is required for units of production depreciation."
        });

    public static Error CommissioningDateRequiredForActive(short? languageId = null) =>
        Error.Business("FaAsset.CommissioningDateRequiredForActive", languageId switch
        {
            LanguageIdConst.UZ => "Ekspluatatsiyadagi asosiy vosita uchun qabul sanasi kiritilishi shart.",
            LanguageIdConst.UZ_CYRL => "Эксплуатациядаги асосий восита учун қабул санаси киритилиши шарт.",
            LanguageIdConst.RU => "Для основного средства в эксплуатации должна быть указана дата ввода.",
            _ => "Commissioning date is required for an active fixed asset."
        });

    public static Error DepreciationStartDateRequiredForActive(short? languageId = null) =>
        Error.Business("FaAsset.DepreciationStartDateRequiredForActive", languageId switch
        {
            LanguageIdConst.UZ => "Ekspluatatsiyadagi asosiy vosita uchun amortizatsiya boshlanish sanasi kiritilishi shart.",
            LanguageIdConst.UZ_CYRL => "Эксплуатациядаги асосий восита учун амортизация бошланиш санаси киритилиши шарт.",
            LanguageIdConst.RU => "Для основного средства в эксплуатации должна быть указана дата начала амортизации.",
            _ => "Depreciation start date is required for an active fixed asset."
        });

}
