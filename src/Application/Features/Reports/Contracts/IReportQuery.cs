namespace Application.Features.Reports.Contracts;

/// <summary>
/// Represents a read-only report query.
/// </summary>
/// <typeparam name="TRequest">Request model.</typeparam>
/// <typeparam name="TResponse">Response model.</typeparam>
public interface IReportQuery<in TRequest, TResponse>
{
    /// <summary>
    /// Executes the report query.
    /// </summary>
    Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}
