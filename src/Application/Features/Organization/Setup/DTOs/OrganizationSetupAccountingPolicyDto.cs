namespace Application.Features.OrganizationSetup;

public sealed class OrganizationSetupAccountingPolicyDto
{
    public string InventoryValuationMethod { get; set; } = "fifo";
    public short? AccountingPolicyId { get; set; }
    public short? BaseCurrencyId { get; set; }
    public DateOnly? AccountingStartDate { get; set; }
    public short FiscalYearStartMonth { get; set; } = 1;
}
