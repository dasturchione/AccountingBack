using Application.Abstractions.Authentication;

namespace Infrastructure.Context;

public sealed class BackgroundOrganizationScope : IBackgroundOrganizationScope
{
    private readonly AsyncLocal<ScopeState?> _current = new();

    public int? OrganizationId => _current.Value?.OrganizationId;
    public bool IsActive => OrganizationId.HasValue;

    public IDisposable Enter(int organizationId, string operation)
    {
        if (organizationId <= 0)
            throw new ArgumentOutOfRangeException(nameof(organizationId));
        if (string.IsNullOrWhiteSpace(operation))
            throw new ArgumentException("A background operation name is required.", nameof(operation));
        if (_current.Value is not null)
            throw new InvalidOperationException("A background organization scope is already active.");

        var state = new ScopeState(organizationId, operation);
        _current.Value = state;
        return new ScopeLease(this, state);
    }

    private sealed record ScopeState(int OrganizationId, string Operation);

    private sealed class ScopeLease(BackgroundOrganizationScope owner, ScopeState state) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            if (ReferenceEquals(owner._current.Value, state))
                owner._current.Value = null;
        }
    }
}
