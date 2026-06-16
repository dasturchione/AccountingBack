namespace Application.Features.Acc.PostingRules
{
    public class PostingRuleListDto
    {
        public int Id { get; set; }
        public int? OrganizationId { get; set; }
        public short DocumentTypeId { get; set; }
        public short? OperationTypeId { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public short StateId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string DocumentTypeName { get; set; } = null!;
        public string? OperationTypeName { get; set; }
        public string? OrganizationName { get; set; }
        public string StateName { get; set; } = null!;
    }
}
