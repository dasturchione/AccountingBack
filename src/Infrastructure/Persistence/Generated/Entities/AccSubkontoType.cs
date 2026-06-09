using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class AccSubkontoType
{
    public short Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string SourceTable { get; set; } = null!;

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<AccChartAccountSubkonto> AccChartAccountSubkontos { get; set; } = new List<AccChartAccountSubkonto>();

    public virtual ICollection<AccRegEntrySubkonto> AccRegEntrySubkontos { get; set; } = new List<AccRegEntrySubkonto>();

    public virtual CmnState State { get; set; } = null!;
}
