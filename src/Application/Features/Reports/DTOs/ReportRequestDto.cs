namespace Application.Features.Reports.DTOs;

/// <summary>
/// Generic report request model.
/// </summary>
public class ReportRequestDto
{
    /// <summary>
    /// Common filter values.
    /// </summary>
    public ReportFilterDto Filter { get; set; } = new();

    /// <summary>
    /// Pagination settings.
    /// </summary>
    public PaginationDto Pagination { get; set; } = new();

    /// <summary>
    /// Sorting settings.
    /// </summary>
    public SortDto? Sort { get; set; }
}
