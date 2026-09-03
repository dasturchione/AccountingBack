using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Manual;

public abstract class NameSelectListDtoOrderByBuilder<TEntity, TResult> : IOrderByBuilder<TEntity, TResult>
    where TResult : SelectListDto
{
    public Func<IQueryable<TResult>, IOrderedQueryable<TResult>> Build() =>
        query => query.OrderBy(x => x.Name).ThenBy(x => x.Id);
}

public sealed class AccountingPolicySelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<AccountingPolicy, SelectListDto>;
public sealed class AccountTypeSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<AccountType, SelectListDto>;
public sealed class BankSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<Bank, SelectListDto>;
public sealed class BankAccountSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<BankAccount, SelectListDto>;
public sealed class PaymentAcceptancePointSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<PaymentAcceptancePoint, SelectListDto>;
public sealed class BranchSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<Branch, SelectListDto>;
public sealed class CashBoxSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<CashBox, SelectListDto>;
public sealed class CashOperationSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<CashOperation, SelectListDto>;
public sealed class ContractSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<Contract, SelectListDto>;
public sealed class ContractTypeSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<ContractType, SelectListDto>;
public sealed class CostingMethodSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<CostingMethod, SelectListDto>;
public sealed class CounterpartyBankAccountSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<CounterpartyBankAccount, SelectListDto>;
public sealed class CounterpartyCardSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<CounterpartyCard, SelectListDto>;
public sealed class CurrencySelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<Currency, SelectListDto>;
public sealed class DepartmentSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<Department, SelectListDto>;
public sealed class DistrictSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<District, SelectListDto>;
public sealed class DocumentStatusSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<DocumentStatus, SelectListDto>;
public sealed class DocumentTypeSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<DocumentType, SelectListDto>;
public sealed class FaDepreciationMethodSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<FaDepreciationMethod, SelectListDto>;
public sealed class FaDisposalTypeSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<FaDisposalType, SelectListDto>;
public sealed class FaGroupSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<FaGroup, SelectListDto>;
public sealed class FaOkofSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<FaOkof, SelectListDto>;
public sealed class FaReceiptTypeSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<FaReceiptType, SelectListDto>;
public sealed class FiscalCashRegisterSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<FiscalCashRegister, SelectListDto>;
public sealed class FiscalCashRegisterTypeSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<FiscalCashRegisterType, SelectListDto>;
public sealed class InventoryAdjustmentTypeSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<InventoryAdjustmentType, SelectListDto>;
public sealed class LanguageSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<Language, SelectListDto>;
public sealed class OperationTypeSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<OperationType, SelectListDto>;
public sealed class OrganizationSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<Organization, SelectListDto>;
public sealed class PaymentMethodSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<PaymentMethod, SelectListDto>;
public sealed class PaymentTypeSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<PaymentType, SelectListDto>;
public sealed class PositionSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<Position, SelectListDto>;
public sealed class PriceRoundingMethodSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<PriceRoundingMethod, SelectListDto>;
public sealed class PricingMethodSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<PricingMethod, SelectListDto>;
public sealed class ProductGroupSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<ProductGroup, SelectListDto>;
public sealed class RegionSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<Region, SelectListDto>;
public sealed class RoleSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<Role, SelectListDto>;
public sealed class StateSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<State, SelectListDto>;
public sealed class SubkontoTypeSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<SubkontoType, SelectListDto>;
public sealed class UnitSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<Unit, SelectListDto>;
public sealed class UserSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<User, SelectListDto>;
public sealed class UserKindSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<UserKind, SelectListDto>;
public sealed class VatRateSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<VatRate, SelectListDto>;
public sealed class WarehouseSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<Warehouse, SelectListDto>;

public sealed class ProductSelectListDtoOrderByBuilder : NameSelectListDtoOrderByBuilder<Product, ProductSelectListDto>;

public sealed class ChartAccountSelectListDtoOrderByBuilder : IOrderByBuilder<ChartAccount, ChartAccountSelectListDto>
{
    public Func<IQueryable<ChartAccountSelectListDto>, IOrderedQueryable<ChartAccountSelectListDto>> Build() =>
        query => query.OrderBy(x => x.Number).ThenBy(x => x.Id);
}

public sealed class FaAssetSelectListDtoOrderByBuilder : IOrderByBuilder<FaAsset, FaAssetSelectListDto>
{
    public Func<IQueryable<FaAssetSelectListDto>, IOrderedQueryable<FaAssetSelectListDto>> Build() =>
        query => query.OrderBy(x => x.InventoryNumber).ThenBy(x => x.Id);
}
