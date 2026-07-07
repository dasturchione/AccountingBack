using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaDisposals;

public class FaDisposalDtoProjection : IProjectionBuilder<FaDisposalDoc, FaDisposalDto>
{
    public Expression<Func<FaDisposalDoc, FaDisposalDto>> Build() =>
        x => new FaDisposalDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            DocNumber = x.DocNumber,
            DisposalDate = x.DisposalDate,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            DisposalType = x.DisposalType,
            Reason = x.Reason,
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
            Lines = x.Lines.Select(line => new FaDisposalLineDto
            {
                Id = line.Id,
                DisposalDocId = line.DisposalDocId,
                FaAssetId = line.FaAssetId,
                InventoryNumber = line.FaAsset.InventoryNumber,
                AssetName = line.FaAsset.Name,
                BookValue = line.BookValue,
                SaleAmount = line.SaleAmount,
                GainLoss = line.GainLoss,
                Note = line.Note
            }).ToList()
        };
}
