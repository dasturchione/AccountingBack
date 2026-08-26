using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_bank_statement_template_header_rule")]
public class BankStatementTemplateHeaderRule
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("template_id")]
    public int TemplateId { get; set; }

    [Column("row_offset")]
    public short RowOffset { get; set; }

    [Column("column_index")]
    public short ColumnIndex { get; set; }

    [Column("match_type")]
    [StringLength(20)]
    public string MatchType { get; set; } = null!;

    [Column("expected_value")]
    [StringLength(500)]
    public string ExpectedValue { get; set; } = null!;

    [Column("normalization_code")]
    [StringLength(30)]
    public string NormalizationCode { get; set; } = null!;

    [Column("is_required")]
    public bool IsRequired { get; set; }

    [ForeignKey(nameof(TemplateId))]
    [InverseProperty(nameof(BankStatementTemplate.HeaderRules))]
    public virtual BankStatementTemplate Template { get; set; } = null!;
}
