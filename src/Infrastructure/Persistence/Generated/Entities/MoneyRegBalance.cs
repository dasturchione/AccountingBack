using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("money_reg_balance")]
[Index("CurrencyId", Name = "idx_money_reg_balance_currency_id")]
[Index("DocDate", Name = "idx_money_reg_balance_doc_date")]
[Index("DocumentTypeId", "DocumentId", Name = "idx_money_reg_balance_document")]
[Index("OrganizationId", Name = "idx_money_reg_balance_organization_id")]
[Index("SourceType", "SourceId", Name = "idx_money_reg_balance_source")]
public partial class MoneyRegBalance
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("document_type_id")]
    public short DocumentTypeId { get; set; }

    [Column("document_id")]
    public long DocumentId { get; set; }

    [Column("source_type")]
    [StringLength(20)]
    public string SourceType { get; set; } = null!;

    [Column("source_id")]
    public int SourceId { get; set; }

    [Column("operation_type_id")]
    public short OperationTypeId { get; set; }

    [Column("currency_id")]
    public short CurrencyId { get; set; }

    [Column("amount")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Column("doc_date", TypeName = "timestamp without time zone")]
    public DateTime DocDate { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("CurrencyId")]
    [InverseProperty("MoneyRegBalances")]
    public virtual CmnCurrency Currency { get; set; } = null!;

    [ForeignKey("DocumentTypeId")]
    [InverseProperty("MoneyRegBalances")]
    public virtual CmnDocumentType DocumentType { get; set; } = null!;

    [ForeignKey("OperationTypeId")]
    [InverseProperty("MoneyRegBalances")]
    public virtual CmnOperationType OperationType { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("MoneyRegBalances")]
    public virtual OrgOrganization Organization { get; set; } = null!;
}
