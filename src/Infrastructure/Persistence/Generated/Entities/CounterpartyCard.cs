using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class CounterpartyCard
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public short CounterpartyTypeId { get; set; }

    public string ShortName { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Inn { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public int? RegionId { get; set; }

    public int? DistrictId { get; set; }

    public string? Address { get; set; }

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();

    public virtual ICollection<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; } = new List<CounterpartyBankAccount>();

    public virtual ICollection<CounterpartyContact> CounterpartyContacts { get; set; } = new List<CounterpartyContact>();

    public virtual ICollection<CounterpartyRegBalance> CounterpartyRegBalances { get; set; } = new List<CounterpartyRegBalance>();

    public virtual CmnCounterpartyType CounterpartyType { get; set; } = null!;

    public virtual CmnDistrict? District { get; set; }

    public virtual OrgOrganization Organization { get; set; } = null!;

    public virtual ICollection<PurDoc> PurDocs { get; set; } = new List<PurDoc>();

    public virtual CmnRegion? Region { get; set; }

    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    public virtual CmnState State { get; set; } = null!;
}
