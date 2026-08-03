using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.FaAssets;

public static class FaAssetErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("FaAsset.NotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita topilmadi.",
            LanguageIdConst.RU => $"Osnovnoye sredstvo s id {id} ne naydeno.",
            _ => $"Fixed asset with id {id} was not found."
        });

    public static Error InventoryNumberConflict(string inventoryNumber, short? languageId = null) =>
        Error.Conflict("FaAsset.InventoryNumberConflict", languageId switch
        {
            LanguageIdConst.UZ => $"Inventar raqami '{inventoryNumber}' bo'lgan asosiy vosita allaqachon mavjud.",
            LanguageIdConst.RU => $"Osnovnoye sredstvo s inventarnym nomerom '{inventoryNumber}' uzhe sushchestvuyet.",
            _ => $"Fixed asset with inventory number '{inventoryNumber}' already exists."
        });

    public static Error FaGroupNotFound(int id, short? languageId = null) =>
        Error.NotFound("FaAsset.FaGroupNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita guruhi topilmadi.",
            LanguageIdConst.RU => $"Gruppa osnovnykh sredstv s id {id} ne naydena.",
            _ => $"Fixed asset group with id {id} was not found."
        });

    public static Error OkofNotFound(short id, short? languageId = null) =>
        Error.NotFound("FaAsset.OkofNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan OKOF kodi topilmadi.",
            LanguageIdConst.RU => $"Kod OKOF s id {id} ne nayden.",
            _ => $"OKOF with id {id} was not found."
        });

    public static Error DepreciationMethodNotFound(short id, short? languageId = null) =>
        Error.NotFound("FaAsset.DepreciationMethodNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan amortizatsiya usuli topilmadi.",
            LanguageIdConst.RU => $"Metod amortizatsii s id {id} ne nayden.",
            _ => $"Depreciation method with id {id} was not found."
        });

    public static Error DepartmentNotFound(int id, short? languageId = null) =>
        Error.NotFound("FaAsset.DepartmentNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan bo'lim topilmadi.",
            LanguageIdConst.RU => $"Otdel s id {id} ne nayden.",
            _ => $"Department with id {id} was not found."
        });

    public static Error ResponsibleUserNotFound(int id, short? languageId = null) =>
        Error.NotFound("FaAsset.ResponsibleUserNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan mas'ul foydalanuvchi topilmadi.",
            LanguageIdConst.RU => $"Otvetstvennyy polzovatel s id {id} ne nayden.",
            _ => $"Responsible user with id {id} was not found."
        });

    public static Error SourceProductTableNotFound(int id, short? languageId = null) =>
        Error.NotFound("FaAsset.SourceProductTableNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan manba tovar kartochkasi topilmadi.",
            LanguageIdConst.RU => $"Istochnik tovarnoy kartochki s id {id} ne nayden.",
            _ => $"Source product table with id {id} was not found."
        });

    public static Error StatusNotFound(short id, short? languageId = null) =>
        Error.NotFound("FaAsset.StatusNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vosita holati topilmadi.",
            LanguageIdConst.RU => $"Status osnovnogo sredstva s id {id} ne nayden.",
            _ => $"Fixed asset status with id {id} was not found."
        });

    public static Error PlannedUnitsRequired(short? languageId = null) =>
        Error.Business("FaAsset.PlannedUnitsRequired", languageId switch
        {
            LanguageIdConst.UZ => "Ishlab chiqarish hajmi bo'yicha usul uchun rejalashtirilgan birliklar soni kiritilishi shart.",
            LanguageIdConst.RU => "Dlya metoda po obyemu proizvodstva nuzhno ukazat planovoye kolichestvo edinits.",
            _ => "Planned units total is required for units of production depreciation."
        });

    public static Error CommissioningDateRequiredForActive(short? languageId = null) =>
        Error.Business("FaAsset.CommissioningDateRequiredForActive", languageId switch
        {
            LanguageIdConst.UZ => "Ekspluatatsiyadagi asosiy vosita uchun qabul sanasi kiritilishi shart.",
            LanguageIdConst.RU => "Dlya osnovnogo sredstva v ekspluatatsii dolzhna byt ukazana data vvoda.",
            _ => "Commissioning date is required for an active fixed asset."
        });

    public static Error DepreciationStartDateRequiredForActive(short? languageId = null) =>
        Error.Business("FaAsset.DepreciationStartDateRequiredForActive", languageId switch
        {
            LanguageIdConst.UZ => "Ekspluatatsiyadagi asosiy vosita uchun amortizatsiya boshlanish sanasi kiritilishi shart.",
            LanguageIdConst.RU => "Dlya osnovnogo sredstva v ekspluatatsii dolzhna byt ukazana data nachala amortizatsii.",
            _ => "Depreciation start date is required for an active fixed asset."
        });

    public static Error CannotConfirmInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaAsset.CannotConfirmInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vositani {statusId} holatida tasdiqlab bo'lmaydi.",
            LanguageIdConst.RU => $"Основное средство с id {id} нельзя подтвердить в статусе {statusId}.",
            _ => $"Fixed asset with id {id} cannot be confirmed in status {statusId}."
        });

    public static Error CannotCancelInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("FaAsset.CannotCancelInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan asosiy vositani {statusId} holatida bekor qilib bo'lmaydi.",
            LanguageIdConst.RU => $"Основное средство с id {id} нельзя отменить в статусе {statusId}.",
            _ => $"Fixed asset with id {id} cannot be cancelled in status {statusId}."
        });
}
