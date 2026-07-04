namespace Application.Features.Reports.Contracts;

/// <summary>
/// Marks a validator that belongs to the Reports module.
/// </summary>
/// <typeparam name="TRequest">Validated request type.</typeparam>
public interface IReportValidator<in TRequest>
{
}
