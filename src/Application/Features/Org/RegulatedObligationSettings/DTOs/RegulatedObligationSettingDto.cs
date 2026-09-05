namespace Application.Features.RegulatedObligationSettings;

public sealed class RegulatedObligationSettingDto
{
    public short RegulatedObligationId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public short CategoryId { get; set; }
    public string CategoryCode { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public int? SettingId { get; set; }
    public int OrganizationId { get; set; }
    public short? PeriodicityId { get; set; }
    public string? PeriodicityCode { get; set; }
    public string? PeriodicityName { get; set; }
    public string? ClassifierCode { get; set; }
    public decimal? Rate { get; set; }
    public int? ChartAccountId { get; set; }
    public string? ChartAccountNumber { get; set; }
    public string? ChartAccountName { get; set; }
    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public short? StateId { get; set; }
    public string? StateName { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
