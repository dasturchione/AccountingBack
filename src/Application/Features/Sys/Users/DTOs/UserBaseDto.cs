namespace Application.Features.Users
{
    public class UserBaseDto
    {
        public string UserName { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string? Email { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public int RoleId { get; set; }

        public bool EmailVerified { get; set; }

        public bool IsPlatformAdmin { get; set; }

        public string? Timezone { get; set; }

        public List<int> Organizations { get; set; } = [];
    }
}
