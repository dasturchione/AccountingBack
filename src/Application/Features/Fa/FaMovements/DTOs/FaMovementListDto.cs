namespace Application.Features.FaMovements;

public class FaMovementListDto
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
    public DateTime UpdatedDate { get; set; }
}
