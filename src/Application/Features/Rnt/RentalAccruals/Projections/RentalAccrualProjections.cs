using Application.Features.Rnt.RentalContracts;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Rnt.RentalAccruals;

public sealed class RentalAccrualDocDtoProjection : IProjectionBuilder<RentalAccrualDoc, RentalAccrualDocDto>
{
    public Expression<Func<RentalAccrualDoc, RentalAccrualDocDto>> Build() => x => new()
    {
        Id = x.Id,
        OrganizationId = x.OrganizationId,
        ContractId = x.ContractId,
        ContractNumber = x.Contract.ContractNumber,
        Lessors = x.Contract.Lessors
            .Where(link => link.Lessor.StateId == StateIdConst.ACTIVE)
            .OrderBy(link => link.LessorId)
            .Select(link => new RentalLessorDto
            {
                Id = link.Lessor.Id,
                LessorKindCode = link.Lessor.LessorKindCode,
                CounterpartyId = link.Lessor.CounterpartyId,
                FullName = link.Lessor.FullName,
                Inn = link.Lessor.Inn,
                Pinfl = link.Lessor.Pinfl,
                PhoneNumber = link.Lessor.PhoneNumber,
                RegisteredAddress = link.Lessor.RegisteredAddress,
                ResidentialAddress = link.Lessor.ResidentialAddress
            }).ToList(),
        DocNumber = x.DocNumber,
        DocDate = x.DocDate,
        CurrencyId = x.CurrencyId,
        CurrencyCode = x.Currency.Code,
        ExchangeRate = x.ExchangeRate,
        ContractAmount = x.ContractAmount,
        TaxBaseAmount = x.TaxBaseAmount,
        TaxAmount = x.TaxAmount,
        PayableAmount = x.PayableAmount,
        Amount = x.Amount,
        LessorPayableAccountId = x.LessorPayableAccountId,
        LessorPayableAccountNumber = x.LessorPayableAccount == null ? null : x.LessorPayableAccount.Number,
        LessorPayableAccountName = x.LessorPayableAccount == null ? null : x.LessorPayableAccount.Name,
        TaxPayableAccountId = x.TaxPayableAccountId,
        TaxPayableAccountNumber = x.TaxPayableAccount == null ? null : x.TaxPayableAccount.Number,
        TaxPayableAccountName = x.TaxPayableAccount == null ? null : x.TaxPayableAccount.Name,
        StatusId = x.StatusId,
        StatusName = x.Status.Name,
        Comment = x.Comment,
        CreatedDate = x.CreatedDate,
        PostedAt = x.PostedAt,
        CancelledAt = x.CancelledAt,
        Items = x.Items.OrderBy(i => i.Id).Select(i => new RentalAccrualDocItemDto
        {
            Id = i.Id,
            ContractObjectId = i.ContractObjectId,
            ObjectName = i.ContractObject.ObjectName,
            PeriodFrom = i.PeriodFrom,
            PeriodTo = i.PeriodTo,
            ContractAmount = i.ContractAmount,
            TaxBaseAmount = i.TaxBaseAmount,
            TaxRate = i.TaxRate,
            TaxAmount = i.TaxAmount,
            PayableAmount = i.PayableAmount,
            Amount = i.Amount,
            ExpenseAccountId = i.ExpenseAccountId,
            ExpenseAccountNumber = i.ExpenseAccount == null ? null : i.ExpenseAccount.Number,
            ExpenseAccountName = i.ExpenseAccount == null ? null : i.ExpenseAccount.Name
        }).ToList()
    };
}

public sealed class RentalAccrualDocListDtoProjection : IProjectionBuilder<RentalAccrualDoc, RentalAccrualDocListDto>
{
    public Expression<Func<RentalAccrualDoc, RentalAccrualDocListDto>> Build() => x => new()
    {
        Id = x.Id,
        ContractId = x.ContractId,
        ContractNumber = x.Contract.ContractNumber,
        Lessors = x.Contract.Lessors
            .Where(link => link.Lessor.StateId == StateIdConst.ACTIVE)
            .OrderBy(link => link.LessorId)
            .Select(link => new RentalLessorDto
            {
                Id = link.Lessor.Id,
                LessorKindCode = link.Lessor.LessorKindCode,
                CounterpartyId = link.Lessor.CounterpartyId,
                FullName = link.Lessor.FullName,
                Inn = link.Lessor.Inn,
                Pinfl = link.Lessor.Pinfl,
                PhoneNumber = link.Lessor.PhoneNumber,
                RegisteredAddress = link.Lessor.RegisteredAddress,
                ResidentialAddress = link.Lessor.ResidentialAddress
            }).ToList(),
        DocNumber = x.DocNumber,
        DocDate = x.DocDate,
        CurrencyCode = x.Currency.Code,
        TaxAmount = x.TaxAmount,
        PayableAmount = x.PayableAmount,
        Amount = x.Amount,
        StatusId = x.StatusId,
        StatusName = x.Status.Name
    };
}
