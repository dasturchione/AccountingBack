using Application.Abstractions.Authentication;
using SharedKernel.Results;

namespace Application.Features.Register.PostingEngines
{
    public class PostingContextDispatcher : IPostingContextDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUserContext _userContext;

        public PostingContextDispatcher(IServiceProvider serviceProvider, IUserContext userContext)
        {
            _serviceProvider = serviceProvider;
            _userContext = userContext;
        }

        public async Task<Result<List<PostingContext>>> ProcessAsync(object document, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(document);

            var resolver = ResolveBuilder(document);
            if (resolver is null)
                return Result.Failure<List<PostingContext>>(PostingContextErrors.UnsupportedDocumentType(_userContext.LanguageId));

            var validator = ResolveValidator(resolver.Value.DocumentType);
            if (validator is not null)
            {
                var validation = await InvokeValidatorAsync(
                    validator,
                    resolver.Value.DocumentType,
                    document,
                    ct);
                if (!validation.IsSuccess)
                    return Result.Failure<List<PostingContext>>(validation.Error);
            }

            var contexts = await InvokeBuilderAsync(resolver.Value.Builder, resolver.Value.DocumentType, document);
            return Result.Success(contexts);
        }

        private object? ResolveValidator(Type documentType) =>
            _serviceProvider.GetService(typeof(IPostingContextValidator<>).MakeGenericType(documentType));

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

        private static async Task<Result> InvokeValidatorAsync(
            object validator,
            Type documentType,
            object document,
            CancellationToken ct)
        {
            var validateMethod = validator.GetType().GetMethod(
                nameof(IPostingContextValidator<object>.ValidateAsync),
                new[] { documentType, typeof(CancellationToken) });
            if (validateMethod is null)
                throw new InvalidOperationException(
                    $"Проверка контекста '{validator.GetType().Name}' не содержит метод ValidateAsync для '{documentType.Name}'.");

            var task = validateMethod.Invoke(validator, new object[] { document, ct }) as Task<Result>;
            if (task is null)
                throw new InvalidOperationException(
                    $"Проверка контекста '{validator.GetType().Name}' вернула неожиданный результат.");

            return await task;
        }
    }
}
