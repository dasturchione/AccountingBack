namespace Application.Features.AiAssistant;

public interface IAiReadOnlyToolExecutor
{
    AiToolDescriptor Descriptor { get; }

    Task<AiToolExecutionOutcome> ExecuteAsync(
        AiToolInvocationRequest request,
        CancellationToken ct = default);
}

public sealed record AiToolInvocationRequest(
    object? Input,
    AiOrganizationContext OrganizationContext,
    AiLanguageContext LanguageContext);

public sealed record AiToolExecutionOutcome(
    AiToolResultStatus Status,
    IReadOnlyList<AiToolEvidence> Evidence,
    AiToolError? Error,
    bool IsComplete,
    string? Answer = null)
{
    public static AiToolExecutionOutcome Success(
        IReadOnlyList<AiToolEvidence>? evidence = null,
        string? answer = null) =>
        new(AiToolResultStatus.Success, evidence ?? [], null, true, answer);

    public static AiToolExecutionOutcome NoData(
        IReadOnlyList<AiToolEvidence>? evidence = null) =>
        new(AiToolResultStatus.NoData, evidence ?? [], null, true);

    public static AiToolExecutionOutcome Incomplete(
        IReadOnlyList<AiToolEvidence>? evidence = null) =>
        new(AiToolResultStatus.IncompleteData, evidence ?? [], null, false);

    public static AiToolExecutionOutcome Failure(
        AiToolError error,
        IReadOnlyList<AiToolEvidence>? evidence = null) =>
        new(AiToolResultStatus.Error, evidence ?? [], error, false);
}

public interface IAiReadOnlyToolRegistry
{
    IReadOnlyList<AiToolDescriptor> Descriptors { get; }

    bool IsAllowed(string? toolName);

    bool TryGet(string toolName, out IAiReadOnlyToolExecutor? executor);
}

public sealed class AiReadOnlyToolRegistry(
    IEnumerable<IAiReadOnlyToolExecutor> executors) : IAiReadOnlyToolRegistry
{
    private readonly IReadOnlyDictionary<string, IAiReadOnlyToolExecutor> _executors =
        executors
            .Where(executor => AccountingReadOnlyToolCatalog.IsAllowed(executor.Descriptor.Name)
                && executor.Descriptor.IsReadOnly
                && executor.Descriptor.Authorization == AiToolAuthorizationRequirement.AuthenticatedOrganizationMember)
            .GroupBy(executor => executor.Descriptor.Name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

    public IReadOnlyList<AiToolDescriptor> Descriptors => AccountingReadOnlyToolCatalog.All;

    public bool IsAllowed(string? toolName) =>
        AccountingReadOnlyToolCatalog.IsAllowed(toolName);

    public bool TryGet(string toolName, out IAiReadOnlyToolExecutor? executor)
    {
        if (!IsAllowed(toolName))
        {
            executor = null;
            return false;
        }

        return _executors.TryGetValue(toolName, out executor);
    }
}
