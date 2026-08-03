namespace SharedKernel.Exceptions;

public sealed class EdoDocumentDirectionMismatchException : InvalidOperationException
{
    public EdoDocumentDirectionMismatchException()
        : base("The requested EDO document direction does not match the stored document.")
    {
    }
}
