using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("org_organization_config")]
[Index("AccountingPolicyId", Name = "idx_org_organization_config_accounting_policy_id")]
[Index("BaseCurrencyId", Name = "idx_org_organization_config_base_currency_id")]
public partial class OrgOrganizationConfig
{
    [Key]
    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("inventory_valuation_method")]
    [StringLength(20)]
    public string InventoryValuationMethod { get; set; } = null!;

    [Column("accounting_policy_id")]
    public short? AccountingPolicyId { get; set; }

    [Column("base_currency_id")]
    public short? BaseCurrencyId { get; set; }

    [Column("accounting_start_date")]
    public DateOnly? AccountingStartDate { get; set; }

    [Column("fiscal_year_start_month")]
    public short FiscalYearStartMonth { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("OrgOrganizationConfig")]
    public virtual OrgOrganization Organization { get; set; } = null!;
}
