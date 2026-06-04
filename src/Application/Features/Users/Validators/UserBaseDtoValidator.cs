using FluentValidation;

namespace Application.Features.Users
{
    public class UserBaseDtoValidator : AbstractValidator<UserBaseDto>
    {
        public UserBaseDtoValidator()
        {
            RuleFor(x => x.UserName).NotEmpty();
        }
    }
}
