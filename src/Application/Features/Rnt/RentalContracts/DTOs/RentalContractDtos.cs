namespace Application.Features.Rnt.RentalContracts;

public sealed class RentalContractObjectInputDto
{
    public long? Id { get; set; }
    public short RentalObjectTypeId { get; set; }
    public string ObjectName { get; set; } = null!;
    public string? ObjectIdentifier { get; set; }
    public string? ObjectAddress { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string PeriodUnit { get; set; } = "MONTH";
    public int PeriodValue { get; set; } = 1;
    public decimal ContractAmount { get; set; }
    public decimal TaxBaseAmount { get; set; }
    public decimal TaxRate { get; set; }
    public int? ExpenseAccountId { get; set; }
}

public class RentalContractBaseDto
{
    public string LessorFullName { get; set; } = null!;
    public string? LessorInn { get; set; }
    public string? LessorPinfl { get; set; }
    public string ContractNumber { get; set; } = null!;
    public DateTime ContractDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public short CurrencyId { get; set; }
    public int? LessorPayableAccountId { get; set; }
    public int? TaxPayableAccountId { get; set; }
    public string? Comment { get; set; }
    public List<RentalContractObjectInputDto> Objects { get; set; } = [];
}

public sealed class RentalContractCreateDto : RentalContractBaseDto;
public sealed class RentalContractUpdateDto : RentalContractBaseDto;

public sealed class RentalContractObjectDto
{
    public long Id { get; set; }
    public short RentalObjectTypeId { get; set; }
    public string RentalObjectTypeCode { get; set; } = null!;
    public string RentalObjectTypeName { get; set; } = null!;
    public string ObjectName { get; set; } = null!;
    public string? ObjectIdentifier { get; set; }
    public string? ObjectAddress { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string PeriodUnit { get; set; } = null!;
    public int PeriodValue { get; set; }
    public DateTime NextAccrualDate { get; set; }
    public decimal ContractAmount { get; set; }
    public decimal TaxBaseAmount { get; set; }
    public decimal TaxRate { get; set; }
    public int? ExpenseAccountId { get; set; }
    public string? ExpenseAccountNumber { get; set; }
    public string? ExpenseAccountName { get; set; }
}

public class RentalContractDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string LessorFullName { get; set; } = null!;
    public string? LessorInn { get; set; }
    public string? LessorPinfl { get; set; }
    public string ContractNumber { get; set; } = null!;
    public DateTime ContractDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public short CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = null!;
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
    public List<RentalContractObjectDto> Objects { get; set; } = [];
}

public sealed class RentalContractListDto
{
    public long Id { get; set; }
    public string ContractNumber { get; set; } = null!;
    public DateTime ContractDate { get; set; }
    public string LessorFullName { get; set; } = null!;
    public string? LessorInn { get; set; }
    public string? LessorPinfl { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public short CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public int ObjectCount { get; set; }
}
