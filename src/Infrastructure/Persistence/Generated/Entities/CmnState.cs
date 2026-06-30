using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_state")]
public partial class CmnState
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("short_name")]
    [StringLength(250)]
    public string ShortName { get; set; } = null!;

    [Column("full_name")]
    [StringLength(250)]
    public string FullName { get; set; } = null!;

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("State")]
    public virtual ICollection<AccAccountType> AccAccountTypes { get; set; } = new List<AccAccountType>();

    [InverseProperty("State")]
    public virtual ICollection<AccAccountingPolicy> AccAccountingPolicies { get; set; } = new List<AccAccountingPolicy>();

    [InverseProperty("State")]
    public virtual ICollection<AccChartAccountSubkonto> AccChartAccountSubkontos { get; set; } = new List<AccChartAccountSubkonto>();

    [InverseProperty("State")]
    public virtual ICollection<AccChartAccount> AccChartAccounts { get; set; } = new List<AccChartAccount>();

    [InverseProperty("State")]
    public virtual ICollection<AccSubkontoType> AccSubkontoTypes { get; set; } = new List<AccSubkontoType>();

    [InverseProperty("State")]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    [InverseProperty("State")]
    public virtual ICollection<CashBox> CashBoxes { get; set; } = new List<CashBox>();

    [InverseProperty("State")]
    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();

    [InverseProperty("State")]
    public virtual ICollection<CmnBank> CmnBanks { get; set; } = new List<CmnBank>();

    [InverseProperty("State")]
    public virtual ICollection<CmnContractType> CmnContractTypes { get; set; } = new List<CmnContractType>();

    [InverseProperty("State")]
    public virtual ICollection<CmnContract> CmnContracts { get; set; } = new List<CmnContract>();

    [InverseProperty("State")]
    public virtual ICollection<CmnCounterpartyType> CmnCounterpartyTypes { get; set; } = new List<CmnCounterpartyType>();

    [InverseProperty("State")]
    public virtual ICollection<CmnCurrency> CmnCurrencies { get; set; } = new List<CmnCurrency>();

    [InverseProperty("State")]
    public virtual ICollection<CmnDocumentSequence> CmnDocumentSequences { get; set; } = new List<CmnDocumentSequence>();

    [InverseProperty("State")]
    public virtual ICollection<CmnDocumentStatus> CmnDocumentStatuses { get; set; } = new List<CmnDocumentStatus>();

    [InverseProperty("State")]
    public virtual ICollection<CmnDocumentType> CmnDocumentTypes { get; set; } = new List<CmnDocumentType>();

    [InverseProperty("State")]
    public virtual ICollection<CmnLanguage> CmnLanguages { get; set; } = new List<CmnLanguage>();

    [InverseProperty("State")]
    public virtual ICollection<CmnOperationType> CmnOperationTypes { get; set; } = new List<CmnOperationType>();

    [InverseProperty("State")]
    public virtual ICollection<CmnPaymentType> CmnPaymentTypes { get; set; } = new List<CmnPaymentType>();

    [InverseProperty("State")]
    public virtual ICollection<CmnPricingCondition> CmnPricingConditions { get; set; } = new List<CmnPricingCondition>();

    [InverseProperty("State")]
    public virtual ICollection<CmnProductTableStatus> CmnProductTableStatuses { get; set; } = new List<CmnProductTableStatus>();

    [InverseProperty("State")]
    public virtual ICollection<CmnRegion> CmnRegions { get; set; } = new List<CmnRegion>();

    [InverseProperty("State")]
    public virtual ICollection<CmnTaxType> CmnTaxTypes { get; set; } = new List<CmnTaxType>();

    [InverseProperty("State")]
    public virtual ICollection<CmnUnit> CmnUnits { get; set; } = new List<CmnUnit>();

    [InverseProperty("State")]
    public virtual ICollection<CmnVatRate> CmnVatRates { get; set; } = new List<CmnVatRate>();

    [InverseProperty("State")]
    public virtual ICollection<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; } = new List<CounterpartyBankAccount>();

    [InverseProperty("State")]
    public virtual ICollection<CounterpartyCard> CounterpartyCards { get; set; } = new List<CounterpartyCard>();

    [InverseProperty("State")]
    public virtual ICollection<CounterpartyContact> CounterpartyContacts { get; set; } = new List<CounterpartyContact>();

    [InverseProperty("State")]
    public virtual ICollection<InvProductGroup> InvProductGroups { get; set; } = new List<InvProductGroup>();

    [InverseProperty("State")]
    public virtual ICollection<InvProductPrice> InvProductPrices { get; set; } = new List<InvProductPrice>();

    [InverseProperty("State")]
    public virtual ICollection<InvProductTable> InvProductTables { get; set; } = new List<InvProductTable>();

    [InverseProperty("State")]
    public virtual ICollection<InvProduct> InvProducts { get; set; } = new List<InvProduct>();

    [InverseProperty("State")]
    public virtual ICollection<InvWarehouse> InvWarehouses { get; set; } = new List<InvWarehouse>();

    [InverseProperty("State")]
    public virtual ICollection<OrgBankAccount> OrgBankAccounts { get; set; } = new List<OrgBankAccount>();

    [InverseProperty("State")]
    public virtual ICollection<OrgBranch> OrgBranches { get; set; } = new List<OrgBranch>();

    [InverseProperty("State")]
    public virtual ICollection<OrgDepartment> OrgDepartments { get; set; } = new List<OrgDepartment>();

    [InverseProperty("State")]
    public virtual ICollection<OrgOrganization> OrgOrganizations { get; set; } = new List<OrgOrganization>();

    [InverseProperty("State")]
    public virtual ICollection<OrgPosition> OrgPositions { get; set; } = new List<OrgPosition>();

    [InverseProperty("State")]
    public virtual ICollection<OrgTaxSetting> OrgTaxSettings { get; set; } = new List<OrgTaxSetting>();

    [InverseProperty("State")]
    public virtual ICollection<OrgUserInvitation> OrgUserInvitations { get; set; } = new List<OrgUserInvitation>();

    [InverseProperty("State")]
    public virtual ICollection<PlatformTenant> PlatformTenants { get; set; } = new List<PlatformTenant>();

    [InverseProperty("State")]
    public virtual ICollection<PurDoc> PurDocs { get; set; } = new List<PurDoc>();

    [InverseProperty("State")]
    public virtual ICollection<SaleCondition> SaleConditions { get; set; } = new List<SaleCondition>();

    [InverseProperty("State")]
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    [InverseProperty("State")]
    public virtual ICollection<SysModule> SysModules { get; set; } = new List<SysModule>();

    [InverseProperty("State")]
    public virtual ICollection<SysRole> SysRoles { get; set; } = new List<SysRole>();

    [InverseProperty("State")]
    public virtual ICollection<SysUserOrganization> SysUserOrganizations { get; set; } = new List<SysUserOrganization>();

    [InverseProperty("State")]
    public virtual ICollection<SysUser> SysUsers { get; set; } = new List<SysUser>();
}
