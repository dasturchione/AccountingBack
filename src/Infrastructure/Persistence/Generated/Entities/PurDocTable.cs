using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class PurDocTable
{
    public long Id { get; set; }

    public long OwnerId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal Amount { get; set; }

    public short? VatRateId { get; set; }

    public decimal VatAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public virtual PurDoc Owner { get; set; } = null!;

    public virtual InvProduct Product { get; set; } = null!;

    public virtual CmnVatRate? VatRate { get; set; }
}
