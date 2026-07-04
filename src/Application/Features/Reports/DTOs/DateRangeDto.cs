namespace Application.Features.Reports.DTOs;

/// <summary>
/// Represents a simple date range filter.
/// </summary>
public class DateRangeDto
{
    /// <summary>
    /// Inclusive range start.
    /// </summary>
    public DateTime? From { get; set; }

    /// <summary>
    /// Inclusive range end.
    /// </summary>
    public DateTime? To { get; set; }
}
