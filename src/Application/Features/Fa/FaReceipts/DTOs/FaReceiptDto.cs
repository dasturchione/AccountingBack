namespace Application.Features.FaReceipts;

public partial class FaReceiptDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public int? CounterpartyId { get; set; }
    public string? CounterpartyName { get; set; }
    public int? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public short CurrencyId { get; set; }
    public string CurrencyName { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public short ReceiptTypeId { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public DateTime? PostedAt { get; set; }
    public int? PostedByUserId { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledByUserId { get; set; }
    public List<FaReceiptLineDto> Lines { get; set; } = new();
}

public partial class FaReceiptLineDto
{
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public int? SourceProductId { get; set; }
    public string? SourceProductName { get; set; }
    public string Name { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Amount { get; set; }
    public short? VatRateId { get; set; }
    public string? VatRateName { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<FaReceiptAssetDto> Assets { get; set; } = new();
}

public partial class FaReceiptAssetDto
{
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public long? FaAssetId { get; set; }
    public string InventoryNumber { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal InitialCost { get; set; }
    public decimal SalvageValue { get; set; }
    public int UsefulLifeMonths { get; set; }
    public short DepreciationMethodId { get; set; }
    public string DepreciationMethodCode { get; set; } = null!;
    public string DepreciationMethodName { get; set; } = null!;
    public int FaGroupId { get; set; }
    public string FaGroupCode { get; set; } = null!;
    public string FaGroupName { get; set; } = null!;
    public short? OkofId { get; set; }
    public string? OkofCode { get; set; }
    public string? OkofName { get; set; }
    public DateTime? CommissioningDate { get; set; }
    public DateTime? DeprStartDate { get; set; }
    public decimal? PlannedUnitsTotal { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? ResponsibleUserId { get; set; }
    public string? ResponsibleUserName { get; set; }
}
