namespace Application.Features.FaDisposals;

public partial class FaDisposalBaseDto
{
    public DateTime DisposalDate { get; set; }
    public short DisposalTypeId { get; set; }
    public string? Reason { get; set; }
    public short StateId { get; set; } = SharedKernel.Constants.StateIdConst.ACTIVE;
    public List<FaDisposalLineWriteDto> Lines { get; set; } = new();
}

public sealed class FaDisposalCreateDto : FaDisposalBaseDto;
public sealed class FaDisposalUpdateDto : FaDisposalBaseDto;

public partial class FaDisposalLineWriteDto
{
    public long FaAssetId { get; set; }
    public decimal SaleAmount { get; set; }
    public string? Note { get; set; }
}

public partial class FaDisposalDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string DocNumber { get; set; } = null!;
    public DateTime DisposalDate { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public short DisposalTypeId { get; set; }
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
    public decimal TotalBookValue { get; set; }
    public decimal TotalSaleAmount { get; set; }
    public decimal TotalGainLoss { get; set; }
    public List<FaDisposalLineDto> Lines { get; set; } = new();
}

public sealed class FaDisposalListDto : FaDisposalDto;

public partial class FaDisposalLineDto
{
    public long Id { get; set; }
    public long DisposalDocId { get; set; }
    public long FaAssetId { get; set; }
    public string InventoryNumber { get; set; } = null!;
    public string AssetName { get; set; } = null!;
    public decimal BookValue { get; set; }
    public decimal SaleAmount { get; set; }
    public decimal GainLoss { get; set; }
    public string? Note { get; set; }
}
