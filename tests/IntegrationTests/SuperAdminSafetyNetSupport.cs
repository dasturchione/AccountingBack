using Application.Abstractions.Integration;
using SharedKernel.Results;

namespace IntegrationTests;

public sealed class RecordingEmailSender : IEmailSender
{
    public List<EmailMessage> Messages { get; } = [];

    public Task<Result> SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        Messages.Add(message);
        return Task.FromResult(Result.Success());
    }
}
