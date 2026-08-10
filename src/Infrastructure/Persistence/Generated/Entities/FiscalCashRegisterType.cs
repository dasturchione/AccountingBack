using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("fiscal_cash_register_type")]
[Index("Code", Name = "fiscal_cash_register_type_code_key", IsUnique = true)]
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

    [InverseProperty("CashRegisterType")]
    public virtual ICollection<FiscalCashRegisterTypeTranslation> FiscalCashRegisterTypeTranslations { get; set; } = new List<FiscalCashRegisterTypeTranslation>();

    [InverseProperty("RegisterType")]
    public virtual ICollection<FiscalCashRegister> FiscalCashRegisters { get; set; } = new List<FiscalCashRegister>();
}
