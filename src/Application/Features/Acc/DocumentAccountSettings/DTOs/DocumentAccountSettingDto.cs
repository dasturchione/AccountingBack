namespace Application.Features.Acc.DocumentAccountSettings
{
    public class DocumentAccountSettingDto
    {
        public short DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; } = null!;
        public string DocumentTypeCode { get; set; } = null!;
        public string DocumentTypeDescription { get; set; } = null!;

        public List<DocumentAccountRuleDto> AccountSettings { get; set; } = new();
    }

    public class DocumentAccountRuleDto
    {
        public short DocumentAccountRoleId { get; set; }
        public int DocumentAccountTypeRoleId { get; set; }
        public string DocumentAccountRoleName { get; set; } = null!;
        public string DocumentAccountRoleCode { get; set; } = null!;
        public string DocumentAccountRoleDescription { get; set; } = null!;
        public string AccountSide { get; set; } = null!;
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }

        public List<DocumentAccountOptionDto> Accounts { get; set; } = new();
    }

    public class DocumentAccountOptionDto
    {
        public long Id { get; set; }
        public int OrganizationId { get; set; }
        public int ChartAccountId { get; set; }
        public string ChartAccountName { get; set; } = null!;
        public string ChartAccountCode { get; set; } = null!;
        public string ChartAccountNumber { get; set; } = null!;
        public bool IsDefault { get; set; }
        public bool CanChange { get; set; }
        public int SortOrder { get; set; }
        public int StateId { get; set; }
        public string StateName { get; set; } = null!;
    }
}
