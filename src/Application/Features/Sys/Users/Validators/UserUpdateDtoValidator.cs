using FluentValidation;

namespace Application.Features.Users
{
    public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
    {
        public UserUpdateDtoValidator()
        {
            Include(new UserBaseDtoValidator());

            RuleFor(x => x.StateId).Must(id => id is 1 or 2);
        }
    }
}
