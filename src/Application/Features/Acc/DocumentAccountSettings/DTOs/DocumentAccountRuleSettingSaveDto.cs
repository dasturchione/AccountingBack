namespace Application.Features.Acc.DocumentAccountSettings
{
    public class DocumentAccountRuleSettingSaveDto
    {
        public List<DocumentTypeAccountSettingSaveDto> Items { get; set; } = new();
    }

    public class DocumentTypeAccountSettingSaveDto
    {
        public int DocumentAccountTypeRoleId { get; set; }

        public List<DocumentAccountSettingSaveDto> Accounts { get; set; } = new();
    }

    public class DocumentAccountSettingSaveDto
    {
        public int ChartAccountId { get; set; }
        public bool IsDefault { get; set; }
        public bool CanChange { get; set; }
        public int SortOrder { get; set; }
    }
}
