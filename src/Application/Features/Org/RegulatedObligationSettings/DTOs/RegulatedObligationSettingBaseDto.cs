namespace Application.Features.RegulatedObligationSettings;

public abstract class RegulatedObligationSettingBaseDto
{
    public short RegulatedObligationId { get; set; }
    public short PeriodicityId { get; set; }
    public string? ClassifierCode { get; set; }
    public decimal? Rate { get; set; }
    public int ChartAccountId { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public short StateId { get; set; }
}
