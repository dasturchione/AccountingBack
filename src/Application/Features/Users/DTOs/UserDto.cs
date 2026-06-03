namespace Application.Features.Users
{
    public class UserDto
    {
        public int Id { get; set; }

        public string UserName { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string? Email { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public int RoleId { get; set; }

        public DateTime? LastAccessTime { get; set; }

        public short StateId { get; set; }

        public DateTime CreatedDate { get; set; }

        public string RoleName { get; set; } = null!;

        public string StateName { get; set; } = null!;
    }
}
