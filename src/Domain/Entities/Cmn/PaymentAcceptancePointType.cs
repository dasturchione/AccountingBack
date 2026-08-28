using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_payment_acceptance_point_type")]
[Index(nameof(Code), Name = "ux_cmn_payment_acceptance_point_type_code", IsUnique = true)]
public sealed class PaymentAcceptancePointType
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(State.PaymentAcceptancePointTypes))]
    public State State { get; set; } = null!;

    [InverseProperty(nameof(PaymentAcceptancePoint.Type))]
    public ICollection<PaymentAcceptancePoint> PaymentAcceptancePoints { get; set; } = [];

    [InverseProperty(nameof(PaymentAcceptancePointTypeTranslation.PaymentAcceptancePointType))]
    public ICollection<PaymentAcceptancePointTypeTranslation> Translations { get; set; } = [];
}
