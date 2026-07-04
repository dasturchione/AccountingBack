namespace Application.Features.Reports.Models;

/// <summary>
/// Base report response model.
/// </summary>
/// <typeparam name="TItem">Row type.</typeparam>
public class ReportResponseDto<TItem>
{
    /// <summary>
    /// Page number.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Page size.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total item count.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total pages.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Indicates whether previous page exists.
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// Indicates whether next page exists.
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// Report rows.
    /// </summary>
    public List<TItem> Items { get; set; } = [];

    /// <summary>
    /// Report summaries.
    /// </summary>
    public List<ReportSummaryDto> Summary { get; set; } = [];

    /// <summary>
    /// Report totals.
    /// </summary>
    public ReportTotalsDto Totals { get; set; } = new();
}
