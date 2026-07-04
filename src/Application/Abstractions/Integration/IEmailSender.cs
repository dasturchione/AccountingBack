using SharedKernel.Results;

namespace Application.Abstractions.Integration;

public sealed class EmailMessage
{
    public List<string> To { get; init; } = [];
    public List<string>? Cc { get; init; }
    public List<string>? Bcc { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string HtmlBody { get; init; } = string.Empty;
    public List<EmailAttachment>? Attachments { get; init; }
}

public sealed class EmailAttachment
{
    public string FileName { get; init; } = null!;
    public byte[] Content { get; init; } = [];
    public string ContentType { get; init; } = "application/octet-stream";
}

public interface IEmailSender
{
    Task<Result> SendAsync(EmailMessage message, CancellationToken ct = default);
}
