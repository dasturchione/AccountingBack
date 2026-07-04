namespace Application.Features.Reports.Contracts;

/// <summary>
/// Builds a report response from a validated request.
/// </summary>
/// <typeparam name="TRequest">Request model.</typeparam>
/// <typeparam name="TResponse">Response model.</typeparam>
public interface IReportBuilder<in TRequest, TResponse>
{
    /// <summary>
    /// Builds the report response.
    /// </summary>
    TResponse Build(TRequest request);
}
