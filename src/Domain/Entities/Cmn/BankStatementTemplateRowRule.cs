using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_bank_statement_template_row_rule")]
public class BankStatementTemplateRowRule
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("template_id")]
    public int TemplateId { get; set; }

    [Column("priority")]
    public short Priority { get; set; }

    [Column("row_kind")]
    [StringLength(20)]
    public string RowKind { get; set; } = null!;

    [Column("column_index")]
    public short ColumnIndex { get; set; }

    [Column("operator_code")]
    [StringLength(20)]
    public string OperatorCode { get; set; } = null!;

    [Column("compare_value")]
    [StringLength(500)]
    public string? CompareValue { get; set; }

    [Column("normalization_code")]
    [StringLength(30)]
    public string NormalizationCode { get; set; } = null!;

    [ForeignKey(nameof(TemplateId))]
    [InverseProperty(nameof(BankStatementTemplate.RowRules))]
    public virtual BankStatementTemplate Template { get; set; } = null!;
}
