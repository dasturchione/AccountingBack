using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class OrgOrganization
{
    public int Id { get; set; }

    public string ShortName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Inn { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public int RegionId { get; set; }

    public int? DistrictId { get; set; }

    public string? Address { get; set; }

    public string? Director { get; set; }

    public bool IsParent { get; set; }

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public short? DefaultLanguageId { get; set; }

    public virtual ICollection<AccChartAccountSubkonto> AccChartAccountSubkontos { get; set; } = new List<AccChartAccountSubkonto>();

    public virtual ICollection<AccChartAccount> AccChartAccounts { get; set; } = new List<AccChartAccount>();

    public virtual ICollection<AccPostingRule> AccPostingRules { get; set; } = new List<AccPostingRule>();

    public virtual ICollection<AccRegEntry> AccRegEntries { get; set; } = new List<AccRegEntry>();

    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    public virtual ICollection<CashBox> CashBoxes { get; set; } = new List<CashBox>();

    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();

    public virtual ICollection<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; } = new List<CounterpartyBankAccount>();

    public virtual ICollection<CounterpartyCard> CounterpartyCards { get; set; } = new List<CounterpartyCard>();

    public virtual ICollection<CounterpartyContact> CounterpartyContacts { get; set; } = new List<CounterpartyContact>();

    public virtual ICollection<CounterpartyRegBalance> CounterpartyRegBalances { get; set; } = new List<CounterpartyRegBalance>();

    public virtual CmnLanguage? DefaultLanguage { get; set; }

    public virtual CmnDistrict? District { get; set; }

    public virtual ICollection<InvProductGroup> InvProductGroups { get; set; } = new List<InvProductGroup>();

    public virtual ICollection<InvProductPrice> InvProductPrices { get; set; } = new List<InvProductPrice>();

    public virtual ICollection<InvProduct> InvProducts { get; set; } = new List<InvProduct>();

    public virtual ICollection<InvRegBalance> InvRegBalances { get; set; } = new List<InvRegBalance>();

    public virtual ICollection<InvWarehouse> InvWarehouses { get; set; } = new List<InvWarehouse>();

    public virtual ICollection<MoneyRegBalance> MoneyRegBalances { get; set; } = new List<MoneyRegBalance>();

    public virtual ICollection<OrgBankAccount> OrgBankAccounts { get; set; } = new List<OrgBankAccount>();

    public virtual ICollection<OrgBranch> OrgBranches { get; set; } = new List<OrgBranch>();

    public virtual ICollection<OrgDepartment> OrgDepartments { get; set; } = new List<OrgDepartment>();

    public virtual ICollection<OrgPosition> OrgPositions { get; set; } = new List<OrgPosition>();

    public virtual ICollection<PurDoc> PurDocs { get; set; } = new List<PurDoc>();

    public virtual CmnRegion Region { get; set; } = null!;

    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    public virtual CmnState State { get; set; } = null!;
}
