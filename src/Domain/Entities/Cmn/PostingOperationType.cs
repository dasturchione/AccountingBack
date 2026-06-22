using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_posting_operation_type")]
public partial class PostingOperationType
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

    [Column("state_id")]
    public short StateId { get; set; }

    [InverseProperty("DocumentType")]
    public virtual ICollection<PostingTemplate> PostingTemplates { get; set; } = new List<PostingTemplate>();

    [ForeignKey("StateId")]
    [InverseProperty("PostingOperationTypes")]
    public virtual State State { get; set; } = null!;
}
