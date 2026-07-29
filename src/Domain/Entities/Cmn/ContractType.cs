using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_contract_type")]
public partial class ContractType
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

    [InverseProperty(nameof(Contract.ContractType))]
    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    [InverseProperty(nameof(ContractTypeTranslation.ContractType))]
    public virtual ICollection<ContractTypeTranslation> ContractTypeTranslations { get; set; } = new List<ContractTypeTranslation>();

    [ForeignKey("StateId")]
    [InverseProperty(nameof(State.ContractTypes))]
    public virtual State State { get; set; } = null!;
}
