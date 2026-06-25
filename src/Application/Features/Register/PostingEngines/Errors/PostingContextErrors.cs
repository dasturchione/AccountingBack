using SharedKernel.Results;

namespace Application.Features.Register.PostingEngines
{
    public static class PostingContextErrors
    {
        public static Error UnsupportedDocumentType() =>
            Error.Business("PostingContext.UnsupportedDocumentType", "Unsupported document type.");
    }
}
