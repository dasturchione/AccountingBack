using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("org_organization")]
[Index("DefaultLanguageId", Name = "idx_org_organization_default_language_id")]
[Index("DistrictId", Name = "idx_org_organization_district_id")]
[Index("FullName", Name = "idx_org_organization_full_name")]
[Index("Inn", Name = "idx_org_organization_inn")]
[Index("RegionId", Name = "idx_org_organization_region_id")]
[Index("SetupStatus", Name = "idx_org_organization_setup_status")]
[Index("ShortName", Name = "idx_org_organization_short_name")]
[Index("StateId", Name = "idx_org_organization_state_id")]
[Index("TenantId", Name = "idx_org_organization_tenant_id")]
public partial class OrgOrganization
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("short_name")]
    [StringLength(250)]
    public string ShortName { get; set; } = null!;

    [Column("full_name")]
    [StringLength(500)]
    public string FullName { get; set; } = null!;

    [Column("inn")]
    [StringLength(20)]
    public string Inn { get; set; } = null!;

    [Column("phone_number")]
    [StringLength(50)]
    public string? PhoneNumber { get; set; }

    [Column("region_id")]
    public int RegionId { get; set; }

    [Column("district_id")]
    public int? DistrictId { get; set; }

    [Column("address")]
    [StringLength(1000)]
    public string? Address { get; set; }

    [Column("director")]
    [StringLength(250)]
    public string? Director { get; set; }

    [Column("is_parent")]
    public bool IsParent { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("default_language_id")]
    public short? DefaultLanguageId { get; set; }

    [Column("tenant_id")]
    public int? TenantId { get; set; }

    [Column("setup_status")]
    [StringLength(30)]
    public string SetupStatus { get; set; } = null!;

    [Column("setup_completed_at", TypeName = "timestamp without time zone")]
    public DateTime? SetupCompletedAt { get; set; }

    [Column("email")]
    [StringLength(200)]
    public string? Email { get; set; }

    [Column("website")]
    [StringLength(250)]
    public string? Website { get; set; }

    [Column("oked")]
    [StringLength(20)]
    public string? Oked { get; set; }

    [InverseProperty("Organization")]
    public virtual ICollection<AccChartAccount> AccChartAccounts { get; set; } = new List<AccChartAccount>();

    [InverseProperty("Organization")]
    public virtual ICollection<AccDocumentAccountSetting> AccDocumentAccountSettings { get; set; } = new List<AccDocumentAccountSetting>();

    [InverseProperty("Organization")]
    public virtual AccOpeningBalance? AccOpeningBalance { get; set; }

    [InverseProperty("Organization")]
    public virtual ICollection<AccRegEntry> AccRegEntries { get; set; } = new List<AccRegEntry>();

    [InverseProperty("Organization")]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    [InverseProperty("Organization")]
    public virtual ICollection<CashBox> CashBoxes { get; set; } = new List<CashBox>();

    [InverseProperty("Organization")]
    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();

    [InverseProperty("Organization")]
    public virtual ICollection<CmnContract> CmnContracts { get; set; } = new List<CmnContract>();

    [InverseProperty("Organization")]
    public virtual ICollection<CmnCurrencyRevaluation> CmnCurrencyRevaluations { get; set; } = new List<CmnCurrencyRevaluation>();

    [InverseProperty("Organization")]
    public virtual ICollection<CmnDocumentNumberSequence> CmnDocumentNumberSequences { get; set; } = new List<CmnDocumentNumberSequence>();

    [InverseProperty("Organization")]
    public virtual ICollection<CmnFaGroup> CmnFaGroups { get; set; } = new List<CmnFaGroup>();

    [InverseProperty("Organization")]
    public virtual ICollection<CmnPricingCondition> CmnPricingConditions { get; set; } = new List<CmnPricingCondition>();

    [InverseProperty("Organization")]
    public virtual ICollection<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; } = new List<CounterpartyBankAccount>();

    [InverseProperty("Organization")]
    public virtual ICollection<CounterpartyCard> CounterpartyCards { get; set; } = new List<CounterpartyCard>();

    [InverseProperty("Organization")]
    public virtual ICollection<CounterpartyContact> CounterpartyContacts { get; set; } = new List<CounterpartyContact>();

    [InverseProperty("Organization")]
    public virtual ICollection<CounterpartyRegBalance> CounterpartyRegBalances { get; set; } = new List<CounterpartyRegBalance>();

    [ForeignKey("DefaultLanguageId")]
    [InverseProperty("OrgOrganizations")]
    public virtual CmnLanguage? DefaultLanguage { get; set; }

    [ForeignKey("DistrictId")]
    [InverseProperty("OrgOrganizations")]
    public virtual CmnDistrict? District { get; set; }

    [InverseProperty("Organization")]
    public virtual ICollection<EdoAuthSigningSession> EdoAuthSigningSessions { get; set; } = new List<EdoAuthSigningSession>();

    [InverseProperty("Organization")]
    public virtual ICollection<EdoDocumentSigningSession> EdoDocumentSigningSessions { get; set; } = new List<EdoDocumentSigningSession>();

    [InverseProperty("Organization")]
    public virtual ICollection<EdoDocument> EdoDocuments { get; set; } = new List<EdoDocument>();

    [InverseProperty("Organization")]
    public virtual ICollection<FaAsset> FaAssets { get; set; } = new List<FaAsset>();

    [InverseProperty("Organization")]
    public virtual ICollection<FaDepreciationRun> FaDepreciationRuns { get; set; } = new List<FaDepreciationRun>();

    [InverseProperty("Organization")]
    public virtual ICollection<FaDisposalDoc> FaDisposalDocs { get; set; } = new List<FaDisposalDoc>();

    [InverseProperty("Organization")]
    public virtual ICollection<FaMovementDoc> FaMovementDocs { get; set; } = new List<FaMovementDoc>();

    [InverseProperty("Organization")]
    public virtual ICollection<FaReceiptDoc> FaReceiptDocs { get; set; } = new List<FaReceiptDoc>();

    [InverseProperty("Organization")]
    public virtual ICollection<FaRevaluationDoc> FaRevaluationDocs { get; set; } = new List<FaRevaluationDoc>();

    [InverseProperty("Organization")]
    public virtual ICollection<HrAbsenceAttachment> HrAbsenceAttachments { get; set; } = new List<HrAbsenceAttachment>();

    [InverseProperty("Organization")]
    public virtual ICollection<HrAbsence> HrAbsences { get; set; } = new List<HrAbsence>();

    [InverseProperty("Organization")]
    public virtual ICollection<HrEmployeeWorkScheduleDay> HrEmployeeWorkScheduleDays { get; set; } = new List<HrEmployeeWorkScheduleDay>();

    [InverseProperty("Organization")]
    public virtual ICollection<HrEmployeeWorkSchedule> HrEmployeeWorkSchedules { get; set; } = new List<HrEmployeeWorkSchedule>();

    [InverseProperty("Organization")]
    public virtual ICollection<IdempotencyRecord> IdempotencyRecords { get; set; } = new List<IdempotencyRecord>();

    [InverseProperty("Organization")]
    public virtual ICollection<IntegrationCredential> IntegrationCredentials { get; set; } = new List<IntegrationCredential>();

    [InverseProperty("Organization")]
    public virtual ICollection<InvInventoryAdjustmentDoc> InvInventoryAdjustmentDocs { get; set; } = new List<InvInventoryAdjustmentDoc>();

    [InverseProperty("Organization")]
    public virtual ICollection<InvInventoryCountDoc> InvInventoryCountDocs { get; set; } = new List<InvInventoryCountDoc>();

    [InverseProperty("Organization")]
    public virtual ICollection<InvOpeningInventory> InvOpeningInventories { get; set; } = new List<InvOpeningInventory>();

    [InverseProperty("Organization")]
    public virtual ICollection<InvProductPrice> InvProductPrices { get; set; } = new List<InvProductPrice>();

    [InverseProperty("Organization")]
    public virtual ICollection<InvProduct> InvProducts { get; set; } = new List<InvProduct>();

    [InverseProperty("Organization")]
    public virtual ICollection<InvRegBalance> InvRegBalances { get; set; } = new List<InvRegBalance>();

    [InverseProperty("Organization")]
    public virtual ICollection<InvTransferDoc> InvTransferDocs { get; set; } = new List<InvTransferDoc>();

    [InverseProperty("Organization")]
    public virtual ICollection<InvWarehouseProductBatch> InvWarehouseProductBatches { get; set; } = new List<InvWarehouseProductBatch>();

    [InverseProperty("Organization")]
    public virtual ICollection<InvWarehouseProductMovement> InvWarehouseProductMovements { get; set; } = new List<InvWarehouseProductMovement>();

    [InverseProperty("Organization")]
    public virtual ICollection<InvWarehouse> InvWarehouses { get; set; } = new List<InvWarehouse>();

    [InverseProperty("Organization")]
    public virtual ICollection<MarkingAggregation> MarkingAggregations { get; set; } = new List<MarkingAggregation>();

    [InverseProperty("Organization")]
    public virtual ICollection<MarkingAslbelgiDocument> MarkingAslbelgiDocuments { get; set; } = new List<MarkingAslbelgiDocument>();

    [InverseProperty("Organization")]
    public virtual ICollection<MarkingBusinessPlace> MarkingBusinessPlaces { get; set; } = new List<MarkingBusinessPlace>();

    [InverseProperty("Organization")]
    public virtual ICollection<MarkingCode> MarkingCodes { get; set; } = new List<MarkingCode>();

    [InverseProperty("Organization")]
    public virtual ICollection<MarkingDidoxDocument> MarkingDidoxDocuments { get; set; } = new List<MarkingDidoxDocument>();

    [InverseProperty("Organization")]
    public virtual ICollection<MarkingEdocsDocument> MarkingEdocsDocuments { get; set; } = new List<MarkingEdocsDocument>();

    [InverseProperty("Organization")]
    public virtual ICollection<MarkingOrder> MarkingOrders { get; set; } = new List<MarkingOrder>();

    [InverseProperty("Organization")]
    public virtual ICollection<MarkingUtilization> MarkingUtilizations { get; set; } = new List<MarkingUtilization>();

    [InverseProperty("Organization")]
    public virtual ICollection<MoneyRegBalance> MoneyRegBalances { get; set; } = new List<MoneyRegBalance>();

    [InverseProperty("Organization")]
    public virtual ICollection<OrgBankAccount> OrgBankAccounts { get; set; } = new List<OrgBankAccount>();

    [InverseProperty("Organization")]
    public virtual ICollection<OrgBranch> OrgBranches { get; set; } = new List<OrgBranch>();

    [InverseProperty("Organization")]
    public virtual ICollection<OrgDepartment> OrgDepartments { get; set; } = new List<OrgDepartment>();

    [InverseProperty("Organization")]
    public virtual OrgOrganizationConfig? OrgOrganizationConfig { get; set; }

    [InverseProperty("Organization")]
    public virtual ICollection<OrgPosition> OrgPositions { get; set; } = new List<OrgPosition>();

    [InverseProperty("Organization")]
    public virtual OrganizationEdoProvider? OrganizationEdoProvider { get; set; }

    [InverseProperty("Organization")]
    public virtual ICollection<PayComponent> PayComponents { get; set; } = new List<PayComponent>();

    [InverseProperty("Organization")]
    public virtual ICollection<PayEmployeeComponent> PayEmployeeComponents { get; set; } = new List<PayEmployeeComponent>();

    [InverseProperty("Organization")]
    public virtual ICollection<PayEmployee> PayEmployees { get; set; } = new List<PayEmployee>();

    [InverseProperty("Organization")]
    public virtual ICollection<PayEmployment> PayEmployments { get; set; } = new List<PayEmployment>();

    [InverseProperty("Organization")]
    public virtual ICollection<PayPaymentBatch> PayPaymentBatches { get; set; } = new List<PayPaymentBatch>();

    [InverseProperty("Organization")]
    public virtual ICollection<PayPaymentLine> PayPaymentLines { get; set; } = new List<PayPaymentLine>();

    [InverseProperty("Organization")]
    public virtual ICollection<PayPayrollCalcLine> PayPayrollCalcLines { get; set; } = new List<PayPayrollCalcLine>();

    [InverseProperty("Organization")]
    public virtual ICollection<PayPayrollDoc> PayPayrollDocs { get; set; } = new List<PayPayrollDoc>();

    [InverseProperty("Organization")]
    public virtual ICollection<PayPayrollLine> PayPayrollLines { get; set; } = new List<PayPayrollLine>();

    [InverseProperty("Organization")]
    public virtual ICollection<PayPeriod> PayPeriods { get; set; } = new List<PayPeriod>();

    [InverseProperty("Organization")]
    public virtual ICollection<PayTimesheetLine> PayTimesheetLines { get; set; } = new List<PayTimesheetLine>();

    [InverseProperty("Organization")]
    public virtual ICollection<PayTimesheet> PayTimesheets { get; set; } = new List<PayTimesheet>();

    [InverseProperty("Organization")]
    public virtual ICollection<PurDoc> PurDocs { get; set; } = new List<PurDoc>();

    [ForeignKey("RegionId")]
    [InverseProperty("OrgOrganizations")]
    public virtual CmnRegion Region { get; set; } = null!;

    [InverseProperty("Organization")]
    public virtual ICollection<SaleCondition> SaleConditions { get; set; } = new List<SaleCondition>();

    [InverseProperty("Organization")]
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    [InverseProperty("Organization")]
    public virtual ICollection<SaleShipmentDoc> SaleShipmentDocs { get; set; } = new List<SaleShipmentDoc>();

    [ForeignKey("StateId")]
    [InverseProperty("OrgOrganizations")]
    public virtual CmnState State { get; set; } = null!;

    [InverseProperty("Organization")]
    public virtual ICollection<SysRole> SysRoles { get; set; } = new List<SysRole>();

    [InverseProperty("Organization")]
    public virtual ICollection<SysUserOrganization> SysUserOrganizations { get; set; } = new List<SysUserOrganization>();
}
