using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("sys_setting")]
[Index("Category", Name = "idx_sys_setting_category")]
[Index("OrganizationId", Name = "idx_sys_setting_organization_id")]
public partial class SysSetting
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("code")]
    [StringLength(100)]
    public string Code { get; set; } = null!;

    [Column("value")]
    public string? Value { get; set; }

    [Column("value_type")]
    public short ValueType { get; set; }

    [Column("category")]
    [StringLength(100)]
    public string? Category { get; set; }

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Column("is_editable")]
    public bool IsEditable { get; set; }

    [Column("organization_id")]
    public int? OrganizationId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }
}
