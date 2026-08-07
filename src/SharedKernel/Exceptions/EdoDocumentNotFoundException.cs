namespace SharedKernel.Exceptions;

public sealed class EdoDocumentNotFoundException : InvalidOperationException
{
    public EdoDocumentNotFoundException()
        : base("The EDO document was not found in the current organization/provider scope.")
    {
    }
}
