using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_contract_type")]
[Index("Code", Name = "idx_cmn_contract_type_code", IsUnique = true)]
[Index("StateId", Name = "idx_cmn_contract_type_state_id")]
public partial class CmnContractType
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

    [InverseProperty("ContractType")]
    public virtual ICollection<CmnContract> CmnContracts { get; set; } = new List<CmnContract>();

    [ForeignKey("StateId")]
    [InverseProperty("CmnContractTypes")]
    public virtual CmnState State { get; set; } = null!;
}
