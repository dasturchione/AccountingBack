using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("organization_edo_provider")]
public sealed class OrganizationEdoProvider
{
    [Key]
    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("provider")]
    [StringLength(20)]
    public string Provider { get; set; } = null!;

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public Organization Organization { get; set; } = null!;
}
