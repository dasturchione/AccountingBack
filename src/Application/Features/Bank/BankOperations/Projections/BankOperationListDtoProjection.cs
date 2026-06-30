using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.BankOperations;

public class BankOperationListDtoProjection : IProjectionBuilder<BankOperation, BankOperationListDto>
{
    public Expression<Func<BankOperation, BankOperationListDto>> Build() =>
        x => new BankOperationListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            BankAccountId = x.BankAccountId,
            BankAccountNumber = x.BankAccount.AccountNumber,
            OperationTypeId = x.OperationTypeId,
            OperationTypeName = x.OperationType.Name,
            PaymentTypeId = x.PaymentTypeId,
            PaymentTypeName = x.PaymentType != null ? x.PaymentType.Name : null,
            CounterpartyId = x.CounterpartyId,
            CounterpartyName = x.Counterparty != null ? x.Counterparty.ShortName : null,
            CounterpartyInn = x.Counterparty != null ? x.Counterparty.Inn : null,
            CounterpartyBankAccountId = x.CounterpartyBankAccount != null ? x.CounterpartyBankAccountId : null,
            CounterpartyBankAccountNumber = x.CounterpartyBankAccount != null ? x.CounterpartyBankAccount.AccountNumber : null,
            DocNumber = x.DocNumber,
            BankId = x.BankAccount.BankId,
            BankInn = x.BankAccount.Bank.Inn,
            BankMfo = x.BankAccount.Bank.Mfo,
            BankName = x.BankAccount.Bank.Name,
            DocDate = x.DocDate,
            CurrencyId = x.CurrencyId,
            CurrencyName = x.Currency.Name,
            Amount = x.Amount,
            ExchangeRate = x.ExchangeRate,
            PostedAt = x.PostedAt,
            PostedByUserId = x.PostedByUserId,
            CancelledAt = x.CancelledAt,
            CancelledByUserId = x.CancelledByUserId,
            Comment = x.Comment,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
