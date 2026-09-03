using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("rnt_rental_object_type_translation")]
[PrimaryKey(nameof(RentalObjectTypeId), nameof(LanguageId))]
public sealed class RentalObjectTypeTranslation
{
    [Column("rental_object_type_id")]
    public short RentalObjectTypeId { get; set; }

    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [ForeignKey(nameof(RentalObjectTypeId))]
    public RentalObjectType RentalObjectType { get; set; } = null!;

    [ForeignKey(nameof(LanguageId))]
    public Language Language { get; set; } = null!;
}
