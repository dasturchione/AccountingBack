namespace Application.Features.Rnt.RentalContracts;

public sealed class RentalLessorInputDto
{
    public string LessorKindCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Inn { get; set; }
    public string? Pinfl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? RegisteredAddress { get; set; }
    public string? ResidentialAddress { get; set; }
}

public sealed class RentalContractObjectUtilityInputDto
{
    public short UtilityServiceId { get; set; }
    public string PayerCode { get; set; } = null!;
}

public sealed class RentalContractObjectInputDto
{
    public long? Id { get; set; }
    public short RentalObjectTypeId { get; set; }
    public string ObjectName { get; set; } = null!;
    public string? ObjectIdentifier { get; set; }
    public string? ObjectAddress { get; set; }
    public decimal? TotalArea { get; set; }
    public decimal? RentedArea { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string PeriodUnit { get; set; } = "MONTH";
    public decimal PeriodAmount { get; set; }
    public decimal TaxBaseAmount { get; set; }
    public decimal TaxRate { get; set; }
    public int? ExpenseAccountId { get; set; }
    public List<RentalContractObjectUtilityInputDto> Utilities { get; set; } = [];
}

public class RentalContractBaseDto
{
    public bool IsFreeOfCharge { get; set; }
    public string ContractNumber { get; set; } = null!;
    public DateTime ContractDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public short CurrencyId { get; set; }
    public int? LessorPayableAccountId { get; set; }
    public int? TaxPayableAccountId { get; set; }
    public string? Comment { get; set; }
    public List<RentalLessorInputDto> Lessors { get; set; } = [];
    public List<RentalContractObjectInputDto> Objects { get; set; } = [];
}

public sealed class RentalContractCreateDto : RentalContractBaseDto;
public sealed class RentalContractUpdateDto : RentalContractBaseDto;

public sealed class RentalLessorDto
{
    public long Id { get; set; }
    public string LessorKindCode { get; set; } = null!;
    public int? CounterpartyId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Inn { get; set; }
    public string? Pinfl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? RegisteredAddress { get; set; }
    public string? ResidentialAddress { get; set; }
}

public sealed class RentalContractObjectUtilityDto
{
    public long Id { get; set; }
    public short UtilityServiceId { get; set; }
    public string UtilityServiceCode { get; set; } = null!;
    public string UtilityServiceName { get; set; } = null!;
    public string PayerCode { get; set; } = null!;
}

public sealed class RentalContractObjectDto
{
    public long Id { get; set; }
    public short RentalObjectTypeId { get; set; }
    public string RentalObjectTypeCode { get; set; } = null!;
    public string RentalObjectTypeName { get; set; } = null!;
    public string ObjectName { get; set; } = null!;
    public string? ObjectIdentifier { get; set; }
    public string? ObjectAddress { get; set; }
    public decimal? TotalArea { get; set; }
    public decimal? RentedArea { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string PeriodUnit { get; set; } = null!;
    public DateTime NextAccrualDate { get; set; }
    public decimal PeriodAmount { get; set; }
    public decimal TaxBaseAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal? ContractAmount { get; set; }
    public decimal? ContractTaxBaseAmount { get; set; }
    public decimal? ContractTaxAmount { get; set; }
    public int? ExpenseAccountId { get; set; }
    public string? ExpenseAccountNumber { get; set; }
    public string? ExpenseAccountName { get; set; }
    public List<RentalContractObjectUtilityDto> Utilities { get; set; } = [];
}

public class RentalContractDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public bool IsFreeOfCharge { get; set; }
    public string ContractNumber { get; set; } = null!;
    public DateTime ContractDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ConfirmationDate { get; set; }
    public DateTime? TerminationDate { get; set; }
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

    public decimal TotalContractAmount { get => Objects.Sum(s => s.ContractAmount ?? 0); }

    public List<RentalLessorDto> Lessors { get; set; } = [];
    public List<RentalContractObjectDto> Objects { get; set; } = [];
}

public sealed class RentalContractListDto
{
    public long Id { get; set; }
    public string ContractNumber { get; set; } = null!;
    public DateTime ContractDate { get; set; }
    public bool IsFreeOfCharge { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ConfirmationDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public short CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public int ObjectCount { get; set; }
    public List<RentalLessorDto> Lessors { get; set; } = [];
}
