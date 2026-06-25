namespace Application.Features.OrgBankAccounts;

public class OrgBankAccountCreateManyDto
{
    public List<OrgBankAccountCreateDto> Accounts { get; set; } = new();
}
