using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("bank_terminal")]
[Index("BankAccountId", Name = "ix_bank_terminal_bank_account_id")]
[Index("OrganizationId", Name = "ix_bank_terminal_organization_id")]
[Index("OrganizationId", "StateId", Name = "ix_bank_terminal_organization_id_state_id")]
[Index("StateId", Name = "ix_bank_terminal_state_id")]
public partial class BankTerminal
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("bank_account_id")]
    public int? BankAccountId { get; set; }

    [Column("name")]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [Column("merchant_id")]
    [StringLength(100)]
    public string? MerchantId { get; set; }

    [Column("external_terminal_id")]
    [StringLength(100)]
    public string? ExternalTerminalId { get; set; }

    [Column("serial_number")]
    [StringLength(100)]
    public string? SerialNumber { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("BankAccountId")]
    [InverseProperty("BankTerminals")]
    public virtual OrgBankAccount? BankAccount { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("BankTerminals")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("BankTerminals")]
    public virtual CmnState State { get; set; } = null!;
}
