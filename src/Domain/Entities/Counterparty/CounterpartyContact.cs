using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("counterparty_contact")]
[Index("CounterpartyId", Name = "idx_counterparty_contact_counterparty_id")]
[Index("OrganizationId", Name = "idx_counterparty_contact_organization_id")]
[Index("StateId", Name = "idx_counterparty_contact_state_id")]
public partial class CounterpartyContact
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("counterparty_id")]
    public int CounterpartyId { get; set; }

    [Column("full_name")]
    [StringLength(250)]
    public string FullName { get; set; } = null!;

    [Column("phone_number")]
    [StringLength(50)]
    public string? PhoneNumber { get; set; }

    [Column("email")]
    [StringLength(250)]
    public string? Email { get; set; }

    [Column("position")]
    [StringLength(250)]
    public string? Position { get; set; }

    [Column("comment")]
    [StringLength(1000)]
    public string? Comment { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("CounterpartyId")]
    [InverseProperty("CounterpartyContacts")]
    public virtual CounterpartyCard Counterparty { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("CounterpartyContacts")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("CounterpartyContacts")]
    public virtual State State { get; set; } = null!;
}
