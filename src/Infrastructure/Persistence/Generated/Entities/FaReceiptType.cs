using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("fa_receipt_type")]
[Index("Code", Name = "fa_receipt_type_code_key", IsUnique = true)]
public partial class FaReceiptType
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

    [InverseProperty("ReceiptType")]
    public virtual ICollection<FaReceiptDoc> FaReceiptDocs { get; set; } = new List<FaReceiptDoc>();

    [InverseProperty("ReceiptType")]
    public virtual ICollection<FaReceiptTypeTranslation> FaReceiptTypeTranslations { get; set; } = new List<FaReceiptTypeTranslation>();
}
