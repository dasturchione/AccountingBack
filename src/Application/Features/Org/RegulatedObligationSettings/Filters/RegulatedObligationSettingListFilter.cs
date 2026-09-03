namespace Application.Features.RegulatedObligationSettings;

public sealed class RegulatedObligationSettingListFilter
{
    public string? CategoryCode { get; set; }
    public DateOnly? ChoosedDate { get; set; }
    public string? Search { get; set; }
    public bool? IsConfigured { get; set; }
}
