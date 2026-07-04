using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_reg_entry_subkonto")]
[Index("SubkontoTypeId", "EntityId", Name = "idx_acc_reg_entry_subkonto_entity")]
[Index("EntryId", Name = "idx_acc_reg_entry_subkonto_entry_id")]
[Index("Side", Name = "idx_acc_reg_entry_subkonto_side")]
[Index("SubkontoTypeId", Name = "idx_acc_reg_entry_subkonto_type_id")]
public partial class AccRegEntrySubkonto
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("entry_id")]
    public long EntryId { get; set; }

    [Column("side")]
    [StringLength(2)]
    public string Side { get; set; } = null!;

    [Column("subkonto_type_id")]
    public short SubkontoTypeId { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("entity_id")]
    public long? EntityId { get; set; }

    [Column("display_value")]
    [StringLength(500)]
    public string? DisplayValue { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("EntryId")]
    [InverseProperty("AccRegEntrySubkontos")]
    public virtual AccRegEntry Entry { get; set; } = null!;

    [ForeignKey("SubkontoTypeId")]
    [InverseProperty("AccRegEntrySubkontos")]
    public virtual AccSubkontoType SubkontoType { get; set; } = null!;
}
