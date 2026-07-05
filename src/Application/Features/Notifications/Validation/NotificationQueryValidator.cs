using FluentValidation;

namespace Application.Features.Notifications;

public sealed class NotificationQueryValidator : AbstractValidator<NotificationQuery>
{
    public NotificationQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .When(x => x.PageSize.HasValue);
        RuleFor(x => x.TypeId).GreaterThan((short)0).When(x => x.TypeId.HasValue);
    }
}
