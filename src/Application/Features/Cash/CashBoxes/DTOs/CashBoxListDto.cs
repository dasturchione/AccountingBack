namespace Application.Features.CashBoxes;

public class CashBoxListDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public short CurrencyId { get; set; }
    public string CurrencyName { get; set; } = null!;
    public bool IsMain { get; set; }
    public int? ResponsibleUserId { get; set; }
    public decimal OpeningBalance { get; set; }
    public DateOnly? OpeningBalanceDate { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
