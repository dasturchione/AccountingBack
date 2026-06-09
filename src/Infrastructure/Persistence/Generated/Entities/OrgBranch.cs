using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class OrgBranch
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int? RegionId { get; set; }

    public int? DistrictId { get; set; }

    public string? Address { get; set; }

    public string? PhoneNumber { get; set; }

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<CashBox> CashBoxes { get; set; } = new List<CashBox>();

    public virtual CmnDistrict? District { get; set; }

    public virtual ICollection<InvWarehouse> InvWarehouses { get; set; } = new List<InvWarehouse>();

    public virtual ICollection<OrgDepartment> OrgDepartments { get; set; } = new List<OrgDepartment>();

    public virtual OrgOrganization Organization { get; set; } = null!;

    public virtual CmnRegion? Region { get; set; }

    public virtual CmnState State { get; set; } = null!;
}
