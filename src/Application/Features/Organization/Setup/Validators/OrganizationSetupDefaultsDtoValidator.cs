using FluentValidation;

namespace Application.Features.OrganizationSetup;

public sealed class OrganizationSetupDefaultsDtoValidator
    : AbstractValidator<OrganizationSetupDefaultsDto>
{
    public OrganizationSetupDefaultsDtoValidator()
    {
        RuleFor(dto => dto.BranchId).GreaterThan(0).When(dto => dto.BranchId.HasValue);
        RuleFor(dto => dto.WarehouseId).GreaterThan(0).When(dto => dto.WarehouseId.HasValue);
        RuleFor(dto => dto.CashBoxId).GreaterThan(0).When(dto => dto.CashBoxId.HasValue);
        RuleFor(dto => dto.BankAccountId).GreaterThan(0).When(dto => dto.BankAccountId.HasValue);
        RuleFor(dto => dto.ReceivableAccountId).GreaterThan(0).When(dto => dto.ReceivableAccountId.HasValue);
        RuleFor(dto => dto.PayableAccountId).GreaterThan(0).When(dto => dto.PayableAccountId.HasValue);
        RuleFor(dto => dto.InventoryAccountId).GreaterThan(0).When(dto => dto.InventoryAccountId.HasValue);
        RuleFor(dto => dto.CashAccountId).GreaterThan(0).When(dto => dto.CashAccountId.HasValue);
        RuleFor(dto => dto.BankAccountingAccountId).GreaterThan(0).When(dto => dto.BankAccountingAccountId.HasValue);
        RuleFor(dto => dto.RevenueAccountId).GreaterThan(0).When(dto => dto.RevenueAccountId.HasValue);
        RuleFor(dto => dto.ExpenseAccountId).GreaterThan(0).When(dto => dto.ExpenseAccountId.HasValue);
        RuleFor(dto => dto.CogsAccountId).GreaterThan(0).When(dto => dto.CogsAccountId.HasValue);
    }
}
