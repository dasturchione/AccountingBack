namespace Application.Features.Users
{
    public class UserCreateDto : UserBaseDto
    {
        public string Password { get; set; } = null!;
    }
}
