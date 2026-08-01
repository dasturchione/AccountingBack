using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaReceipts;

public class FaReceiptDtoProjection : IProjectionBuilder<FaReceiptDoc, FaReceiptDto>
{
    public Expression<Func<FaReceiptDoc, FaReceiptDto>> Build() =>
        x => new FaReceiptDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            DocNumber = x.DocNumber,
            DocDate = x.DocDate,
            CounterpartyId = x.CounterpartyId,
            CounterpartyName = x.Counterparty != null ? x.Counterparty.ShortName : null,
            WarehouseId = x.WarehouseId,
            WarehouseName = x.Warehouse != null ? x.Warehouse.Name : null,
            CurrencyId = x.CurrencyId,
            CurrencyName = x.Currency.Name,
            TotalAmount = x.TotalAmount,
            VatAmount = x.VatAmount,
            FinalAmount = x.FinalAmount,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            ReceiptType = x.ReceiptType,
            SupplierAccountId = x.SupplierAccountId,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate,
            UpdatedDate = x.UpdatedDate,
            PostedAt = x.PostedAt,
            PostedByUserId = x.PostedByUserId,
            CancelledAt = x.CancelledAt,
            CancelledByUserId = x.CancelledByUserId,
            Lines = x.Lines.Select(line => new FaReceiptLineDto
            {
                Id = line.Id,
                OwnerId = line.OwnerId,
                SourceProductId = line.SourceProductId,
                SourceProductName = line.SourceProduct != null ? line.SourceProduct.Name : null,
                Name = line.Name,
                Quantity = line.Quantity,
                Price = line.Price,
                Amount = line.Amount,
                VatRateId = line.VatRateId,
                VatRateName = line.VatRate != null ? line.VatRate.Name : null,
                VatAmount = line.VatAmount,
                TotalAmount = line.TotalAmount,
                CapitalInvestmentAccountId = line.CapitalInvestmentAccountId,
                VatAccountId = line.VatAccountId,
                Assets = line.Assets.Select(asset => new FaReceiptAssetDto
                {
                    Id = asset.Id,
                    OwnerId = asset.OwnerId,
                    FaAssetId = asset.FaAssetId,
                    InventoryNumber = asset.InventoryNumber,
                    Name = asset.Name,
                    InitialCost = asset.InitialCost,
                    SalvageValue = asset.SalvageValue,
                    UsefulLifeMonths = asset.UsefulLifeMonths,
                    DepreciationMethodId = asset.DepreciationMethodId,
                    DepreciationMethodCode = asset.DepreciationMethod.Code,
                    DepreciationMethodName = asset.DepreciationMethod.Name,
                    FaGroupId = asset.FaGroupId,
                    FaGroupCode = asset.FaGroup.Code,
                    FaGroupName = asset.FaGroup.Name,
                    OkofId = asset.OkofId,
                    OkofCode = asset.Okof != null ? asset.Okof.Code : null,
                    OkofName = asset.Okof != null ? asset.Okof.Name : null,
                    CommissioningDate = asset.CommissioningDate,
                    DeprStartDate = asset.DeprStartDate,
                    PlannedUnitsTotal = asset.PlannedUnitsTotal,
                    DepartmentId = asset.DepartmentId,
                    DepartmentName = asset.Department != null ? asset.Department.Name : null,
                    ResponsibleUserId = asset.ResponsibleUserId,
                    AssetAccountId = asset.AssetAccountId,
                    AccumulatedDepreciationAccountId = asset.AccumulatedDepreciationAccountId,
                    DepreciationExpenseAccountId = asset.DepreciationExpenseAccountId,
                    ResponsibleUserName = asset.ResponsibleUser != null
                        ? asset.ResponsibleUser.FirstName + " " + asset.ResponsibleUser.LastName
                        : null
                }).ToList()
            }).ToList()
        };
}
