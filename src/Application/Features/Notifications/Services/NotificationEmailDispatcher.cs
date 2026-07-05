using System.Net;
using Application.Abstractions;
using Application.Abstractions.Integration;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query.Specifications;

namespace Application.Features.Notifications;

public sealed class NotificationEmailDispatcher : INotificationEmailDispatcher
{
    private readonly IQueryRepository<User> _userQuery;
    private readonly IQueryRepository<Notification> _notificationQuery;
    private readonly IQueryRepository<NotificationDelivery> _deliveryQuery;
    private readonly ICommandRepository<NotificationDelivery> _deliveryCommand;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<NotificationEmailDispatcher> _logger;

    public NotificationEmailDispatcher(
        IQueryRepository<User> userQuery,
        IQueryRepository<Notification> notificationQuery,
        IQueryRepository<NotificationDelivery> deliveryQuery,
        ICommandRepository<NotificationDelivery> deliveryCommand,
        IEmailSender emailSender,
        ILogger<NotificationEmailDispatcher> logger)
    {
        _userQuery = userQuery;
        _notificationQuery = notificationQuery;
        _deliveryQuery = deliveryQuery;
        _deliveryCommand = deliveryCommand;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task DispatchAsync(Notification notification, NotificationDelivery delivery, CancellationToken ct = default)
    {
        if (delivery.Channel != (short)NotificationChannel.Email)
            return;

        try
        {
            var recipient = await ResolveRecipientAsync(notification, ct);
            if (!recipient.IsSuccess)
            {
                await TryMarkFailedAsync(delivery, recipient.Error, ct);
                return;
            }

            var message = BuildMessage(notification, recipient.Value);
            var sendResult = await _emailSender.SendAsync(message, ct);

            if (sendResult.IsSuccess)
                await TryMarkSentAsync(delivery, ct);
            else
                await TryMarkFailedAsync(delivery, sendResult.Error.Description, ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Notification email dispatch failed for delivery {DeliveryId}", delivery.Id);
            await TryMarkFailedAsync(delivery, ex.Message, ct);
        }
    }

    public async Task DispatchPendingAsync(CancellationToken ct = default)
    {
        var pendingDeliveries = await _deliveryQuery.GetAllAsync(new QuerySpecification<NotificationDelivery>
        {
            Criteria = x => x.Channel == (short)NotificationChannel.Email
                && x.Status == (short)NotificationDeliveryStatus.Pending,
            OrderBy = q => q.OrderBy(x => x.CreatedDate)
        }, ct);

        foreach (var delivery in pendingDeliveries)
        {
            var notification = await _notificationQuery.GetAsync(new QuerySpecification<Notification>
            {
                Criteria = x => x.Id == delivery.NotificationId
            }, ct);

            if (notification is null)
            {
                await TryMarkFailedAsync(delivery, $"Parent notification {delivery.NotificationId} was not found.", ct);
                continue;
            }

            await DispatchAsync(notification, delivery, ct);
        }
    }

    private async Task<(bool IsSuccess, RecipientInfo Value, string Error)> ResolveRecipientAsync(Notification notification, CancellationToken ct)
    {
        if (!notification.UserId.HasValue)
            return (false, default, "Email channel currently supports only personal notifications.");

        var user = await _userQuery.GetAsync(new QuerySpecification<User>
        {
            Criteria = x => x.Id == notification.UserId.Value
        }, ct);

        if (user is null)
            return (false, default, $"User {notification.UserId.Value} was not found.");

        if (string.IsNullOrWhiteSpace(user.Email))
            return (false, default, $"User {user.Id} does not have an email address.");

        return (true, new RecipientInfo(user.Email.Trim(), user.LanguageId, BuildDisplayName(user)), string.Empty);
    }

    private static EmailMessage BuildMessage(Notification notification, RecipientInfo recipient)
    {
        var intro = recipient.LanguageId switch
        {
            LanguageIdConst.UZ => "Siz uchun yangi bildirishnoma yaratildi.",
            LanguageIdConst.UZ_CYRL => "Siz uchun yangi bildirishnoma yaratildi.",
            LanguageIdConst.RU => "Dlya vas sozdano novoye uvedomleniye.",
            _ => "A new notification is available for you."
        };

        var greeting = recipient.LanguageId switch
        {
            LanguageIdConst.UZ => $"Assalomu alaykum, {recipient.DisplayName}!",
            LanguageIdConst.UZ_CYRL => $"Assalomu alaykum, {recipient.DisplayName}!",
            LanguageIdConst.RU => $"Zdravstvuyte, {recipient.DisplayName}!",
            _ => $"Hello, {recipient.DisplayName}!"
        };

        var cta = recipient.LanguageId switch
        {
            LanguageIdConst.UZ => "Tizimga kirib tafsilotlarni ko'rishingiz mumkin.",
            LanguageIdConst.UZ_CYRL => "Tizimga kirib tafsilotlarni ko'rishingiz mumkin.",
            LanguageIdConst.RU => "Vy mozhete voiti v sistemu i posmotret podrobnosti.",
            _ => "You can sign in to the system to review the details."
        };

        var htmlBody = $"""
        <div style="font-family:Arial,sans-serif;max-width:640px;margin:auto;color:#222">
          <p>{WebUtility.HtmlEncode(greeting)}</p>
          <p>{WebUtility.HtmlEncode(intro)}</p>
          <h3 style="margin-bottom:8px">{WebUtility.HtmlEncode(notification.Title)}</h3>
          <div style="background:#f7f7f7;padding:12px 14px;border-radius:8px;line-height:1.5">
            {FormatBody(notification.Body)}
          </div>
          <p style="margin-top:14px">{WebUtility.HtmlEncode(cta)}</p>
          {FormatLink(notification.Link)}
        </div>
        """;

        return new EmailMessage
        {
            To = [recipient.Email],
            Subject = notification.Title,
            HtmlBody = htmlBody
        };
    }

    private async Task MarkSentAsync(NotificationDelivery delivery, CancellationToken ct)
    {
        delivery.Status = (short)NotificationDeliveryStatus.Sent;
        delivery.SentAt = DateTime.Now;
        delivery.Error = null;
        await _deliveryCommand.UpdateAsync(delivery, ct);
    }

    private async Task MarkFailedAsync(NotificationDelivery delivery, string error, CancellationToken ct)
    {
        delivery.Status = (short)NotificationDeliveryStatus.Failed;
        delivery.Error = TrimError(error);
        await _deliveryCommand.UpdateAsync(delivery, ct);
    }

    private async Task TryMarkSentAsync(NotificationDelivery delivery, CancellationToken ct)
    {
        try
        {
            await MarkSentAsync(delivery, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist sent status for notification delivery {DeliveryId}", delivery.Id);
        }
    }

    private async Task TryMarkFailedAsync(NotificationDelivery delivery, string error, CancellationToken ct)
    {
        try
        {
            await MarkFailedAsync(delivery, error, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist failed status for notification delivery {DeliveryId}", delivery.Id);
        }
    }

    private static string BuildDisplayName(User user)
    {
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? user.UserName : fullName;
    }

    private static string FormatBody(string body) =>
        WebUtility.HtmlEncode(body).Replace("\r\n", "<br/>").Replace("\n", "<br/>");

    private static string FormatLink(string? link)
    {
        if (string.IsNullOrWhiteSpace(link))
            return string.Empty;

        var encodedLink = WebUtility.HtmlEncode(link.Trim());
        return $"""<p><a href="{encodedLink}" target="_blank" rel="noopener noreferrer">{encodedLink}</a></p>""";
    }

    private static string TrimError(string error) =>
        string.IsNullOrWhiteSpace(error)
            ? "Unknown email delivery error."
            : error.Length <= 1000 ? error : error[..1000];

    private readonly record struct RecipientInfo(string Email, short? LanguageId, string DisplayName);
}
