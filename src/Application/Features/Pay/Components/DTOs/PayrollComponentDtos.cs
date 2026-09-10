using SharedKernel.Filters;
using SharedKernel.Constants;

namespace Application.Features.Pay.Components;

public class PayrollComponentBaseDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string ComponentType { get; set; } = null!;
    public string CalculationMethod { get; set; } = null!;
    public string ProrationBasis { get; set; } = PayrollProrationBasisConst.Days;
    public decimal? DefaultAmount { get; set; }
    public decimal? DefaultRate { get; set; }
    public int? DependsOnComponentId { get; set; }
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
    public bool IsTaxable { get; set; } = true;
    public bool IsMandatory { get; set; }
    public int? ExpenseAccountId { get; set; }
    public int? LiabilityAccountId { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public int SortOrder { get; set; } = 1;
}

public sealed class PayrollComponentCreateDto : PayrollComponentBaseDto;
public sealed class PayrollComponentUpdateDto : PayrollComponentBaseDto;

public sealed class PayrollComponentListFilter : ISearchFilter, IPaginationFilter
{
    public string? Search { get; set; }
    public string? ComponentType { get; set; }
    public DateOnly? EffectiveOn { get; set; }
    public short? StateId { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}

public class PayrollComponentDto : PayrollComponentBaseDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string? ExpenseAccountNumber { get; set; }
    public string? LiabilityAccountNumber { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

public sealed class PayrollComponentListDto : PayrollComponentDto;
