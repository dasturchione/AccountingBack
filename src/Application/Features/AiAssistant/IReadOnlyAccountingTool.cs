namespace Application.Features.AiAssistant;

public interface IReadOnlyAccountingTool<TInput, TOutput>
    where TInput : IAiToolInput
{
    AiToolDescriptor Descriptor { get; }

    Task<AiToolExecutionResult<TOutput>> ExecuteAsync(
        AiToolExecutionRequest<TInput> request,
        CancellationToken ct = default);
}

public interface IAiToolInput
{
    bool IsValid { get; }
}

public sealed record AiToolExecutionRequest<TInput>(
    TInput Input,
    AiOrganizationContext OrganizationContext,
    AiLanguageContext LanguageContext)
    where TInput : IAiToolInput;

public sealed record AiToolDescriptor(
    string Name,
    bool IsReadOnly,
    AiToolAuthorizationRequirement Authorization);

public enum AiToolAuthorizationRequirement : short
{
    AuthenticatedOrganizationMember = 1
}

public sealed record AiToolEvidence(
    string Source,
    string Description,
    DateTimeOffset? RetrievedAtUtc = null,
    string? Period = null);

public sealed record AiToolError(
    AiToolErrorKind Kind,
    string SafeMessage);

public enum AiToolErrorKind : short
{
    InvalidInput = 1,
    UnauthorizedOrganization = 2,
    Unavailable = 3
}

public sealed record AiToolExecutionResult<TOutput>(
    AiToolResultStatus Status,
    TOutput? Data,
    IReadOnlyList<AiToolEvidence> Evidence,
    AiToolError? Error)
{
    public static AiToolExecutionResult<TOutput> Success(
        TOutput data,
        IReadOnlyList<AiToolEvidence>? evidence = null) =>
        new(AiToolResultStatus.Success, data, evidence ?? [], null);

    public static AiToolExecutionResult<TOutput> NoData(
        IReadOnlyList<AiToolEvidence>? evidence = null) =>
        new(AiToolResultStatus.NoData, default, evidence ?? [], null);

    public static AiToolExecutionResult<TOutput> Failure(
        AiToolError error,
        IReadOnlyList<AiToolEvidence>? evidence = null) =>
        new(AiToolResultStatus.Error, default, evidence ?? [], error);
}

public enum AiToolResultStatus : short
{
    Success = 1,
    NoData = 2,
    Error = 3,
    IncompleteData = 4
}
