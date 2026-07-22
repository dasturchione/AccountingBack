namespace Application.Features.Platform
{
    public class PlatformUserBaseDto
    {
        public string UserName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Email { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public int RoleId { get; set; }
        public short? LanguageId { get; set; }
        public bool EmailVerified { get; set; }
        public bool IsPlatformAdmin { get; set; }
        public string? Timezone { get; set; }
    }
}
