using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Rnt.RentalContracts;

public sealed class RentalContractDtoProjection(IUserContext userContext) : IProjectionBuilder<RentalContract, RentalContractDto>
{
    public Expression<Func<RentalContract, RentalContractDto>> Build()
    {
        var languageId = userContext.LanguageId ?? LanguageIdConst.UZ;
        return x => new()
        {
        Id = x.Id,
        OrganizationId = x.OrganizationId,
        LessorFullName = x.LessorFullName,
        LessorInn = x.LessorInn,
        LessorPinfl = x.LessorPinfl,
        ContractNumber = x.ContractNumber,
        ContractDate = x.ContractDate,
        StartDate = x.StartDate,
        EndDate = x.EndDate,
        CurrencyId = x.CurrencyId,
        CurrencyCode = x.Currency.Code,
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
        Objects = x.Objects
            .Where(o => o.StateId == StateIdConst.ACTIVE)
            .OrderBy(o => o.Id)
            .Select(o => new RentalContractObjectDto
            {
                Id = o.Id,
                RentalObjectTypeId = o.RentalObjectTypeId,
                RentalObjectTypeCode = o.RentalObjectType.Code,
                RentalObjectTypeName = o.RentalObjectType.Translations
                    .Where(t => t.LanguageId == languageId)
                    .Select(t => t.Name)
                    .FirstOrDefault() ?? o.RentalObjectType.Code,
                ObjectName = o.ObjectName,
                ObjectIdentifier = o.ObjectIdentifier,
                ObjectAddress = o.ObjectAddress,
                StartDate = o.StartDate,
                EndDate = o.EndDate,
                PeriodUnit = o.PeriodUnit,
                PeriodValue = o.PeriodValue,
                NextAccrualDate = o.NextAccrualDate,
                ContractAmount = o.ContractAmount,
                TaxBaseAmount = o.TaxBaseAmount,
                TaxRate = o.TaxRate,
                ExpenseAccountId = o.ExpenseAccountId,
                ExpenseAccountNumber = o.ExpenseAccount == null ? null : o.ExpenseAccount.Number,
                ExpenseAccountName = o.ExpenseAccount == null ? null : o.ExpenseAccount.Name
            }).ToList()
        };
    }
}

public sealed class RentalContractListDtoProjection : IProjectionBuilder<RentalContract, RentalContractListDto>
{
    public Expression<Func<RentalContract, RentalContractListDto>> Build() => x => new()
    {
        Id = x.Id,
        ContractNumber = x.ContractNumber,
        ContractDate = x.ContractDate,
        LessorFullName = x.LessorFullName,
        LessorInn = x.LessorInn,
        LessorPinfl = x.LessorPinfl,
        StartDate = x.StartDate,
        EndDate = x.EndDate,
        CurrencyId = x.CurrencyId,
        CurrencyCode = x.Currency.Code,
        StatusId = x.StatusId,
        StatusName = x.Status.Name,
        ObjectCount = x.Objects.Count(o => o.StateId == StateIdConst.ACTIVE)
    };
}
