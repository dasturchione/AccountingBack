namespace Application.Features.FaDepreciations;

public sealed class FaDepreciationRunRequestDto
{
    public string Period { get; set; } = null!;
}

public class FaDepreciationRunDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string DocNumber { get; set; } = null!;
    public DateTime PeriodMonth { get; set; }
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
    public decimal TotalAmount { get; set; }
    public List<FaDepreciationRunLineDto> Lines { get; set; } = new();
}

public sealed class FaDepreciationRunListDto : FaDepreciationRunDto;

public class FaDepreciationRunLineDto
{
    public long Id { get; set; }
    public long DepreciationRunId { get; set; }
    public long FaAssetId { get; set; }
    public string InventoryNumber { get; set; } = null!;
    public string AssetName { get; set; } = null!;
    public short DepreciationMethodId { get; set; }
    public string DepreciationMethodCode { get; set; } = null!;
    public string DepreciationMethodName { get; set; } = null!;
    public decimal Amount { get; set; }
    public string? Note { get; set; }
}
