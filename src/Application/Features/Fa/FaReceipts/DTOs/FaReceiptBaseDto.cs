namespace Application.Features.FaReceipts;

public partial class FaReceiptBaseDto
{
    public DateTime DocDate { get; set; }
    public int? CounterpartyId { get; set; }
    public int? WarehouseId { get; set; }
    public short CurrencyId { get; set; }
    public string ReceiptType { get; set; } = null!;
    public List<FaReceiptLineWriteDto> Lines { get; set; } = new();
}

public partial class FaReceiptLineWriteDto
{
    public int? SourceProductId { get; set; }
    public string Name { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public short? VatRateId { get; set; }
    public List<FaReceiptAssetWriteDto> Assets { get; set; } = new();
}

public partial class FaReceiptAssetWriteDto
{
    public string InventoryNumber { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal InitialCost { get; set; }
    public decimal SalvageValue { get; set; }
    public int UsefulLifeMonths { get; set; }
    public short DepreciationMethodId { get; set; }
    public int FaGroupId { get; set; }
    public short? OkofId { get; set; }
    public DateTime? CommissioningDate { get; set; }
    public DateTime? DeprStartDate { get; set; }
    public decimal? PlannedUnitsTotal { get; set; }
    public int? DepartmentId { get; set; }
    public int? ResponsibleUserId { get; set; }
}
