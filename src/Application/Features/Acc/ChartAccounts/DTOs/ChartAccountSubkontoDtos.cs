namespace Application.Features.ChartAccounts;

public class ChartAccountSubkontoDto
{
    public int Id { get; set; }
    public short SubkontoTypeId { get; set; }
    public string? SubkontoTypeCode { get; set; }
    public string SubkontoTypeName { get; set; } = null!;
    public int SortOrder { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}

public class ChartAccountSubkontoUpsertDto
{
    public short SubkontoTypeId { get; set; }
    public int SortOrder { get; set; }
}
