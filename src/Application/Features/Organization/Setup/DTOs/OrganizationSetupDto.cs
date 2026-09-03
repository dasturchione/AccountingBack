namespace Application.Features.OrganizationSetup;

public sealed class OrganizationSetupDto
{
    public int OrganizationId { get; set; }
    public string SetupStatus { get; set; } = null!;
    public string CurrentStep { get; set; } = null!;
    public bool OrganizationCompleted { get; set; }
    public bool TaxCompleted { get; set; }
    public bool AccountingCompleted { get; set; }
    public bool DefaultsCompleted { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public OrganizationSetupCompanyProfileDto CompanyProfile { get; set; } = null!;
    public OrganizationSetupTaxSettingsDto? TaxSettings { get; set; }
    public OrganizationSetupAccountingPolicyDto? AccountingPolicy { get; set; }
    public OrganizationSetupCostingConditionDto? CostingCondition { get; set; }
    public OrganizationSetupPricingConditionDto? PricingCondition { get; set; }
    public OrganizationSetupDefaultsDto? Defaults { get; set; }
}
