using Application.Features.Reports.Models;

namespace Application.Features.Reports.Queries;

/// <summary>
/// Base response abstraction for future report queries.
/// </summary>
/// <typeparam name="TItem">Row type.</typeparam>
public class ReportQueryResponse<TItem> : ReportResponseDto<TItem>
{
}
