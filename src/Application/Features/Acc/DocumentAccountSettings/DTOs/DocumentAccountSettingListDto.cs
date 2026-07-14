namespace Application.Features.Acc.DocumentAccountSettings
{
    public class DocumentAccountSettingListDto
    {
        public short DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; } = null!;
        public string DocumentTypeCode { get; set; } = null!;
        public string DocumentTypeDescription { get; set; } = null!;
        public short StateId { get; set; }
        public string StateName { get; set; } = null!;
    }
}
