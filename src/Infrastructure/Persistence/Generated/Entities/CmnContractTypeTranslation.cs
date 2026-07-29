using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("ContractTypeId", "LanguageId")]
[Table("cmn_contract_type_translation")]
[Index("LanguageId", Name = "ix_cmn_contract_type_translation_language_id")]
public partial class CmnContractTypeTranslation
{
    [Key]
    [Column("contract_type_id")]
    public short ContractTypeId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [ForeignKey("ContractTypeId")]
    [InverseProperty("CmnContractTypeTranslations")]
    public virtual CmnContractType ContractType { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty("CmnContractTypeTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;
}
