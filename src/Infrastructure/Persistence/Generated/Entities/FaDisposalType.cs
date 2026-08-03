using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("fa_disposal_type")]
[Index("Code", Name = "fa_disposal_type_code_key", IsUnique = true)]
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

    [InverseProperty("DisposalType")]
    public virtual ICollection<FaDisposalDoc> FaDisposalDocs { get; set; } = new List<FaDisposalDoc>();

    [InverseProperty("DisposalType")]
    public virtual ICollection<FaDisposalTypeTranslation> FaDisposalTypeTranslations { get; set; } = new List<FaDisposalTypeTranslation>();
}
