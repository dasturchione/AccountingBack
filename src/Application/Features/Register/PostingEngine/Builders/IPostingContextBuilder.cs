namespace Application.Features.Register.PostingEngine
{
    /// <summary>
    /// Строит PostingContext из конкретного документа (Sale, Purchase, Payment...).
    /// Один builder = один тип операции. PostingContextBuilderFactory выбирает нужный
    /// по типу документа, аналогично тому, как CompositeAccountResolver выбирал резолвер по алиасу.
    /// </summary>
    public interface IPostingContextBuilder<TDocument>
    {
        Task<List<PostingContext>> BuildAsync(TDocument document);
    }
}
