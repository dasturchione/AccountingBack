using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class AccAccountType
{
    public short Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<AccChartAccount> AccChartAccounts { get; set; } = new List<AccChartAccount>();

    public virtual CmnState State { get; set; } = null!;
}
