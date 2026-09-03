using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("rnt_rental_object_type")]
public sealed class RentalObjectType
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey(nameof(StateId))]
    public State State { get; set; } = null!;

    public ICollection<RentalObjectTypeTranslation> Translations { get; set; } = new List<RentalObjectTypeTranslation>();
    public ICollection<RentalContractObject> ContractObjects { get; set; } = new List<RentalContractObject>();
}
