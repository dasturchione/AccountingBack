using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.BankOperations;

public static class BankOperationRelatedDocumentPolicy
{
    public static Result Validate(DocumentRegistry document, int organizationId)
    {
        if (document.OrganizationId != organizationId)
            return Result.Failure(BankOperationErrors.RelatedDocumentOrganizationMismatch(document.Id));

        if (document.StateId != StateIdConst.ACTIVE)
            return Result.Failure(BankOperationErrors.RelatedDocumentInactive(document.Id));

        return Result.Success();
    }
}
