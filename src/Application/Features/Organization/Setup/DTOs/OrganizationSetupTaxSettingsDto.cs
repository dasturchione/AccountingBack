namespace Application.Features.OrganizationSetup;

public sealed class OrganizationSetupTaxSettingsDto
{
    public short TaxTypeId { get; set; }
    public bool IsVatPayer { get; set; }
    public string? VatRegistrationNumber { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public short StateId { get; set; } = 1;
}
