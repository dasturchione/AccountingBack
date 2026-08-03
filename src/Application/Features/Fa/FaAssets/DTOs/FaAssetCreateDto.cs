namespace Application.Features.FaAssets;

public class FaAssetCreateDto : FaAssetBaseDto
{
    public FaAssetProcessingMode ProcessingMode { get; set; } = FaAssetProcessingMode.StepByStep;
}

public enum FaAssetProcessingMode
{
    StepByStep = 1,
    Immediate = 2
}