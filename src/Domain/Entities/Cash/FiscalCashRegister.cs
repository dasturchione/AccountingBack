using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("fiscal_cash_register")]
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

    [ForeignKey(nameof(OrganizationId))]
    [InverseProperty(nameof(Organization.FiscalCashRegisters))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(RegisterTypeId))]
    [InverseProperty(nameof(FiscalCashRegisterType.FiscalCashRegisters))]
    public virtual FiscalCashRegisterType RegisterType { get; set; } = null!;

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(State.FiscalCashRegisters))]
    public virtual State State { get; set; } = null!;

    [ForeignKey(nameof(WarehouseId))]
    [InverseProperty(nameof(Warehouse.FiscalCashRegisters))]
    public virtual Warehouse? Warehouse { get; set; }
}
