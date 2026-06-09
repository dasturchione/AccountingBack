using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class CmnTranslation
{
    public long Id { get; set; }

    public short LanguageId { get; set; }

    public string TableName { get; set; } = null!;

    public long RecordId { get; set; }

    public string ColumnName { get; set; } = null!;

    public string Value { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public virtual CmnLanguage Language { get; set; } = null!;
}
