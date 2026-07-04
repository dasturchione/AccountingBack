using SharedKernel.Filters;

namespace Application.Features.Reports.DTOs;

/// <summary>
/// Represents sorting parameters for a report.
/// </summary>
public class SortDto
{
    /// <summary>
    /// Field name used for sorting.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sorting direction.
    /// </summary>
    public SortDirection SortDirection { get; set; } = SortDirection.Asc;
}
