using Application.Abstractions;
using Microsoft.Extensions.Logging;
using SharedKernel.Results;

namespace Application.Features
{
    public abstract class BaseService(ILogger logger)
    {
        private string ServiceName => GetType().Name;

        protected async Task<Result<T>> ExecuteAsync<T>(string operationName, Func<Task<Result<T>>> operation)
        {
            var fullName = $"{ServiceName}.{operationName}";
            logger.LogInformation("Processing {Operation}", fullName);
            var result = await operation();
            if (result.IsSuccess)
                logger.LogInformation("Completed {Operation}", fullName);
            else if (result.Error.Type == ErrorType.NotFound)
                logger.LogWarning("{Operation} failed: {Error}", fullName, result.Error);
            else
                logger.LogError("Completed {Operation} with error: {Error}", fullName, result.Error);
            return result;
        }

        protected async Task<Result> ExecuteAsync(string operationName, Func<Task<Result>> operation)
        {
            var fullName = $"{ServiceName}.{operationName}";
            logger.LogInformation("Processing {Operation}", fullName);
            var result = await operation();
            if (result.IsSuccess)
                logger.LogInformation("Completed {Operation}", fullName);
            else if (result.Error.Type == ErrorType.NotFound)
                logger.LogWarning("{Operation} failed: {Error}", fullName, result.Error);
            else
                logger.LogError("Completed {Operation} with error: {Error}", fullName, result.Error);
            return result;
        }

        protected async Task<Result<T>> ExecuteAsync<T>(string operationName, IUnitOfWork unitOfWork, Func<Task<Result<T>>> operation, CancellationToken ct = default)
        {
            var fullName = $"{ServiceName}.{operationName}";
            logger.LogInformation("Processing {Operation}", fullName);
            await unitOfWork.BeginAsync(ct);
            try
            {
                var result = await operation();
                if (result.IsSuccess)
                {
                    await unitOfWork.CommitAsync(ct);
                    logger.LogInformation("Completed {Operation}", fullName);
                }
                else
                {
                    await unitOfWork.RollbackAsync(ct);

                    if (result.Error.Type == ErrorType.NotFound)
                        logger.LogWarning("{Operation} failed: {Error}", fullName, result.Error);
                    else
                        logger.LogError("Completed {Operation} with error: {Error}", fullName, result.Error);
                }
                return result;
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync(ct);
                logger.LogError(ex, "Exception in {Operation}", fullName);
                throw;
            }
        }

        protected async Task<Result> ExecuteAsync(string operationName, IUnitOfWork unitOfWork, Func<Task<Result>> operation, CancellationToken ct = default)
        {
            var fullName = $"{ServiceName}.{operationName}";
            logger.LogInformation("Processing {Operation}", fullName);
            await unitOfWork.BeginAsync(ct);
            try
            {
                var result = await operation();
                if (result.IsSuccess)
                {
                    await unitOfWork.CommitAsync(ct);
                    logger.LogInformation("Completed {Operation}", fullName);
                }
                else
                {
                    await unitOfWork.RollbackAsync(ct);

                    if (result.Error.Type == ErrorType.NotFound)
                        logger.LogWarning("{Operation} failed: {Error}", fullName, result.Error);
                    else 
                        logger.LogError("Completed {Operation} with error: {Error}", fullName, result.Error);
                }
                return result;
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync(ct);
                logger.LogError(ex, "Exception in {Operation}", fullName);
                throw;
            }
        }
    }
}
