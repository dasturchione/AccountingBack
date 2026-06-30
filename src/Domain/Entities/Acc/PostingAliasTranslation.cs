using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[PrimaryKey("PostingAliasId", "LanguageId")]
[Table("acc_posting_alias_translation")]
public partial class PostingAliasTranslation
{
    [Key]
    [Column("posting_alias_id")]
    public short PostingAliasId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;
}
