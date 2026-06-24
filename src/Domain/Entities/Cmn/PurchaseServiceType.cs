using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_pur_service_type")]
[Index("AccountId", Name = "idx_cmn_pur_service_type_account_id")]
public partial class PurchaseServiceType
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("account_id")]
    public int AccountId { get; set; }

    [Column("vat_applicable")]
    public bool VatApplicable { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("PurchaseServiceTypes")]
    public virtual ChartAccount Account { get; set; } = null!;

    [InverseProperty("ServiceType")]
    public virtual ICollection<PurchaseService> PurchaseServices { get; set; } = new List<PurchaseService>();

    [ForeignKey("StateId")]
    [InverseProperty("PurchaseServiceTypes")]
    public virtual State State { get; set; } = null!;
}
