using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.BankOperations;

public class BankOperationDtoProjection : IProjectionBuilder<BankOperation, BankOperationDto>
{
    public Expression<Func<BankOperation, BankOperationDto>> Build() =>
        x => new BankOperationDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            BankAccountId = x.BankAccountId,
            BankAccountNumber = x.BankAccount.AccountNumber,
            BankAccountInn = x.BankAccount.Bank.Inn, 
            BankAccountName = x.BankAccount.Bank.Name,
            BankId = x.BankAccount.BankId,
            BankName = x.BankAccount.Bank.Name,
            BankInn = x.BankAccount.Bank.Inn,
            BankMfo = x.BankAccount.Bank.Mfo,
            OperationTypeId = x.OperationTypeId,
            OperationTypeName = x.OperationType.Name,
            PaymentTypeId = x.PaymentTypeId,
            PaymentTypeName = x.PaymentType != null ? x.PaymentType.Name : null,
            PaymentPurposeId = x.PaymentPurposeId,
            PaymentPurposeName = x.PaymentPurpose.Name,
            CounterpartyId = x.CounterpartyId,
            CounterpartyName = x.Counterparty != null ? x.Counterparty.ShortName : null,
            CounterpartyInn = x.Counterparty != null ? x.Counterparty.Inn : null,
            CounterpartyBankAccountId = x.CounterpartyBankAccountId,
            CounterpartyBankAccountNumber = x.CounterpartyBankAccount == null ? null : x.CounterpartyBankAccount.AccountNumber,
            DocNumber = x.DocNumber,
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
            CreatedDate = x.CreatedDate,
            ContractId = x.ContractId,
            ContractNumber = x.Contract != null ? x.Contract.ContractNumber : null
        };
}
