using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_posting_operation_type")]
[Index("Code", Name = "cmn_posting_operation_type_code_key", IsUnique = true)]
public partial class CmnPostingOperationType
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [InverseProperty("DocumentType")]
    public virtual ICollection<AccPostingTemplate> AccPostingTemplates { get; set; } = new List<AccPostingTemplate>();

    [ForeignKey("StateId")]
    [InverseProperty("CmnPostingOperationTypes")]
    public virtual CmnState State { get; set; } = null!;
}
