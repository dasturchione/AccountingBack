namespace Application.Features.Platform
{
    public class PlatformTenantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public int? OwnerUserId { get; set; }
        public string? OwnerUserName { get; set; }
        public short StateId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int OrganizationsCount { get; set; }
        public int UsersCount { get; set; }
    }
}
