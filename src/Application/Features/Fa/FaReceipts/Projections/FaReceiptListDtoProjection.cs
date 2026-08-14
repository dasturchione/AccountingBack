using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaReceipts;

public class FaReceiptListDtoProjection : IProjectionBuilder<FaReceiptDoc, FaReceiptListDto>
{
    public Expression<Func<FaReceiptDoc, FaReceiptListDto>> Build() =>
        receipt => new FaReceiptListDto
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
            ReceiptTypeName = receipt.ReceiptType.Name,
            SupplierAccountId = receipt.SupplierAccountId,
            StateId = receipt.StateId,
            StateName = receipt.State.FullName,
            UpdatedDate = receipt.UpdatedDate
        };
}
