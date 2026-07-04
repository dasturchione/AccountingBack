namespace Application.Features.Reports.DTOs;

/// <summary>
/// Represents pagination parameters for reports.
/// </summary>
public class PaginationDto
{
    /// <summary>
    /// Page number starting from 1.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int? PageSize { get; set; } = 50;
}
