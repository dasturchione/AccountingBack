using Application.Abstractions;
using Microsoft.Extensions.Logging;
using SharedKernel.Results;

namespace Application.Features
{
    public abstract class BaseService
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private string ServiceName => GetType().Name;

        public BaseService(ILogger logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        // ─── Публичные перегрузки ────────────────────────────────────────────

        protected Task<Result<T>> ExecuteAsync<T>(string operationName, Func<Task<Result<T>>> operation)
            => ExecuteCoreAsync(operationName, operation, r => r.IsSuccess, r => r.Error);

        protected Task<Result> ExecuteAsync(string operationName, Func<Task<Result>> operation)
            => ExecuteCoreAsync(operationName, operation, r => r.IsSuccess, r => r.Error);

        protected Task<Result<T>> ExecuteInTransactionAsync<T>(string operationName, Func<Task<Result<T>>> operation, CancellationToken ct)
            => ExecuteCoreAsync(operationName, operation, r => r.IsSuccess, r => r.Error, ct);

        protected Task<Result> ExecuteInTransactionAsync(string operationName, Func<Task<Result>> operation, CancellationToken ct)
            => ExecuteCoreAsync(operationName, operation, r => r.IsSuccess, r => r.Error, ct);

        // ─── Вся логика здесь ────────────────────────────────────────────────

        private async Task<TResult> ExecuteCoreAsync<TResult>(
            string operationName,
            Func<Task<TResult>> operation,
            Func<TResult, bool> isSuccess,
            Func<TResult, Error> getError,
            CancellationToken? ct = null)
        {
            var fullName = $"{ServiceName}.{operationName}";
            _logger.LogInformation("Processing {Operation}", fullName);

            return ct.HasValue
                ? await ExecuteWithTransactionAsync(fullName, operation, isSuccess, getError, ct.Value)
                : await ExecuteWithoutTransactionAsync(fullName, operation, isSuccess, getError);
        }

        private async Task<TResult> ExecuteWithoutTransactionAsync<TResult>(
            string fullName,
            Func<Task<TResult>> operation,
            Func<TResult, bool> isSuccess,
            Func<TResult, Error> getError)
        {
            var result = await operation();
            LogResult(fullName, isSuccess(result), isSuccess(result) ? null : getError(result));
            return result;
        }

        private async Task<TResult> ExecuteWithTransactionAsync<TResult>(
            string fullName,
            Func<Task<TResult>> operation,
            Func<TResult, bool> isSuccess,
            Func<TResult, Error> getError,
            CancellationToken ct)
        {
            await _unitOfWork.BeginAsync(ct);
            try
            {
                var result = await operation();
                var success = isSuccess(result);

                if (success)
                    await _unitOfWork.CommitAsync(ct);
                else
                    await _unitOfWork.RollbackAsync(ct);

                LogResult(fullName, success, success ? null : getError(result));
                return result;
            }
            catch (Exception ex)
            {
                await TryRollbackAsync(fullName, ct);
                _logger.LogError(ex, "Exception in {Operation}", fullName);
                throw;
            }
        }

        private void LogResult(string fullName, bool success, Error? error)
        {
            if (success)
                _logger.LogInformation("Completed {Operation}", fullName);
            else if (error!.Type == ErrorType.NotFound)
                _logger.LogWarning("{Operation} failed: {Error}", fullName, error);
            else
                _logger.LogError("Failed {Operation}: {Error}", fullName, error);
        }

        private async Task TryRollbackAsync(string fullName, CancellationToken ct)
        {
            try
            {
                await _unitOfWork.RollbackAsync(ct);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Rollback failed in {Operation}", fullName);
            }
        }
    }
}
