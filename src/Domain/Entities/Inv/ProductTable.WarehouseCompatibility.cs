using System.ComponentModel.DataAnnotations.Schema;

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
        get => WarehouseProductTable is null ? (short)0 : (short)1;
        set { }
    }

    [NotMapped]
    public short StatusId
    {
        get => WarehouseProductTable?.StatusId ?? 0;
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