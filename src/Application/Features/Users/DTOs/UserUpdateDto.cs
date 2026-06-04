namespace Application.Features.Users;

public class UserUpdateDto : UserCreateDto
{
    public int Id { get; set; }
    public short StateId { get; set; }
}
