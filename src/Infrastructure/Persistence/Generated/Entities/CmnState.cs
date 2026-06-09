using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class CmnState
{
    public short Id { get; set; }

    public string ShortName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<AccAccountType> AccAccountTypes { get; set; } = new List<AccAccountType>();

    public virtual ICollection<AccChartAccountSubkonto> AccChartAccountSubkontos { get; set; } = new List<AccChartAccountSubkonto>();

    public virtual ICollection<AccChartAccount> AccChartAccounts { get; set; } = new List<AccChartAccount>();

    public virtual ICollection<AccPostingRuleLine> AccPostingRuleLines { get; set; } = new List<AccPostingRuleLine>();

    public virtual ICollection<AccPostingRule> AccPostingRules { get; set; } = new List<AccPostingRule>();

    public virtual ICollection<AccSubkontoType> AccSubkontoTypes { get; set; } = new List<AccSubkontoType>();

    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    public virtual ICollection<CashBox> CashBoxes { get; set; } = new List<CashBox>();

    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();

    public virtual ICollection<CmnBank> CmnBanks { get; set; } = new List<CmnBank>();

    public virtual ICollection<CmnCounterpartyType> CmnCounterpartyTypes { get; set; } = new List<CmnCounterpartyType>();

    public virtual ICollection<CmnCurrency> CmnCurrencies { get; set; } = new List<CmnCurrency>();

    public virtual ICollection<CmnDocumentStatus> CmnDocumentStatuses { get; set; } = new List<CmnDocumentStatus>();

    public virtual ICollection<CmnDocumentType> CmnDocumentTypes { get; set; } = new List<CmnDocumentType>();

    public virtual ICollection<CmnLanguage> CmnLanguages { get; set; } = new List<CmnLanguage>();

    public virtual ICollection<CmnOperationType> CmnOperationTypes { get; set; } = new List<CmnOperationType>();

    public virtual ICollection<CmnPaymentType> CmnPaymentTypes { get; set; } = new List<CmnPaymentType>();

    public virtual ICollection<CmnRegion> CmnRegions { get; set; } = new List<CmnRegion>();

    public virtual ICollection<CmnTaxType> CmnTaxTypes { get; set; } = new List<CmnTaxType>();

    public virtual ICollection<CmnUnit> CmnUnits { get; set; } = new List<CmnUnit>();

    public virtual ICollection<CmnVatRate> CmnVatRates { get; set; } = new List<CmnVatRate>();

    public virtual ICollection<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; } = new List<CounterpartyBankAccount>();

    public virtual ICollection<CounterpartyCard> CounterpartyCards { get; set; } = new List<CounterpartyCard>();

    public virtual ICollection<CounterpartyContact> CounterpartyContacts { get; set; } = new List<CounterpartyContact>();

    public virtual ICollection<InvProductGroup> InvProductGroups { get; set; } = new List<InvProductGroup>();

    public virtual ICollection<InvProductPrice> InvProductPrices { get; set; } = new List<InvProductPrice>();

    public virtual ICollection<InvProduct> InvProducts { get; set; } = new List<InvProduct>();

    public virtual ICollection<InvWarehouse> InvWarehouses { get; set; } = new List<InvWarehouse>();

    public virtual ICollection<OrgBankAccount> OrgBankAccounts { get; set; } = new List<OrgBankAccount>();

    public virtual ICollection<OrgBranch> OrgBranches { get; set; } = new List<OrgBranch>();

    public virtual ICollection<OrgDepartment> OrgDepartments { get; set; } = new List<OrgDepartment>();

    public virtual ICollection<OrgOrganization> OrgOrganizations { get; set; } = new List<OrgOrganization>();

    public virtual ICollection<OrgPosition> OrgPositions { get; set; } = new List<OrgPosition>();

    public virtual ICollection<PurDoc> PurDocs { get; set; } = new List<PurDoc>();

    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    public virtual ICollection<SysModule> SysModules { get; set; } = new List<SysModule>();

    public virtual ICollection<SysRole> SysRoles { get; set; } = new List<SysRole>();

    public virtual ICollection<SysUser> SysUsers { get; set; } = new List<SysUser>();
}
