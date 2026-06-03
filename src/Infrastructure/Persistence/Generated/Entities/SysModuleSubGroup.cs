using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class SysModuleSubGroup
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string ShortName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<SysModule> SysModules { get; set; } = new List<SysModule>();
}
