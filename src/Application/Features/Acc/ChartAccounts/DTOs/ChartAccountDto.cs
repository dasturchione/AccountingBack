namespace Application.Features.ChartAccounts;

public class ChartAccountDto
{
    public int Id { get; set; }
    public string? Code { get; set; } 
    public string Name { get; set; } = null!;
    public string Number { get; set; } = null!;
    public short? AccountTypeId { get; set; }
    public string? AccountTypeCode { get; set; }
    public string? AccountTypeName { get; set; }
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public string? ParentCode { get; set; }
    public string? ParentNumber { get; set; }
    public bool IsGroup { get; set; }
    public bool IsQuantity { get; set; }
    public bool IsCurrency { get; set; }
    public bool IsDepartment { get; set; }
    public bool IsTaxAccounting { get; set; }
    public bool IsOffBalance { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
