namespace Application.Features.FaMovements;

public class FaMovementBaseDto
{
    public DateTime DocDate { get; set; }
    public int? ToDepartmentId { get; set; }
    public int? ToResponsibleUserId { get; set; }
    public string? Note { get; set; }
    public List<FaMovementLineWriteDto> Lines { get; set; } = new();
}

public class FaMovementLineWriteDto
{
    public long FaAssetId { get; set; }
    public string? Note { get; set; }
}
