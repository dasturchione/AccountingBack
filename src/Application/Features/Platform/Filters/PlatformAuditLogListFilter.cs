using SharedKernel.Filters;

namespace Application.Features.Platform.Filters
{
    public class PlatformAuditLogListFilter : IPaginationFilter
    {
        public int? OrganizationId { get; set; }
        public int? UserId { get; set; }
        public int? ChangedUserId { get; set; }
        public string? EntityType { get; set; }
        public string? TableName { get; set; }
        public string? EntityId { get; set; }
        public string? RecordId { get; set; }
        public string? Action { get; set; }
        public string? SearchText { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int? PageSize { get; set; } = 50;
    }
}
