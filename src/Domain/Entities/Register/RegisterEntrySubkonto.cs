namespace Domain.Entities;

public partial class RegisterEntrySubkonto
{
    public long Id { get; set; }
    public long EntryId { get; set; }
    public string Side { get; set; } = null!;
    public short SubkontoTypeId { get; set; }
    public int SortOrder { get; set; }
    public long? EntityId { get; set; }
    public string? DisplayValue { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual AccountingRegisterEntry AccountingRegisterEntry { get; set; } = null!;
    public virtual SubkontoType SubkontoType { get; set; } = null!;
}
