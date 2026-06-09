using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class CmnLanguage
{
    public short Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string NativeName { get; set; } = null!;

    public bool IsDefault { get; set; }

    public int SortOrder { get; set; }

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<CmnTranslation> CmnTranslations { get; set; } = new List<CmnTranslation>();

    public virtual ICollection<OrgOrganization> OrgOrganizations { get; set; } = new List<OrgOrganization>();

    public virtual CmnState State { get; set; } = null!;

    public virtual ICollection<SysUser> SysUsers { get; set; } = new List<SysUser>();
}
