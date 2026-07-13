using SharedKernel.Results;

namespace Application.Features.Didox;

internal static class DidoxErrors
{
    public static Error InvalidEffectiveDates() => Error.Business("Didox.InvalidEffectiveDates", "effective_to must be on or after effective_from.");
    public static Error OverlappingEffectiveDates() => Error.Conflict("Didox.EffectiveDateOverlap", "The effective-date interval overlaps an existing Didox record.");
    public static Error NotFound(string name) => Error.NotFound("Didox.NotFound", $"Didox {name} was not found.");
    public static Error SourceMissing(string name) => Error.Business("Didox.SourceMissing", $"Required Didox source is missing: {name}.");
    public static Error OrganizationRequired() => Error.Business("Didox.OrganizationRequired", "An organization scope is required.");
    public static Error OrganizationMismatch(string name) => Error.Forbidden("Didox.OrganizationMismatch", $"{name} does not belong to the current organization.");
    public static Error Duplicate(string name) => Error.Conflict("Didox.Duplicate", $"A Didox {name} with the same key already exists.");
    public static Error EmptyImport() => Error.Business("Didox.EmptyImport", "The MXIK import command contains no items.");
}
