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
        }
    }
}
