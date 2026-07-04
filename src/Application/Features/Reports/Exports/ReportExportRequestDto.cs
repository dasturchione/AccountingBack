namespace Application.Features.Reports.Exports;

/// <summary>
/// Standard export request payload.
/// </summary>
public sealed class ReportExportRequestDto
{
    public ReportExportFormat Format { get; set; } = ReportExportFormat.Excel;
}
