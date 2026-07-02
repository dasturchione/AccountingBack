using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_translation")]
[Index("LanguageId", Name = "idx_cmn_translation_language_id")]
[Index("TableName", "RecordId", "ColumnName", Name = "idx_cmn_translation_lookup")]
[Index("LanguageId", "TableName", "RecordId", "ColumnName", Name = "idx_cmn_translation_unique", IsUnique = true)]
public partial class Translation
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("table_name")]
    [StringLength(100)]
    public string TableName { get; set; } = null!;

    [Column("record_id")]
    public long RecordId { get; set; }

    [Column("column_name")]
    [StringLength(100)]
    public string ColumnName { get; set; } = null!;

    [Column("value")]
    public string Value { get; set; } = null!;

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("LanguageId")]
    [InverseProperty("Translations")]
    public virtual Language Language { get; set; } = null!;
}
