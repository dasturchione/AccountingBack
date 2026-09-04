using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("rnt_lessor")]
public sealed class RentalLessor
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("lessor_kind_code")]
    [StringLength(20)]
    public string LessorKindCode { get; set; } = null!;

    [Column("counterparty_id")]
    public int? CounterpartyId { get; set; }

    [Column("full_name")]
    [StringLength(500)]
    public string FullName { get; set; } = null!;

    [Column("inn")]
    [StringLength(20)]
    public string? Inn { get; set; }

    [Column("pinfl")]
    [StringLength(14)]
    public string? Pinfl { get; set; }

    [Column("phone_number")]
    [StringLength(50)]
    public string? PhoneNumber { get; set; }

    [Column("registered_address")]
    [StringLength(1000)]
    public string? RegisteredAddress { get; set; }

    [Column("residential_address")]
    [StringLength(1000)]
    public string? ResidentialAddress { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    [InverseProperty(nameof(Entities.Organization.RentalLessors))]
    public Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(CounterpartyId))]
    [InverseProperty(nameof(CounterpartyCard.RentalLessors))]
    public CounterpartyCard? Counterparty { get; set; }

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(Entities.State.RentalLessors))]
    public State State { get; set; } = null!;

    [InverseProperty(nameof(RentalContractLessor.Lessor))]
    public ICollection<RentalContractLessor> Contracts { get; set; } = [];
}
