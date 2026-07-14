using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_document_account_setting")]
[Index("ChartAccountId", Name = "idx_acc_document_account_setting_chart_account_id")]
[Index("OrganizationId", Name = "idx_acc_document_account_setting_organization_id")]
[Index("DocumentAccountTypeRoleId", Name = "idx_acc_document_account_setting_type_role_id")]
[Index("OrganizationId", "DocumentAccountTypeRoleId", "ChartAccountId", Name = "uq_acc_document_account_setting", IsUnique = true)]
public partial class AccDocumentAccountSetting
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("document_account_type_role_id")]
    public int DocumentAccountTypeRoleId { get; set; }

    [Column("chart_account_id")]
    public int ChartAccountId { get; set; }

    [Column("is_default")]
    public bool IsDefault { get; set; }

    [Column("can_change")]
    public bool CanChange { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [ForeignKey("ChartAccountId")]
    [InverseProperty("AccDocumentAccountSettings")]
    public virtual AccChartAccount ChartAccount { get; set; } = null!;

    [ForeignKey("DocumentAccountTypeRoleId")]
    [InverseProperty("AccDocumentAccountSettings")]
    public virtual AccDocumentAccountTypeRole DocumentAccountTypeRole { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("AccDocumentAccountSettings")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("AccDocumentAccountSettings")]
    public virtual CmnState State { get; set; } = null!;
}
