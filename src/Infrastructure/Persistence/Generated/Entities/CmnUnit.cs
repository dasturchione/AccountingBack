using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class CmnUnit
{
    public short Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public short StateId { get; set; }

    public virtual ICollection<InvProduct> InvProducts { get; set; } = new List<InvProduct>();

    public virtual CmnState State { get; set; } = null!;
}
