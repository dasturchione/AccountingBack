using SharedKernel.Results;

namespace Application.Features.Register.PostingEngines
{
    public class PostingContextDispatcher : IPostingContextDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public PostingContextDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<Result<List<PostingContext>>> ProcessAsync(object document, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(document);

            var resolver = ResolveBuilder(document);
            if (resolver is null)
                return Result.Failure<List<PostingContext>>(PostingContextErrors.UnsupportedDocumentType());

            var contexts = await InvokeBuilderAsync(resolver.Value.Builder, resolver.Value.DocumentType, document);
            return Result.Success(contexts);
        }

        private (object Builder, Type DocumentType)? ResolveBuilder(object document)
        {
            var documentType = document.GetType();

            while (documentType != typeof(object) && documentType != null)
            {
                var builderType = typeof(IPostingContextBuilder<>).MakeGenericType(documentType);
                var builder = _serviceProvider.GetService(builderType);

                if (builder is not null)
                    return (builder, documentType);

                documentType = documentType.BaseType;
            }

            return null;
        }

        private static async Task<List<PostingContext>> InvokeBuilderAsync(object builder, Type documentType, object document)
        {
            var buildMethod = builder.GetType().GetMethod(nameof(IPostingContextBuilder<object>.BuildAsync), new[] { documentType });
            if (buildMethod is null)
                throw new InvalidOperationException(
                    $"Построитель контекста '{builder.GetType().Name}' не содержит метод BuildAsync для '{documentType.Name}'.");

            var task = buildMethod.Invoke(builder, new[] { document }) as Task<List<PostingContext>>;
            if (task is null)
                throw new InvalidOperationException(
                    $"Построитель контекста '{builder.GetType().Name}' вернул неожиданный результат.");

            return await task;
        }
    }
}
