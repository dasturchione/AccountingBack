using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class SysUser
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string PasswordSalt { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string? Email { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public int RoleId { get; set; }

    public DateTime? LastAccessTime { get; set; }

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public short? LanguageId { get; set; }

    public virtual ICollection<InvWarehouse> InvWarehouses { get; set; } = new List<InvWarehouse>();

    public virtual CmnLanguage? Language { get; set; }

    public virtual SysRole Role { get; set; } = null!;

    public virtual CmnState State { get; set; } = null!;
}
