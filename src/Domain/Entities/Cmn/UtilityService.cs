using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_utility_service")]
public sealed class UtilityService
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
    [InverseProperty(nameof(Entities.State.UtilityServices))]
    public State State { get; set; } = null!;

    [InverseProperty(nameof(UtilityServiceTranslation.UtilityService))]
    public ICollection<UtilityServiceTranslation> Translations { get; set; } = [];

    [InverseProperty(nameof(RentalContractObjectUtility.UtilityService))]
    public ICollection<RentalContractObjectUtility> ContractObjectUtilities { get; set; } = [];
}
