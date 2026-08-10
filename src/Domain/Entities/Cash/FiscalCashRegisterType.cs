using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("fiscal_cash_register_type")]
public partial class FiscalCashRegisterType
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [InverseProperty(nameof(FiscalCashRegisterTypeTranslation.CashRegisterType))]
    public virtual ICollection<FiscalCashRegisterTypeTranslation> FiscalCashRegisterTypeTranslations { get; set; } = new List<FiscalCashRegisterTypeTranslation>();

    [InverseProperty((nameof(FiscalCashRegister.RegisterType)))]
    public virtual ICollection<FiscalCashRegister> FiscalCashRegisters { get; set; } = new List<FiscalCashRegister>();
}
