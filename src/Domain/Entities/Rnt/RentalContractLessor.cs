using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[PrimaryKey(nameof(ContractId), nameof(LessorId))]
[Table("rnt_contract_lessor")]
public sealed class RentalContractLessor
{
    [Key]
    [Column("contract_id")]
    public long ContractId { get; set; }

    [Key]
    [Column("lessor_id")]
    public long LessorId { get; set; }

    [ForeignKey(nameof(ContractId))]
    [InverseProperty(nameof(RentalContract.Lessors))]
    public RentalContract Contract { get; set; } = null!;

    [ForeignKey(nameof(LessorId))]
    [InverseProperty(nameof(RentalLessor.Contracts))]
    public RentalLessor Lessor { get; set; } = null!;
}
