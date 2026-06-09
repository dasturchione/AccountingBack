using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class AccChartAccountSubkonto
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public int AccountId { get; set; }

    public short SubkontoTypeId { get; set; }

    public int SortOrder { get; set; }

    public bool IsRequired { get; set; }

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual AccChartAccount Account { get; set; } = null!;

    public virtual OrgOrganization Organization { get; set; } = null!;

    public virtual CmnState State { get; set; } = null!;

    public virtual AccSubkontoType SubkontoType { get; set; } = null!;
}
