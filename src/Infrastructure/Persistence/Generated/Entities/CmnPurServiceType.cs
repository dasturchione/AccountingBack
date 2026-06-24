using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_pur_service_type")]
[Index("AccountId", Name = "idx_cmn_pur_service_type_account_id")]
public partial class CmnPurServiceType
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
    [InverseProperty("CmnPurServiceTypes")]
    public virtual AccChartAccount Account { get; set; } = null!;

    [InverseProperty("ServiceType")]
    public virtual ICollection<PurService> PurServices { get; set; } = new List<PurService>();

    [ForeignKey("StateId")]
    [InverseProperty("CmnPurServiceTypes")]
    public virtual CmnState State { get; set; } = null!;
}
