namespace Application.Features.FaCommissionings;

public class FaCommissioningBaseDto
{
    public DateTime DocDate { get; set; }
    public string? Note { get; set; }
    public List<FaCommissioningLineWriteDto> Lines { get; set; } = new();
}

public sealed class FaCommissioningCreateDto : FaCommissioningBaseDto;
public sealed class FaCommissioningUpdateDto : FaCommissioningBaseDto;

public class FaCommissioningLineWriteDto
{
    public long FaAssetId { get; set; }
    public DateTime DeprStartDate { get; set; }
    public decimal SalvageValue { get; set; }
    public int UsefulLifeMonths { get; set; }
    public short DepreciationMethodId { get; set; }
    public decimal? PlannedUnitsTotal { get; set; }
    public int? DepartmentId { get; set; }
    public int? ResponsibleUserId { get; set; }
    public int AccumulatedDepreciationAccountId { get; set; }
    public int DepreciationExpenseAccountId { get; set; }
    public string? Note { get; set; }
}

public class FaCommissioningDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public string? Note { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime UpdatedDate { get; set; }
    public int? UpdatedByUserId { get; set; }
    public DateTime? PostedAt { get; set; }
    public int? PostedByUserId { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledByUserId { get; set; }
    public decimal TotalCapitalizedAmount { get; set; }
    public List<FaCommissioningLineDto> Lines { get; set; } = new();
}

public class FaCommissioningListDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public string? Note { get; set; }
    public int AssetCount { get; set; }
    public decimal TotalCapitalizedAmount { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime UpdatedDate { get; set; }
}

public class FaCommissioningLineDto
{
    public long Id { get; set; }
    public long CommissioningDocId { get; set; }
    public long FaAssetId { get; set; }
    public string InventoryNumber { get; set; } = null!;
    public string AssetName { get; set; } = null!;
    public decimal CapitalizedAmount { get; set; }
    public DateTime DeprStartDate { get; set; }
    public decimal SalvageValue { get; set; }
    public int UsefulLifeMonths { get; set; }
    public short DepreciationMethodId { get; set; }
    public string DepreciationMethodCode { get; set; } = null!;
    public string DepreciationMethodName { get; set; } = null!;
    public decimal? PlannedUnitsTotal { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? ResponsibleUserId { get; set; }
    public string? ResponsibleUserName { get; set; }
    public int AssetAccountId { get; set; }
    public string AssetAccountNumber { get; set; } = null!;
    public string AssetAccountName { get; set; } = null!;
    public int CapitalInvestmentAccountId { get; set; }
    public string CapitalInvestmentAccountNumber { get; set; } = null!;
    public string CapitalInvestmentAccountName { get; set; } = null!;
    public int AccumulatedDepreciationAccountId { get; set; }
    public string AccumulatedDepreciationAccountNumber { get; set; } = null!;
    public string AccumulatedDepreciationAccountName { get; set; } = null!;
    public int DepreciationExpenseAccountId { get; set; }
    public string DepreciationExpenseAccountNumber { get; set; } = null!;
    public string DepreciationExpenseAccountName { get; set; } = null!;
    public string? Note { get; set; }
}
