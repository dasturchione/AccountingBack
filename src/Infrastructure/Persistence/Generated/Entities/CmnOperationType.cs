using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_operation_type")]
[Index("Code", Name = "idx_cmn_operation_type_code", IsUnique = true)]
[Index("StateId", Name = "idx_cmn_operation_type_state_id")]
public partial class CmnOperationType
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

    [InverseProperty("OperationType")]
    public virtual ICollection<AccPaymentPurpose> AccPaymentPurposes { get; set; } = new List<AccPaymentPurpose>();

    [InverseProperty("OperationType")]
    public virtual ICollection<AccRegEntry> AccRegEntries { get; set; } = new List<AccRegEntry>();

    [InverseProperty("OperationType")]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    [InverseProperty("OperationType")]
    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();

    [InverseProperty("OperationType")]
    public virtual ICollection<CounterpartyRegBalance> CounterpartyRegBalances { get; set; } = new List<CounterpartyRegBalance>();

    [InverseProperty("OperationType")]
    public virtual ICollection<InvRegBalance> InvRegBalances { get; set; } = new List<InvRegBalance>();

    [InverseProperty("OperationType")]
    public virtual ICollection<MoneyRegBalance> MoneyRegBalances { get; set; } = new List<MoneyRegBalance>();

    [ForeignKey("StateId")]
    [InverseProperty("CmnOperationTypes")]
    public virtual CmnState State { get; set; } = null!;
}
