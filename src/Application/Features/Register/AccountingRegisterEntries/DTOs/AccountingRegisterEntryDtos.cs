namespace Application.Features.AccountingRegisterEntries;

public class AccountingRegisterEntryBaseDto
{
    public int OrganizationId { get; set; }
    public short DocumentTypeId { get; set; }
    public long DocumentId { get; set; }
    public int? DebitAccountId { get; set; }
    public int? CreditAccountId { get; set; }
    public short CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public DateTime DocDate { get; set; }
}

public class AccountingRegisterEntryCreateDto : AccountingRegisterEntryBaseDto { }

public class AccountingRegisterEntryUpdateDto : AccountingRegisterEntryBaseDto { }

public class AccountingRegisterEntryDto : AccountingRegisterEntryBaseDto
{
    public long Id { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class AccountingRegisterEntryListDto : AccountingRegisterEntryDto { }
