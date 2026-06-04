namespace Application.Features.Users;

public class UserUpdateDto : UserCreateDto
{
    public class UserUpdateDto : UserBaseDto
    {
        public int StateId { get; set; }
    }
}
