using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class OrgBankAccount
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public int BankId { get; set; }

    public string AccountNumber { get; set; } = null!;

    public short CurrencyId { get; set; }

    public bool IsMain { get; set; }

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual CmnBank Bank { get; set; } = null!;

    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    public virtual CmnCurrency Currency { get; set; } = null!;

    public virtual OrgOrganization Organization { get; set; } = null!;

    public virtual CmnState State { get; set; } = null!;
}
