namespace Application.Features.FaMovements;

public class FaMovementDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public int? FromDepartmentId { get; set; }
    public string? FromDepartmentName { get; set; }
    public int? ToDepartmentId { get; set; }
    public string? ToDepartmentName { get; set; }
    public int? FromResponsibleUserId { get; set; }
    public string? FromResponsibleUserName { get; set; }
    public int? ToResponsibleUserId { get; set; }
    public string? ToResponsibleUserName { get; set; }
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
    public List<FaMovementLineDto> Lines { get; set; } = new();
}

public class FaMovementLineDto
{
    public long Id { get; set; }
    public long MovementDocId { get; set; }
    public long FaAssetId { get; set; }
    public string InventoryNumber { get; set; } = null!;
    public string AssetName { get; set; } = null!;
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? ResponsibleUserId { get; set; }
    public string? ResponsibleUserName { get; set; }
    public short AssetStatusId { get; set; }
    public string AssetStatusName { get; set; } = null!;
    public string? Note { get; set; }
}
