namespace Application.Features.Cmn.Taxes;

/// <summary>
/// Common tax payload.
/// </summary>
public class TaxBaseDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Rate { get; set; }
    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
}
