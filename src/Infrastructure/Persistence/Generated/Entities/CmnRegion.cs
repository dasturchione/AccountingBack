using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class CmnRegion
{
    public int Id { get; set; }

    public string ShortName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<CmnDistrict> CmnDistricts { get; set; } = new List<CmnDistrict>();

    public virtual CmnState State { get; set; } = null!;
}
