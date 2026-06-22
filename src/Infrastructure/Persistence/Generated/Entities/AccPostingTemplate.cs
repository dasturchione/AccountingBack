using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_posting_template")]
[Index("Code", Name = "acc_posting_template_code_key", IsUnique = true)]
public partial class AccPostingTemplate
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

    [Column("document_type_id")]
    public short DocumentTypeId { get; set; }

    [InverseProperty("Template")]
    public virtual ICollection<AccPostingTemplateLine> AccPostingTemplateLines { get; set; } = new List<AccPostingTemplateLine>();

    [ForeignKey("DocumentTypeId")]
    [InverseProperty("AccPostingTemplates")]
    public virtual CmnPostingOperationType DocumentType { get; set; } = null!;
}
