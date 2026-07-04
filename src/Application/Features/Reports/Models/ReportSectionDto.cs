namespace Application.Features.Reports.Models;

/// <summary>
/// Logical report section containing rows and totals.
/// </summary>
/// <typeparam name="TItem">Row type.</typeparam>
public class ReportSectionDto<TItem>
{
    /// <summary>
    /// Section code.
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Section name.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Section rows.
    /// </summary>
    public List<TItem> Items { get; set; } = [];

    /// <summary>
    /// Section totals.
    /// </summary>
    public ReportTotalsDto Totals { get; set; } = new();
}
