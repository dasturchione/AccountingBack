using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("acc_posting_alias")]
[Index("Code", Name = "acc_posting_alias_code_key", IsUnique = true)]
public partial class PostingAlias
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

    [InverseProperty("Alias")]
    public virtual ICollection<PaymentPurpose> PaymentPurposes { get; set; } = new List<PaymentPurpose>();

    [InverseProperty("PostingAlias")]
    public virtual ICollection<PostingAliasTranslation> PostingAliasTranslations { get; set; } = new List<PostingAliasTranslation>();
}
