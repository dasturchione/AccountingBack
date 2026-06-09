using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class AccRegEntrySubkonto
{
    public long Id { get; set; }

    public long EntryId { get; set; }

    public string Side { get; set; } = null!;

    public short SubkontoTypeId { get; set; }

    public int SortOrder { get; set; }

    public long? EntityId { get; set; }

    public string? DisplayValue { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual AccRegEntry Entry { get; set; } = null!;

    public virtual AccSubkontoType SubkontoType { get; set; } = null!;
}
