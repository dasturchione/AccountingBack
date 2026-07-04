using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_contract")]
[Index("ContractDate", Name = "idx_cmn_contract_contract_date")]
[Index("CounterpartyId", Name = "idx_cmn_contract_counterparty_id")]
[Index("OrganizationId", "CounterpartyId", "ContractNumber", Name = "idx_cmn_contract_number", IsUnique = true)]
[Index("OrganizationId", Name = "idx_cmn_contract_organization_id")]
[Index("StateId", Name = "idx_cmn_contract_state_id")]
public partial class CmnContract
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("counterparty_id")]
    public int CounterpartyId { get; set; }

    [Column("contract_number")]
    [StringLength(100)]
    public string ContractNumber { get; set; } = null!;

    [Column("contract_date", TypeName = "timestamp without time zone")]
    public DateTime ContractDate { get; set; }

    [Column("start_date", TypeName = "timestamp without time zone")]
    public DateTime? StartDate { get; set; }

    [Column("end_date", TypeName = "timestamp without time zone")]
    public DateTime? EndDate { get; set; }

    [Column("comment")]
    [StringLength(1000)]
    public string? Comment { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("contract_type_id")]
    public short ContractTypeId { get; set; }

    [InverseProperty("Contract")]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    [ForeignKey("ContractTypeId")]
    [InverseProperty("CmnContracts")]
    public virtual CmnContractType ContractType { get; set; } = null!;

    [ForeignKey("CounterpartyId")]
    [InverseProperty("CmnContracts")]
    public virtual CounterpartyCard Counterparty { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("CmnContracts")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [InverseProperty("Contract")]
    public virtual ICollection<PurDoc> PurDocs { get; set; } = new List<PurDoc>();

    [InverseProperty("Contract")]
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    [ForeignKey("StateId")]
    [InverseProperty("CmnContracts")]
    public virtual CmnState State { get; set; } = null!;
}
