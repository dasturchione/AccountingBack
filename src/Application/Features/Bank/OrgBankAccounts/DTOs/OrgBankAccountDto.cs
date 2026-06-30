namespace Application.Features.OrgBankAccounts;

public class OrgBankAccountDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string OrganizationInn { get; set; } = null!;
    public int BankId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string BankName { get; set; } = null!;
    public string? BankInn { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public short CurrencyId { get; set; }
    public string CurrencyName { get; set; } = null!;
    public bool IsMain { get; set; }
    public decimal OpeningBalance { get; set; }
    public DateOnly? OpeningBalanceDate { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
