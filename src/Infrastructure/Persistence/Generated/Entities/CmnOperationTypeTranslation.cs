using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("OperationTypeId", "LanguageId")]
[Table("cmn_operation_type_translation")]
[Index("LanguageId", Name = "ix_cmn_operation_type_translation_language_id")]
public partial class CmnOperationTypeTranslation
{
    [Key]
    [Column("operation_type_id")]
    public short OperationTypeId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty("CmnOperationTypeTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;

    [ForeignKey("OperationTypeId")]
    [InverseProperty("CmnOperationTypeTranslations")]
    public virtual CmnOperationType OperationType { get; set; } = null!;
}
