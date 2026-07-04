namespace Application.Features.Reports.DTOs;

/// <summary>
/// Common filter model for report requests.
/// </summary>
public class ReportFilterDto
{
    /// <summary>
    /// Optional organization identifier.
    /// </summary>
    public int? OrganizationId { get; set; }

    /// <summary>
    /// Optional branch identifier.
    /// </summary>
    public int? BranchId { get; set; }

    /// <summary>
    /// Optional currency identifier.
    /// </summary>
    public short? CurrencyId { get; set; }

    /// <summary>
    /// Optional date range.
    /// </summary>
    public DateRangeDto? DateRange { get; set; }
}
