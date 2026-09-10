using SharedKernel.Constants;
using SharedKernel.Filters;

namespace Application.Features.Pay.Taxes;

public class PayrollTaxDefinitionBaseDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string TaxType { get; set; } = PayrollTaxTypeConst.Withholding;
    public string BaseType { get; set; } = PayrollTaxBaseTypeConst.Gross;
    public decimal Rate { get; set; }
    public decimal? ExemptionAmount { get; set; }
    public decimal? LimitAmount { get; set; }
    public int LiabilityAccountId { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
}

public sealed class PayrollTaxDefinitionCreateDto : PayrollTaxDefinitionBaseDto;
public sealed class PayrollTaxDefinitionUpdateDto : PayrollTaxDefinitionBaseDto;

public sealed class PayrollTaxDefinitionListFilter : ISearchFilter, IPaginationFilter
{
    public string? Search { get; set; }
    public string? TaxType { get; set; }
    public DateOnly? EffectiveOn { get; set; }
    public short? StateId { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}

public class PayrollTaxDefinitionDto : PayrollTaxDefinitionBaseDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string? LiabilityAccountNumber { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

public sealed class PayrollTaxDefinitionListDto : PayrollTaxDefinitionDto;
