using SharedKernel.Results;

namespace Application.Features.Hr;

public static class HrErrors
{
    public static Error NotFound(string entity, long id) =>
        Error.NotFound($"Hr.{entity}.NotFound", $"{entity} with id {id} was not found.");

    public static Error Business(string code, string message) =>
        Error.Business($"Hr.{code}", message);

    public static Error Conflict(string code, string message) =>
        Error.Conflict($"Hr.{code}", message);

    public static Error FileStorage(string message) =>
        Error.Problem("Hr.FileStorage", message);
}
