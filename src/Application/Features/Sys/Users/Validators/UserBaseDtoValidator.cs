using FluentValidation;

namespace Application.Features.Users;

public class UserBaseDtoValidator : AbstractValidator<UserBaseDto>
{
    public UserBaseDtoValidator()
    {
        RuleFor(user => user.UserName).NotEmpty().MaximumLength(100);
        RuleFor(user => user.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(user => user.Email).NotEmpty().EmailAddress();
        RuleFor(user => user.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(user => user.LastName).NotEmpty().MaximumLength(100);
        RuleFor(user => user.Organizations).NotEmpty();
        RuleForEach(user => user.Organizations).ChildRules(organization =>
        {
            organization.RuleFor(item => item.OrganizationId).GreaterThan(0);
            organization.RuleFor(item => item.RoleId).NotNull().GreaterThan(0);
        });
    }
}