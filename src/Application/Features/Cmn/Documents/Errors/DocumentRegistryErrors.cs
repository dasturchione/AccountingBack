using SharedKernel.Results;

namespace Application.Features.Cmn.Documents;

public static class DocumentRegistryErrors
{
    public static Error NotFound(long id) =>
        Error.NotFound("DocumentRegistry.NotFound", $"Document registry entry {id} was not found.");
}
