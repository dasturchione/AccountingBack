using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaReceipts;

public class FaReceiptDtoProjection : IProjectionBuilder<FaReceiptDoc, FaReceiptDto>
{
    public Expression<Func<FaReceiptDoc, FaReceiptDto>> Build() =>
        receipt => new FaReceiptDto
        {
            Id = receipt.Id,
            OrganizationId = receipt.OrganizationId,
            OrganizationName = receipt.Organization.ShortName,
            DocNumber = receipt.DocNumber,
            DocDate = receipt.DocDate,
            CounterpartyId = receipt.CounterpartyId,
            CounterpartyName = receipt.Counterparty != null ? receipt.Counterparty.ShortName : null,
            CurrencyId = receipt.CurrencyId,
            CurrencyName = receipt.Currency.Name,
            TotalAmount = receipt.TotalAmount,
            VatAmount = receipt.VatAmount,
            FinalAmount = receipt.FinalAmount,
            StatusId = receipt.StatusId,
            StatusName = receipt.Status.Name,
            ReceiptTypeId = receipt.ReceiptTypeId,
            ReceiptTypeCode = receipt.ReceiptType.Code,
            ReceiptTypeName = receipt.ReceiptType.Name,
            SupplierAccountId = receipt.SupplierAccountId,
            SupplierAccountNumber = receipt.SupplierAccount != null ? receipt.SupplierAccount.Number : null,
            SupplierAccountName = receipt.SupplierAccount != null ? receipt.SupplierAccount.Name : null,
            StateId = receipt.StateId,
            StateName = receipt.State.FullName,
            CreatedDate = receipt.CreatedDate,
            UpdatedDate = receipt.UpdatedDate,
            PostedAt = receipt.PostedAt,
            PostedByUserId = receipt.PostedByUserId,
            CancelledAt = receipt.CancelledAt,
            CancelledByUserId = receipt.CancelledByUserId,
            Lines = receipt.Lines.Select(line => new FaReceiptLineDto
            {
                Id = line.Id,
                ReceiptDocId = line.ReceiptDocId,
                Name = line.Name,
                Quantity = line.Quantity,
                Price = line.Price,
                Amount = line.Amount,
                VatRateId = line.VatRateId,
                VatRateName = line.VatRate != null ? line.VatRate.Name : null,
                VatAmount = line.VatAmount,
                TotalAmount = line.TotalAmount,
                CapitalInvestmentAccountId = line.CapitalInvestmentAccountId,
                CapitalInvestmentAccountNumber = line.CapitalInvestmentAccount != null
                    ? line.CapitalInvestmentAccount.Number
                    : null,
                CapitalInvestmentAccountName = line.CapitalInvestmentAccount != null
                    ? line.CapitalInvestmentAccount.Name
                    : null,
                VatAccountId = line.VatAccountId,
                VatAccountNumber = line.VatAccount != null ? line.VatAccount.Number : null,
                VatAccountName = line.VatAccount != null ? line.VatAccount.Name : null,
                Assets = line.Assets.Select(asset => new FaReceiptAssetDto
                {
                    Id = asset.Id,
                    ReceiptDocLineId = asset.ReceiptDocLineId,
                    FaAssetId = asset.FaAssetId,
                    InventoryNumber = asset.InventoryNumber,
                    Name = asset.Name,
                    InitialCost = asset.InitialCost,
                    FaGroupId = asset.FaGroupId,
                    FaGroupCode = asset.FaGroup.Code,
                    FaGroupName = asset.FaGroup.Name,
                    OkofId = asset.OkofId,
                    OkofCode = asset.Okof != null ? asset.Okof.Code : null,
                    OkofName = asset.Okof != null ? asset.Okof.Name : null,
                    AssetAccountId = asset.AssetAccountId,
                    AssetAccountNumber = asset.AssetAccount.Number,
                    AssetAccountName = asset.AssetAccount.Name
                }).ToList()
            }).ToList()
        };
}
