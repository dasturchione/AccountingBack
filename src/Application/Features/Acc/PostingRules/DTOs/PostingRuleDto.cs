namespace Application.Features.Acc.PostingRules
{
    public class PostingRuleDto
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
        public List<PostingRuleLineDto> Lines { get; set; } = new List<PostingRuleLineDto>();
    }

    public class PostingRuleLineDto
    {
        public int Id { get; set; }
        public int SortOrder { get; set; }
        public int? DebitAccountId { get; set; }
        public int? CreditAccountId { get; set; }
        public string AmountSource { get; set; } = null!;
        public string? QuantitySource { get; set; }
        public string? ContentTemplate { get; set; }
        public short StateId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreditAccountCode { get; set; }
        public string? CreditAccountName { get; set; }
        public string? DebitAccountCode { get; set; }
        public string? DebitAccountName { get; set; }
        public string StateName { get; set; } = null!;
    }
}
