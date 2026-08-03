using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("fa_disposal_type")]
public partial class FaDisposalType
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(30)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [InverseProperty(nameof(FaDisposalDoc.DisposalType))]
    public virtual ICollection<FaDisposalDoc> FaDisposalDocs { get; set; } = new List<FaDisposalDoc>();

    [InverseProperty(nameof(FaDisposalTypeTranslation.DisposalType))]
    public virtual ICollection<FaDisposalTypeTranslation> FaDisposalTypeTranslations { get; set; } = new List<FaDisposalTypeTranslation>();
}
