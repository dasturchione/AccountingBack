using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("acc_posting_template_line")]
public partial class PostingTemplateLine
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("template_id")]
    public short TemplateId { get; set; }

    [Column("order_number")]
    public short OrderNumber { get; set; }

    [Column("debit_alias")]
    [StringLength(250)]
    public string DebitAlias { get; set; } = null!;

    [Column("credit_alias")]
    [StringLength(250)]
    public string CreditAlias { get; set; } = null!;

    [Column("amount_source")]
    [StringLength(20)]
    public string? AmountSource { get; set; }

    [Column("is_optional")]
    public bool IsOptional { get; set; }

    [ForeignKey("TemplateId")]
    [InverseProperty("PostingTemplateLines")]
    public virtual PostingTemplate Template { get; set; } = null!;
}
