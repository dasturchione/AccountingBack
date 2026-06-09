using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class AccChartAccount
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public int? ParentId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public bool IsGroup { get; set; }

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public short? AccountTypeId { get; set; }

    public bool IsQuantity { get; set; }

    public bool IsCurrency { get; set; }

    public virtual ICollection<AccChartAccountSubkonto> AccChartAccountSubkontos { get; set; } = new List<AccChartAccountSubkonto>();

    public virtual ICollection<AccPostingRuleLine> AccPostingRuleLineCreditAccounts { get; set; } = new List<AccPostingRuleLine>();

    public virtual ICollection<AccPostingRuleLine> AccPostingRuleLineDebitAccounts { get; set; } = new List<AccPostingRuleLine>();

    public virtual ICollection<AccRegEntry> AccRegEntryCreditAccounts { get; set; } = new List<AccRegEntry>();

    public virtual ICollection<AccRegEntry> AccRegEntryDebitAccounts { get; set; } = new List<AccRegEntry>();

    public virtual AccAccountType? AccountType { get; set; }

    public virtual ICollection<AccChartAccount> InverseParent { get; set; } = new List<AccChartAccount>();

    public virtual OrgOrganization Organization { get; set; } = null!;

    public virtual AccChartAccount? Parent { get; set; }

    public virtual CmnState State { get; set; } = null!;
}
