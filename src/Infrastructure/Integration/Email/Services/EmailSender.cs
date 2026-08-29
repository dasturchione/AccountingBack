using Application.Abstractions.Integration;
using Application.Abstractions.Authentication;
using Application.Features.Notifications;
using Integration.Email.Configs;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SharedKernel.Results;

namespace Integration.Email.Services;

public sealed class EmailSender : IEmailSender
{
    private readonly EmailOptions _settings;
    private readonly ILogger<EmailSender> _logger;
    private readonly IUserContext _userContext;

    public EmailSender(IOptions<EmailOptions> options, ILogger<EmailSender> logger, IUserContext userContext)
    {
        _settings = options.Value;
        _logger = logger;
        _userContext = userContext;
    }

    public async Task<Result> SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        if (message.To is null || message.To.Count == 0)
            return Result.Failure(EmailErrors.NoRecipient(_userContext.LanguageId));

        MimeMessage mime;
        try
        {
            mime = BuildMessage(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build email message (subject: {Subject})", message.Subject);
            return Result.Failure(EmailErrors.BuildFailed(_userContext.LanguageId));
        }

        return await SendWithRetryAsync(mime, message.Subject, ct);
    }

    private MimeMessage BuildMessage(EmailMessage message)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));

        foreach (var to in message.To.Where(x => !string.IsNullOrWhiteSpace(x)))
            mime.To.Add(MailboxAddress.Parse(to));

        foreach (var cc in (message.Cc ?? []).Where(x => !string.IsNullOrWhiteSpace(x)))
            mime.Cc.Add(MailboxAddress.Parse(cc));

        foreach (var bcc in (message.Bcc ?? []).Where(x => !string.IsNullOrWhiteSpace(x)))
            mime.Bcc.Add(MailboxAddress.Parse(bcc));

        mime.Subject = message.Subject ?? string.Empty;

        var builder = new BodyBuilder { HtmlBody = message.HtmlBody };
        foreach (var attachment in message.Attachments ?? [])
        {
            if (attachment is null || attachment.Content.Length == 0 || string.IsNullOrWhiteSpace(attachment.FileName))
                continue;

            builder.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));
        }

        mime.Body = builder.ToMessageBody();
        return mime;
    }

    private async Task<Result> SendWithRetryAsync(MimeMessage mime, string subject, CancellationToken ct)
    {
        var attempts = Math.Max(1, _settings.MaxRetries);
        var delay = TimeSpan.FromMilliseconds(300);
        var lastError = EmailErrors.SendFailed(_userContext.LanguageId);

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            using var client = new SmtpClient
            {
                Timeout = Math.Max(5, _settings.TimeoutSeconds) * 1000
            };

            try
            {
                var socketOptions = _settings.UseStartTls
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.Auto;

                await client.ConnectAsync(_settings.Host, _settings.Port, socketOptions, ct);
                await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);
                await client.SendAsync(mime, ct);

                _logger.LogInformation(
                    "Email sent to {Recipients} (subject: {Subject})",
                    string.Join(", ", mime.To.Mailboxes.Select(x => x.Address)),
                    subject);

                return Result.Success();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastError = EmailErrors.SendFailed(_userContext.LanguageId);
                _logger.LogWarning(ex, "Email send attempt {Attempt}/{Attempts} failed", attempt, attempts);

                if (attempt < attempts)
                {
                    await Task.Delay(delay, ct);
                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
                }
            }
            finally
            {
                if (client.IsConnected)
                {
                    try { await client.DisconnectAsync(true, CancellationToken.None); }
                    catch { /* disconnect xatosi asosiy natijaga ta'sir qilmasin */ }
                }
            }
        }

        return Result.Failure(lastError);
    }
}
