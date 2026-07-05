using SharedKernel.Results;

namespace Application.Features.Settings;

public interface ISettingService
{
    Task<Result<string?>> GetValueAsync(string code, CancellationToken ct = default);
    Task<Result<T>> GetValueAsync<T>(string code, CancellationToken ct = default);
    Task<Result<SettingDto>> GetAsync(string code, CancellationToken ct = default);
    Task<Result<List<SettingDto>>> GetAllAsync(string? category, CancellationToken ct = default);
    Task<Result> UpdateAsync(string code, string? value, CancellationToken ct = default);
}
