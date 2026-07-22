namespace Application.Features.Platform
{
    public class PlatformTenantUpdateDto : PlatformTenantBaseDto
    {
        public int? OwnerUserId { get; set; }

        public short StateId { get; set; }
    }
}
