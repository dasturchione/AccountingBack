namespace Application.Features.Manual;

public class SelectListDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
}

public class ProductSelectListDto : SelectListDto
{
    public string? Mxik { get; set; }

    public short UnitId { get; set; }
    
    public string? UnitCode { get; set; }

    public bool IsPieceTracked { get; set; }

    public bool IsService { get; set; }

    public bool IsSold { get; set; }

    public bool IsPurchased { get; set; }
}
