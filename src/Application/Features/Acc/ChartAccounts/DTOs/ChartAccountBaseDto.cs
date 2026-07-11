namespace Application.Features.ChartAccounts;

public class ChartAccountBaseDto
{
    public int? ParentId { get; set; }
    public string Number { get; set; } = null!;
    public string? Code { get; set; } 
    public string Name { get; set; } = null!;
    public bool IsGroup { get; set; }
    public short? AccountTypeId { get; set; }
    public bool IsQuantity { get; set; }
    public bool IsCurrency { get; set; }
    public bool IsDepartment { get; set; }
    public bool IsTaxAccounting { get; set; }
    public bool IsOffBalance { get; set; }
}
