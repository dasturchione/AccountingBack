using System.ComponentModel.DataAnnotations.Schema;
using SharedKernel.Constants;

namespace Domain.Entities;

public partial class ProductTable
{
    [NotMapped]
    public int OrganizationId
    {
        get => Product?.OrganizationId ?? 0;
        set { }
    }

    [NotMapped]
    public short StateId
    {
        get => StateIdConst.ACTIVE;
        set { }
    }

    [NotMapped]
    public short StatusId
    {
        get => WarehouseProductTable?.StatusId ?? ProductTableStatusIdConst.SOLD;
        set { }
    }

    [NotMapped]
    public int? CurrentWarehouseId
    {
        get => WarehouseProductTable?.WarehouseId;
        set { }
    }

    [NotMapped]
    public Warehouse? CurrentWarehouse => WarehouseProductTable?.Warehouse;
}