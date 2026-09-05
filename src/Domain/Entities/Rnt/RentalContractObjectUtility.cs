using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("rnt_contract_object_utility")]
public sealed class RentalContractObjectUtility
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("contract_object_id")]
    public long ContractObjectId { get; set; }

    [Column("utility_service_id")]
    public short UtilityServiceId { get; set; }

    [Column("payer_code")]
    [StringLength(20)]
    public string PayerCode { get; set; } = null!;

    [ForeignKey(nameof(ContractObjectId))]
    [InverseProperty(nameof(RentalContractObject.Utilities))]
    public RentalContractObject ContractObject { get; set; } = null!;

    [ForeignKey(nameof(UtilityServiceId))]
    [InverseProperty(nameof(Entities.UtilityService.ContractObjectUtilities))]
    public UtilityService UtilityService { get; set; } = null!;
}
