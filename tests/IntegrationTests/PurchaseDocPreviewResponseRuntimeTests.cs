using System.Text.Json;
using Application.Common.Pagination;
using Application.Features.PurchaseDocs;
using Microsoft.AspNetCore.Http.HttpResults;
using SharedKernel.Results;
using WebApi.Controllers;

namespace IntegrationTests;

public sealed class PurchaseDocPreviewResponseRuntimeTests
{
    [Fact]
    public async Task PreviewEndpointReturnsNormalizedPurchaseDocPreviewDto()
    {
        const string livePackageName = "ÑƒÑÐ»ÑƒÐ³Ð° (ÑÑƒÐ¼)";
        const string liveProductNamePrefix = "Ð‘ÐµÑÐ¿Ñ€Ð¾Ñ†ÐµÐ½Ñ‚Ð½Ñ‹Ðµ";
        var preview = new PurchaseDocPreviewDto
        {
            Lines =
            [
                new PurchaseDocPreviewLineDto
                {
                    PackageName = livePackageName,
                    ProductName = $"{liveProductNamePrefix} ..."
                }
            ]
        };
        var service = new PreviewServiceStub(preview);
        var controller = new PurchaseDocController(service);

        var result = await controller.PreviewAsync(new PurchaseDocPreviewRequestDto());

        var ok = Assert.IsType<Ok<PurchaseDocPreviewDto>>(result);
        var response = Assert.IsType<PurchaseDocPreviewDto>(ok.Value);
        var line = Assert.Single(response.Lines);
        Assert.Equal("услуга (сум)", line.PackageName);
        Assert.StartsWith("Беспроцентные", line.ProductName);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        using var document = JsonDocument.Parse(json);
        var serializedLine = document.RootElement.GetProperty("lines")[0];
        Assert.Equal("услуга (сум)", serializedLine.GetProperty("packageName").GetString());
        Assert.StartsWith("Беспроцентные", serializedLine.GetProperty("productName").GetString());
        Assert.Equal(1, service.PreviewCallCount);
        Assert.Equal(0, service.WriteCallCount);
    }

    private sealed class PreviewServiceStub(PurchaseDocPreviewDto preview) : IPurchaseDocService
    {
        public int PreviewCallCount { get; private set; }
        public int WriteCallCount { get; private set; }

        public Task<Result<PurchaseDocPreviewDto>> PreviewAsync(
            PurchaseDocPreviewRequestDto request,
            CancellationToken ct = default)
        {
            PreviewCallCount++;
            return Task.FromResult(Result.Success(preview));
        }

        public Task<Result<PagedResponse<PurchaseDocListDto>>> GetAllAsync(
            PurchaseDocListFilter filter,
            CancellationToken ct = default) => throw new NotSupportedException();

        public Task<Result<PurchaseDocDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<Result<PurchaseDocDto>> CreateFromEdoAsync(
            PurchaseDocFromEdoRequestDto request,
            CancellationToken ct = default)
        {
            WriteCallCount++;
            throw new NotSupportedException();
        }

        public Task<Result<long>> CreateAsync(PurchaseDocCreateDto dto, CancellationToken ct = default)
        {
            WriteCallCount++;
            throw new NotSupportedException();
        }

        public Task<Result> UpdateAsync(
            long id,
            PurchaseDocUpdateDto dto,
            CancellationToken ct = default)
        {
            WriteCallCount++;
            throw new NotSupportedException();
        }

        public Task<Result> ConfirmAsync(long id, CancellationToken ct = default)
        {
            WriteCallCount++;
            throw new NotSupportedException();
        }

        public Task<Result> CancelAsync(long id, CancellationToken ct = default)
        {
            WriteCallCount++;
            throw new NotSupportedException();
        }

        public Task<Result> DeleteAsync(long id, CancellationToken ct = default)
        {
            WriteCallCount++;
            throw new NotSupportedException();
        }
    }
}
