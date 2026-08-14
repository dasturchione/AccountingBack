namespace SharedKernel.Constants;

public static class FaDocumentAccountTypeCodeConst
{
    public const string Receipt = "fa_receipt";
    public const string Commissioning = "fa_commissioning";
    public const string Depreciation = "fa_depreciation";
    public const string Revaluation = "fa_revaluation";
    public const string Disposal = "fa_disposal";

    public static string? FromDocumentTypeId(short documentTypeId) =>
        documentTypeId switch
        {
            DocumentTypeIdConst.FARECEIPT => Receipt,
            DocumentTypeIdConst.FACOMMISSIONING => Commissioning,
            DocumentTypeIdConst.FADEPRECIATION => Depreciation,
            DocumentTypeIdConst.FAREVALUATION => Revaluation,
            DocumentTypeIdConst.FADISPOSAL => Disposal,
            _ => null
        };
}