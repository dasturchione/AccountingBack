using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_bank_statement_template")]
public class BankStatementTemplate
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("bank_id")]
    public int BankId { get; set; }

    [Column("code")]
    [StringLength(100)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("version")]
    public short Version { get; set; }

    [Column("sheet_name_match_type")]
    [StringLength(20)]
    public string SheetNameMatchType { get; set; } = null!;

    [Column("sheet_name_pattern")]
    [StringLength(250)]
    public string? SheetNamePattern { get; set; }

    [Column("data_start_row_offset")]
    public short DataStartRowOffset { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey(nameof(BankId))]
    [InverseProperty(nameof(Bank.BankStatementTemplates))]
    public virtual Bank Bank { get; set; } = null!;

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(State.BankStatementTemplates))]
    public virtual State State { get; set; } = null!;

    [InverseProperty(nameof(BankStatementTemplateHeaderRule.Template))]
    public virtual ICollection<BankStatementTemplateHeaderRule> HeaderRules { get; set; } = [];

    [InverseProperty(nameof(BankStatementTemplateRowRule.Template))]
    public virtual ICollection<BankStatementTemplateRowRule> RowRules { get; set; } = [];

    [InverseProperty(nameof(BankStatementTemplateField.Template))]
    public virtual ICollection<BankStatementTemplateField> Fields { get; set; } = [];
}
