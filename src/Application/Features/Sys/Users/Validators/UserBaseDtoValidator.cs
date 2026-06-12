using FluentValidation;

namespace Application.Features.Users
{
    public class UserBaseDtoValidator : AbstractValidator<UserBaseDto>
    {
        public UserBaseDtoValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().MaximumLength(100);

            RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);

            RuleFor(x => x.Email).NotEmpty().EmailAddress();

            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);

            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

            RuleFor(x => x.RoleId).GreaterThan(0);

            RuleFor(x => x.Organizations).NotEmpty();

            RuleForEach(x => x.Organizations).ChildRules(org =>
            {
                org.RuleFor(o => o.OrganizationId).GreaterThan(0);
            });

            //RuleFor(x => x.Organizations)
            //    .Must(orgs => orgs.Count(o => o.IsDefault) <= 1)
            //    .WithMessage("Only one organization can be marked as default.");
        }
    }
}
