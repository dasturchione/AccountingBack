using SharedKernel.Constants;

namespace Application.Features.Register.PostingEngines;

internal static class ProductTypeDimensionValueResolver
{
    public static string Resolve(short productTypeId) =>
        productTypeId switch
        {
            ProductTypeIdConst.Good => "good",
            ProductTypeIdConst.GoodRetail => "good_retail",
            ProductTypeIdConst.GoodOther => "good_other",
            ProductTypeIdConst.MaterialRaw => "material_raw",
            ProductTypeIdConst.MaterialComponent => "material_component",
            ProductTypeIdConst.MaterialSparePart => "material_spare_part",
            ProductTypeIdConst.MaterialConstruction => "material_construction",
            ProductTypeIdConst.MaterialPackaging => "material_packaging",
            ProductTypeIdConst.MaterialOther => "material_other",
            ProductTypeIdConst.SemiFinished => "semi_finished",
            ProductTypeIdConst.FinishedGoods => "finished_goods",
            ProductTypeIdConst.ServiceMain => "service_main",
            ProductTypeIdConst.ServiceToll => "service_toll",
            ProductTypeIdConst.ServiceAuxiliary => "service_auxiliary",
            ProductTypeIdConst.ServiceMaintenance => "service_maintenance",
            ProductTypeIdConst.ServiceRental => "service_rental",
            ProductTypeIdConst.ExpenseAdministrative => "expense_administrative",
            ProductTypeIdConst.ExpenseOperating => "expense_operating",
            _ => RegisterDefaultsConst.DefaultDimensionValue
        };
}
