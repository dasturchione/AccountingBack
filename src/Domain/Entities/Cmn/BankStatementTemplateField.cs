using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_bank_statement_template_field")]
public class BankStatementTemplateField
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("template_id")]
    public int TemplateId { get; set; }

    [Column("section_code")]
    [StringLength(20)]
    public string SectionCode { get; set; } = null!;

    [Column("target_code")]
    [StringLength(50)]
    public string TargetCode { get; set; } = null!;

    [Column("source_type")]
    [StringLength(30)]
    public string SourceType { get; set; } = null!;

    [Column("anchor_code")]
    [StringLength(20)]
    public string? AnchorCode { get; set; }

    [Column("absolute_row_index")]
    public int? AbsoluteRowIndex { get; set; }

    [Column("row_offset")]
    public short? RowOffset { get; set; }

    [Column("column_index")]
    public short? ColumnIndex { get; set; }

    [Column("search_direction")]
    [StringLength(10)]
    public string? SearchDirection { get; set; }

    [Column("search_limit")]
    public short? SearchLimit { get; set; }

    [Column("locator_match_type")]
    [StringLength(20)]
    public string? LocatorMatchType { get; set; }

    [Column("locator_value")]
    [StringLength(500)]
    public string? LocatorValue { get; set; }

    [Column("extract_regex")]
    [StringLength(1000)]
    public string? ExtractRegex { get; set; }

    [Column("extract_group")]
    [StringLength(100)]
    public string? ExtractGroup { get; set; }

    [Column("constant_value")]
    [StringLength(1000)]
    public string? ConstantValue { get; set; }

    [Column("value_type")]
    [StringLength(20)]
    public string ValueType { get; set; } = null!;

    [Column("format")]
    [StringLength(100)]
    public string? Format { get; set; }

    [Column("transform_code")]
    [StringLength(50)]
    public string TransformCode { get; set; } = null!;

    [Column("is_required")]
    public bool IsRequired { get; set; }

    [ForeignKey(nameof(TemplateId))]
    [InverseProperty(nameof(BankStatementTemplate.Fields))]
    public virtual BankStatementTemplate Template { get; set; } = null!;
}
