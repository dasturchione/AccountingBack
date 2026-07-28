using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("ContractTypeId", "LanguageId")]
[Table("cmn_contract_type_translation")]
public partial class ContractTypeTranslation
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
    [InverseProperty(nameof(Entities.ContractType.ContractTypeTranslations))]
    public virtual ContractType ContractType { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty(nameof(Entities.Language.ContractTypeTranslations))]
    public virtual Language Language { get; set; } = null!;
}
