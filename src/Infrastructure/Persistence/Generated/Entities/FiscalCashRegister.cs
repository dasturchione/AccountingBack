using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("fiscal_cash_register")]
[Index("OrganizationId", Name = "ix_fiscal_cash_register_organization_id")]
[Index("OrganizationId", "StateId", Name = "ix_fiscal_cash_register_organization_state_id")]
[Index("RegisterTypeId", Name = "ix_fiscal_cash_register_register_type_id")]
[Index("StateId", Name = "ix_fiscal_cash_register_state_id")]
[Index("WarehouseId", Name = "ix_fiscal_cash_register_warehouse_id")]
public partial class FiscalCashRegister
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("warehouse_id")]
    public int? WarehouseId { get; set; }

    [Column("register_type_id")]
    public short RegisterTypeId { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("external_register_id")]
    [StringLength(100)]
    public string? ExternalRegisterId { get; set; }

    [Column("model")]
    [StringLength(100)]
    public string? Model { get; set; }

    [Column("serial_number")]
    [StringLength(100)]
    public string? SerialNumber { get; set; }

    [Column("fiscal_module_number")]
    [StringLength(100)]
    public string? FiscalModuleNumber { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("FiscalCashRegisters")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("RegisterTypeId")]
    [InverseProperty("FiscalCashRegisters")]
    public virtual FiscalCashRegisterType RegisterType { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("FiscalCashRegisters")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("WarehouseId")]
    [InverseProperty("FiscalCashRegisters")]
    public virtual InvWarehouse? Warehouse { get; set; }
}
