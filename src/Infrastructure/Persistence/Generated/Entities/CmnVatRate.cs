using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class CmnVatRate
{
    public short Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public decimal Rate { get; set; }

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<PurDocTable> PurDocTables { get; set; } = new List<PurDocTable>();

    public virtual ICollection<SaleDocTable> SaleDocTables { get; set; } = new List<SaleDocTable>();

    public virtual CmnState State { get; set; } = null!;
}
