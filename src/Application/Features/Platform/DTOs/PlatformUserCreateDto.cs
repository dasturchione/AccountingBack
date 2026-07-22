namespace Application.Features.Platform
{
    public class PlatformUserCreateDto : PlatformUserBaseDto
    {
        public string Password { get; set; } = null!;
    }
}
