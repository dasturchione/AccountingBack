namespace Application.Features.ChartAccountPresetAccounts;

public class ChartAccountPresetAccountListDto
{
    public int Id { get; set; }
    public short PresetId { get; set; }
    public string? Code { get; set; }
    public string Number { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int? ParentPresetAccountId { get; set; }
    public string? ParentNumber { get; set; }
    public short AccountTypeId { get; set; }
    public string AccountTypeCode { get; set; } = null!;
    public string AccountTypeName { get; set; } = null!;
    public bool IsGroup { get; set; }
    public bool IsQuantity { get; set; }
    public bool IsCurrency { get; set; }
    public bool IsDepartment { get; set; }
    public bool IsTaxAccounting { get; set; }
    public bool IsOffBalance { get; set; }
    public int DisplayOrder { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public bool HasChartAccount { get; set; }
    public List<ChartAccountPresetAccountListDto> Lines { get; set; } = new();
}
