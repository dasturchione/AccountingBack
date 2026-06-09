using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class AccPostingRule
{
    public int Id { get; set; }

    public int? OrganizationId { get; set; }

    public short DocumentTypeId { get; set; }

    public short? OperationTypeId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<AccPostingRuleLine> AccPostingRuleLines { get; set; } = new List<AccPostingRuleLine>();

    public virtual CmnDocumentType DocumentType { get; set; } = null!;

    public virtual CmnOperationType? OperationType { get; set; }

    public virtual OrgOrganization? Organization { get; set; }

    public virtual CmnState State { get; set; } = null!;
}
