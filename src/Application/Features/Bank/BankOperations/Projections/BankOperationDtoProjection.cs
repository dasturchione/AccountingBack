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
            DirectionId = x.DirectionId,
            DirectionName = x.Direction.Name,
            PaymentTypeId = x.PaymentTypeId,
            PaymentTypeName = x.PaymentType != null ? x.PaymentType.Name : null,
            BankChartAccountId = x.BankChartAccountId,
            BankChartAccountNumber = x.BankChartAccount == null ? null : x.BankChartAccount.Number,
            BankChartAccountName = x.BankChartAccount == null ? null : x.BankChartAccount.Name,
            OffsetAccountId = x.OffsetAccountId,
            OffsetAccountNumber = x.OffsetAccount == null ? null : x.OffsetAccount.Number,
            OffsetAccountName = x.OffsetAccount == null ? null : x.OffsetAccount.Name,
            CounterpartyId = x.CounterpartyId,
            CounterpartyName = x.Counterparty != null ? x.Counterparty.ShortName : null,
            CounterpartyInn = x.Counterparty != null ? x.Counterparty.Inn : null,
            CounterpartyBankAccountId = x.CounterpartyBankAccountId,
            CounterpartyBankAccountNumber = x.CounterpartyBankAccount == null ? null : x.CounterpartyBankAccount.AccountNumber,
            DocNumber = x.DocNumber,
            BankDocumentNumber = x.BankDocumentNumber,
            ClassificationCategoryId = x.ClassificationCategoryId,
            ClassificationCode = x.ClassificationCategory == null ? null : x.ClassificationCategory.Code,
            ClassificationName = x.ClassificationCategory == null ? null : x.ClassificationCategory.Name,
            ClassificationRuleId = x.ClassificationRuleId,
            ClassificationRuleCode = x.ClassificationRule == null ? null : x.ClassificationRule.Code,
            RelatedDocumentId = x.RelatedDocumentId,
            RelatedDocumentTypeId = x.RelatedDocument == null ? null : x.RelatedDocument.DocumentTypeId,
            RelatedDocumentTypeName = x.RelatedDocument == null ? null : x.RelatedDocument.DocumentType.Name,
            RelatedDocumentEntityId = x.RelatedDocument == null ? null : x.RelatedDocument.DocumentId,
            RelatedDocumentNumber = x.RelatedDocument == null ? null : x.RelatedDocument.DocNumber,
            RelatedDocumentDate = x.RelatedDocument == null ? null : x.RelatedDocument.DocDate,
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
