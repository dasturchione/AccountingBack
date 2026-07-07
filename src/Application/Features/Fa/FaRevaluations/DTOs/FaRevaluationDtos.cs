namespace Application.Features.FaRevaluations;

public class FaRevaluationBaseDto
{
    public DateTime RevaluationDate { get; set; }
    public string? Reason { get; set; }
    public short StateId { get; set; } = SharedKernel.Constants.StateIdConst.ACTIVE;
    public List<FaRevaluationLineWriteDto> Lines { get; set; } = new();
}

public sealed class FaRevaluationCreateDto : FaRevaluationBaseDto;
public sealed class FaRevaluationUpdateDto : FaRevaluationBaseDto;

public class FaRevaluationLineWriteDto
{
    public long FaAssetId { get; set; }
    public decimal NewValue { get; set; }
    public string? Note { get; set; }
}

public class FaRevaluationDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string DocNumber { get; set; } = null!;
    public DateTime RevaluationDate { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public string? Reason { get; set; }
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
    public decimal TotalRevaluationAmount { get; set; }
    public List<FaRevaluationLineDto> Lines { get; set; } = new();
}

public sealed class FaRevaluationListDto : FaRevaluationDto;

public class FaRevaluationLineDto
{
    public long Id { get; set; }
    public long RevaluationDocId { get; set; }
    public long FaAssetId { get; set; }
    public string InventoryNumber { get; set; } = null!;
    public string AssetName { get; set; } = null!;
    public decimal OldValue { get; set; }
    public decimal NewValue { get; set; }
    public decimal RevaluationAmount { get; set; }
    public string? Note { get; set; }
}
