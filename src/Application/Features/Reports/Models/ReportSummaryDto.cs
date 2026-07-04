namespace Application.Features.Reports.Models;

/// <summary>
/// Summary row for a report section.
/// </summary>
public class ReportSummaryDto
{
    /// <summary>
    /// Summary label.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Summary value.
    /// </summary>
    public decimal Value { get; set; }
}
