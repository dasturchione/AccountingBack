using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.BankOperations;

public static class BankOperationRelatedDocumentPolicy
{
    public static Result Validate(DocumentRegistry document, int organizationId, short? languageId = null)
    {
        if (document.OrganizationId != organizationId)
            return Result.Failure(BankOperationErrors.RelatedDocumentOrganizationMismatch(document.Id, languageId));

        if (document.StateId != StateIdConst.ACTIVE)
            return Result.Failure(BankOperationErrors.RelatedDocumentInactive(document.Id, languageId));

        return Result.Success();
    }
}
