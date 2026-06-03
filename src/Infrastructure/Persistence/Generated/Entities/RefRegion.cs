using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class RefRegion
{
    public int Id { get; set; }

    public string ShortName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<RefDistrict> RefDistricts { get; set; } = new List<RefDistrict>();

    public virtual RefState State { get; set; } = null!;
}
