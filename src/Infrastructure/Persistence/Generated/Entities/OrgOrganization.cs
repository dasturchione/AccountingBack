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
[Index("ShortName", Name = "idx_org_organization_short_name")]
[Index("StateId", Name = "idx_org_organization_state_id")]
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

    [InverseProperty("Organization")]
    public virtual ICollection<AccChartAccountSubkonto> AccChartAccountSubkontos { get; set; } = new List<AccChartAccountSubkonto>();

    [InverseProperty("Organization")]
    public virtual ICollection<AccChartAccount> AccChartAccounts { get; set; } = new List<AccChartAccount>();

    [InverseProperty("Organization")]
    public virtual ICollection<AccPostingRule> AccPostingRules { get; set; } = new List<AccPostingRule>();

    [InverseProperty("Organization")]
    public virtual ICollection<AccRegEntry> AccRegEntries { get; set; } = new List<AccRegEntry>();

    [InverseProperty("Organization")]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    [InverseProperty("Organization")]
    public virtual ICollection<CashBox> CashBoxes { get; set; } = new List<CashBox>();

    [InverseProperty("Organization")]
    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();

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
    public virtual ICollection<InvProductGroup> InvProductGroups { get; set; } = new List<InvProductGroup>();

    [InverseProperty("Organization")]
    public virtual ICollection<InvProductPrice> InvProductPrices { get; set; } = new List<InvProductPrice>();

    [InverseProperty("Organization")]
    public virtual ICollection<InvProductTable> InvProductTables { get; set; } = new List<InvProductTable>();

    [InverseProperty("Organization")]
    public virtual ICollection<InvProduct> InvProducts { get; set; } = new List<InvProduct>();

    [InverseProperty("Organization")]
    public virtual ICollection<InvRegBalance> InvRegBalances { get; set; } = new List<InvRegBalance>();

    [InverseProperty("Organization")]
    public virtual ICollection<InvWarehouse> InvWarehouses { get; set; } = new List<InvWarehouse>();

    [InverseProperty("Organization")]
    public virtual ICollection<MoneyRegBalance> MoneyRegBalances { get; set; } = new List<MoneyRegBalance>();

    [InverseProperty("Organization")]
    public virtual ICollection<OrgBankAccount> OrgBankAccounts { get; set; } = new List<OrgBankAccount>();

    [InverseProperty("Organization")]
    public virtual ICollection<OrgBranch> OrgBranches { get; set; } = new List<OrgBranch>();

    [InverseProperty("Organization")]
    public virtual ICollection<OrgDepartment> OrgDepartments { get; set; } = new List<OrgDepartment>();

    [InverseProperty("Organization")]
    public virtual ICollection<OrgPosition> OrgPositions { get; set; } = new List<OrgPosition>();

    [InverseProperty("Organization")]
    public virtual ICollection<PurDoc> PurDocs { get; set; } = new List<PurDoc>();

    [ForeignKey("RegionId")]
    [InverseProperty("OrgOrganizations")]
    public virtual CmnRegion Region { get; set; } = null!;

    [InverseProperty("Organization")]
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    [ForeignKey("StateId")]
    [InverseProperty("OrgOrganizations")]
    public virtual CmnState State { get; set; } = null!;

    [InverseProperty("Organization")]
    public virtual ICollection<SysRole> SysRoles { get; set; } = new List<SysRole>();

    [InverseProperty("Organization")]
    public virtual ICollection<SysUserOrganization> SysUserOrganizations { get; set; } = new List<SysUserOrganization>();

    [InverseProperty("Organization")]
    public virtual ICollection<SysUser> SysUsers { get; set; } = new List<SysUser>();
}
