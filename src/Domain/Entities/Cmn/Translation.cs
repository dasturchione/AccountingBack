namespace Domain.Entities;

public partial class Translation
{
    public long Id { get; set; }

    public short LanguageId { get; set; }

    public string TableName { get; set; } = null!;

    public long RecordId { get; set; }

    public string ColumnName { get; set; } = null!;

    public string Value { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public virtual Language Language { get; set; } = null!;
}
