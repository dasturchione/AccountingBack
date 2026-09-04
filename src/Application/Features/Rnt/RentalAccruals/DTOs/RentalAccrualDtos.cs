using Application.Features.Rnt.RentalContracts;

namespace Application.Features.Rnt.RentalAccruals;

public sealed class RentalAccrualDocItemDto
{
    public long Id { get; set; }
    public long ContractObjectId { get; set; }
    public string ObjectName { get; set; } = null!;
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
    public decimal ContractAmount { get; set; }
    public decimal TaxBaseAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal PayableAmount { get; set; }
    public decimal Amount { get; set; }
    public int? ExpenseAccountId { get; set; }
    public string? ExpenseAccountNumber { get; set; }
    public string? ExpenseAccountName { get; set; }
}

public sealed class RentalAccrualDocDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public long ContractId { get; set; }
    public string ContractNumber { get; set; } = null!;
    public List<RentalLessorDto> Lessors { get; set; } = [];
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public short CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public decimal ExchangeRate { get; set; }
    public decimal ContractAmount { get; set; }
    public decimal TaxBaseAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal PayableAmount { get; set; }
    public decimal Amount { get; set; }
    public int? LessorPayableAccountId { get; set; }
    public string? LessorPayableAccountNumber { get; set; }
    public string? LessorPayableAccountName { get; set; }
    public int? TaxPayableAccountId { get; set; }
    public string? TaxPayableAccountNumber { get; set; }
    public string? TaxPayableAccountName { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public string? Comment { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? PostedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public List<RentalAccrualDocItemDto> Items { get; set; } = [];
}

public sealed class RentalAccrualDocListDto
{
    public long Id { get; set; }
    public long ContractId { get; set; }
    public string ContractNumber { get; set; } = null!;
    public List<RentalLessorDto> Lessors { get; set; } = [];
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public decimal TaxAmount { get; set; }
    public decimal PayableAmount { get; set; }
    public decimal Amount { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
}

public sealed class RentalAccrualItemAccountDto
{
    public long ItemId { get; set; }
    public int ExpenseAccountId { get; set; }
}

public sealed class RentalAccrualUpdateDto
{
    public decimal ExchangeRate { get; set; } = 1m;
    public int LessorPayableAccountId { get; set; }
    public int TaxPayableAccountId { get; set; }
    public string? Comment { get; set; }
    public List<RentalAccrualItemAccountDto> Items { get; set; } = [];
}

public sealed class RentalAccrualGenerateDueDto
{
    public DateTime? AsOfDate { get; set; }
}

public sealed record RentalAccrualGenerationResult(int CreatedDocumentCount, int CreatedItemCount, IReadOnlyList<long> DocumentIds);
