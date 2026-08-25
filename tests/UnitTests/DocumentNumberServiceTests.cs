using Application.Features.DocumentNumbers;

public sealed class DocumentNumberServiceTests
{
    [Fact]
    public void ExistingInventoryAdjustmentNumberIsSkippedWhenSequenceIsMissing()
    {
        Assert.Equal(2, DocumentNumberService.GetNextNumber(maxExistingNumber: 1, sequenceLastNumber: null));
    }

    [Fact]
    public void ExistingInventoryAdjustmentNumberIsSkippedWhenSequenceIsStale()
    {
        Assert.Equal(2, DocumentNumberService.GetNextNumber(maxExistingNumber: 1, sequenceLastNumber: 1));
    }

    [Fact]
    public void HigherSequenceNumberRemainsTheBaseline()
    {
        Assert.Equal(11, DocumentNumberService.GetNextNumber(maxExistingNumber: 1, sequenceLastNumber: 10));
    }
}
