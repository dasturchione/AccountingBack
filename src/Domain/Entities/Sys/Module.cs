using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("sys_module")]
[Index("Code", Name = "sys_module_unique_index_code", IsUnique = true)]
[Index("SubGroupId", Name = "sys_module_unique_index_sub_group_id")]
[Index("IsVisible", Name = "idx_sys_module_is_visible")]
[Index("ParentId", Name = "idx_sys_module_parent_id")]
[Index("SortOrder", Name = "idx_sys_module_sort_order")]
public partial class Module
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("code")]
    [StringLength(100)]
    public string Code { get; set; } = null!;

    [Column("short_name")]
    [StringLength(250)]
    public string ShortName { get; set; } = null!;

    [Column("full_name")]
    [StringLength(300)]
    public string FullName { get; set; } = null!;

    [Column("sub_group_id")]
    public int SubGroupId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }


    [Column("parent_id")]
    public int? ParentId { get; set; }

    [Column("route")]
    [StringLength(250)]
    public string? Route { get; set; }

    [Column("icon")]
    [StringLength(100)]
    public string? Icon { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("is_visible")]
    public bool IsVisible { get; set; }
    [ForeignKey("StateId")]
    [InverseProperty("Modules")]
    public virtual State State { get; set; } = null!;

    [ForeignKey("SubGroupId")]
    [InverseProperty("Modules")]
    public virtual ModuleSubGroup SubGroup { get; set; } = null!;

    [InverseProperty("Module")]
    public virtual ICollection<RoleModule> RoleModules { get; set; } = new List<RoleModule>();
}
