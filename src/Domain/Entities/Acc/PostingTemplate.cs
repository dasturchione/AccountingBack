using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("acc_posting_template")]
public partial class PostingTemplate
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

    [Column("document_type_id")]
    public short DocumentTypeId { get; set; }

    [InverseProperty("Template")]
    public virtual ICollection<PostingTemplateLine> PostingTemplateLines { get; set; } = new List<PostingTemplateLine>();

    [ForeignKey("DocumentTypeId")]
    [InverseProperty("PostingTemplates")]
    public virtual PostingOperationType DocumentType { get; set; } = null!;
}
