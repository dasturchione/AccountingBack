namespace Application.Features.Reports.Models;

/// <summary>
/// Common totals container for reports.
/// </summary>
public class ReportTotalsDto
{
    /// <summary>
    /// Total debit amount.
    /// </summary>
    public decimal Debit { get; set; }

    /// <summary>
    /// Total credit amount.
    /// </summary>
    public decimal Credit { get; set; }

    /// <summary>
    /// Net amount.
    /// </summary>
    public decimal Net => Debit - Credit;
}
