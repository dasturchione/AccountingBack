using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaDisposals;

public class FaDisposalListDtoProjection : IProjectionBuilder<FaDisposalDoc, FaDisposalListDto>
{
    public Expression<Func<FaDisposalDoc, FaDisposalListDto>> Build() =>
        x => new FaDisposalListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            DocNumber = x.DocNumber,
            DisposalDate = x.DisposalDate,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            DisposalTypeId = x.DisposalTypeId,
            Reason = x.Reason,
            DisposalAccountId = x.DisposalAccountId,
            CustomerAccountId = x.CustomerAccountId,
            VatAccountId = x.VatAccountId,
            GainAccountId = x.GainAccountId,
            LossAccountId = x.LossAccountId,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate,
            CreatedByUserId = x.CreatedByUserId,
            UpdatedDate = x.UpdatedDate,
            UpdatedByUserId = x.UpdatedByUserId,
            PostedAt = x.PostedAt,
            PostedByUserId = x.PostedByUserId,
            CancelledAt = x.CancelledAt,
            CancelledByUserId = x.CancelledByUserId,
            TotalBookValue = x.Lines.Sum(line => line.BookValue),
            TotalSaleAmount = x.Lines.Sum(line => line.SaleAmount),
            TotalGainLoss = x.Lines.Sum(line => line.GainLoss),
            Lines = new List<FaDisposalLineDto>()
        };
}
