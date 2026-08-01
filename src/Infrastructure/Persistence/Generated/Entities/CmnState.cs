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
    public virtual ICollection<AccChartAccountPresetAccount> AccChartAccountPresetAccounts { get; set; } = new List<AccChartAccountPresetAccount>();

    [InverseProperty("State")]
    public virtual ICollection<AccChartAccountPreset> AccChartAccountPresets { get; set; } = new List<AccChartAccountPreset>();

    [InverseProperty("State")]
    public virtual ICollection<AccChartAccountSubkonto> AccChartAccountSubkontos { get; set; } = new List<AccChartAccountSubkonto>();

    [InverseProperty("State")]
    public virtual ICollection<AccChartAccount> AccChartAccounts { get; set; } = new List<AccChartAccount>();

    [InverseProperty("State")]
    public virtual ICollection<AccDocumentAccountRole> AccDocumentAccountRoles { get; set; } = new List<AccDocumentAccountRole>();

    [InverseProperty("State")]
    public virtual ICollection<AccDocumentAccountSetting> AccDocumentAccountSettings { get; set; } = new List<AccDocumentAccountSetting>();

    [InverseProperty("State")]
    public virtual ICollection<AccDocumentAccountType> AccDocumentAccountTypes { get; set; } = new List<AccDocumentAccountType>();

    [InverseProperty("State")]
    public virtual ICollection<AccOpeningBalance> AccOpeningBalances { get; set; } = new List<AccOpeningBalance>();

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
    public virtual ICollection<CmnCurrencyRate> CmnCurrencyRates { get; set; } = new List<CmnCurrencyRate>();

    [InverseProperty("State")]
    public virtual ICollection<CmnCurrencyRevaluationLine> CmnCurrencyRevaluationLines { get; set; } = new List<CmnCurrencyRevaluationLine>();

    [InverseProperty("State")]
    public virtual ICollection<CmnCurrencyRevaluation> CmnCurrencyRevaluations { get; set; } = new List<CmnCurrencyRevaluation>();

    [InverseProperty("State")]
    public virtual ICollection<CmnDocumentStatus> CmnDocumentStatuses { get; set; } = new List<CmnDocumentStatus>();

    [InverseProperty("State")]
    public virtual ICollection<CmnDocumentType> CmnDocumentTypes { get; set; } = new List<CmnDocumentType>();

    [InverseProperty("State")]
    public virtual ICollection<CmnFaAssetStatus> CmnFaAssetStatuses { get; set; } = new List<CmnFaAssetStatus>();

    [InverseProperty("State")]
    public virtual ICollection<CmnFaDepreciationMethod> CmnFaDepreciationMethods { get; set; } = new List<CmnFaDepreciationMethod>();

    [InverseProperty("State")]
    public virtual ICollection<CmnFaGroup> CmnFaGroups { get; set; } = new List<CmnFaGroup>();

    [InverseProperty("State")]
    public virtual ICollection<CmnFaOkof> CmnFaOkofs { get; set; } = new List<CmnFaOkof>();

    [InverseProperty("State")]
    public virtual ICollection<CmnInventoryAdjustmentType> CmnInventoryAdjustmentTypes { get; set; } = new List<CmnInventoryAdjustmentType>();

    [InverseProperty("State")]
    public virtual ICollection<CmnLanguage> CmnLanguages { get; set; } = new List<CmnLanguage>();

    [InverseProperty("State")]
    public virtual ICollection<CmnMxikCatalog> CmnMxikCatalogs { get; set; } = new List<CmnMxikCatalog>();

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
    public virtual ICollection<FaAsset> FaAssets { get; set; } = new List<FaAsset>();

    [InverseProperty("State")]
    public virtual ICollection<FaDepreciationRun> FaDepreciationRuns { get; set; } = new List<FaDepreciationRun>();

    [InverseProperty("State")]
    public virtual ICollection<FaDisposalDoc> FaDisposalDocs { get; set; } = new List<FaDisposalDoc>();

    [InverseProperty("State")]
    public virtual ICollection<FaMovementDoc> FaMovementDocs { get; set; } = new List<FaMovementDoc>();

    [InverseProperty("State")]
    public virtual ICollection<FaReceiptDoc> FaReceiptDocs { get; set; } = new List<FaReceiptDoc>();

    [InverseProperty("State")]
    public virtual ICollection<FaRevaluationDoc> FaRevaluationDocs { get; set; } = new List<FaRevaluationDoc>();

    [InverseProperty("State")]
    public virtual ICollection<HrAbsenceType> HrAbsenceTypes { get; set; } = new List<HrAbsenceType>();

    [InverseProperty("State")]
    public virtual ICollection<HrAbsence> HrAbsences { get; set; } = new List<HrAbsence>();

    [InverseProperty("State")]
    public virtual ICollection<HrEmployeeWorkSchedule> HrEmployeeWorkSchedules { get; set; } = new List<HrEmployeeWorkSchedule>();

    [InverseProperty("State")]
    public virtual ICollection<InvInventoryAdjustmentDoc> InvInventoryAdjustmentDocs { get; set; } = new List<InvInventoryAdjustmentDoc>();

    [InverseProperty("State")]
    public virtual ICollection<InvInventoryCountDoc> InvInventoryCountDocs { get; set; } = new List<InvInventoryCountDoc>();

    [InverseProperty("State")]
    public virtual ICollection<InvOpeningInventory> InvOpeningInventories { get; set; } = new List<InvOpeningInventory>();

    [InverseProperty("State")]
    public virtual ICollection<InvProductGroup> InvProductGroups { get; set; } = new List<InvProductGroup>();

    [InverseProperty("State")]
    public virtual ICollection<InvProductPrice> InvProductPrices { get; set; } = new List<InvProductPrice>();

    [InverseProperty("State")]
    public virtual ICollection<InvProduct> InvProducts { get; set; } = new List<InvProduct>();

    [InverseProperty("State")]
    public virtual ICollection<InvTransferDoc> InvTransferDocs { get; set; } = new List<InvTransferDoc>();

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
    public virtual ICollection<PayComponent> PayComponents { get; set; } = new List<PayComponent>();

    [InverseProperty("State")]
    public virtual ICollection<PayEmployeeComponent> PayEmployeeComponents { get; set; } = new List<PayEmployeeComponent>();

    [InverseProperty("State")]
    public virtual ICollection<PayEmployee> PayEmployees { get; set; } = new List<PayEmployee>();

    [InverseProperty("State")]
    public virtual ICollection<PayEmployment> PayEmployments { get; set; } = new List<PayEmployment>();

    [InverseProperty("State")]
    public virtual ICollection<PayPaymentBatch> PayPaymentBatches { get; set; } = new List<PayPaymentBatch>();

    [InverseProperty("State")]
    public virtual ICollection<PayPayrollDoc> PayPayrollDocs { get; set; } = new List<PayPayrollDoc>();

    [InverseProperty("State")]
    public virtual ICollection<PayTimesheet> PayTimesheets { get; set; } = new List<PayTimesheet>();

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
