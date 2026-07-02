namespace Application.Features.Acc.PostingTemplateViews;

public class PostingTemplateViewListDto
{
    public short Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public short DocumentTypeId { get; set; }
    public string DocumentTypeCode { get; set; } = null!;
    public string DocumentTypeName { get; set; } = null!;
    public int LinesCount { get; set; }
}

public class PostingTemplateViewDto : PostingTemplateViewListDto
{
    public short PolicyId { get; set; }
    public List<PostingTemplateLineViewDto> Lines { get; set; } = new();
}

public class PostingTemplateLineViewDto
{
    public int Id { get; set; }
    public short OrderNumber { get; set; }
    public short DebitAliasId { get; set; }
    public string DebitAliasCode { get; set; } = null!;
    public string DebitAliasName { get; set; } = null!;
    public short CreditAliasId { get; set; }
    public string CreditAliasCode { get; set; } = null!;
    public string CreditAliasName { get; set; } = null!;
    public string? AmountSource { get; set; }
    public bool IsOptional { get; set; }
    public List<AccountAliasResolveDto> DebitResolveRules { get; set; } = new();
    public List<AccountAliasResolveDto> CreditResolveRules { get; set; } = new();
}

public class AccountAliasResolveDto
{
    public int Id { get; set; }
    public short PolicyId { get; set; }
    public string Alias { get; set; } = null!;
    public string DimensionKey { get; set; } = null!;
    public string DimensionValue { get; set; } = null!;
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public int Priority { get; set; }
}
