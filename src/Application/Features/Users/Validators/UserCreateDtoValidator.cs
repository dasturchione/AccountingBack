using FluentValidation;

namespace Application.Features.Users.Validators
{
    public class UserCreateDtoValidator : AbstractValidator<UserCreateDto>
    {
        public UserCreateDtoValidator()
        {
            Include(new UserBaseDtoValidator());

            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        }
    }
}
