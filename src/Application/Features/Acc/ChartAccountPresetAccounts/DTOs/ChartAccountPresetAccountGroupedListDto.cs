namespace Application.Features.ChartAccountPresetAccounts
{
    public class ChartAccountPresetAccountGroupedListDto : ChartAccountPresetAccountListDto
    {

        public List<ChartAccountPresetAccountListDto> Lines { get; set; } = new();
    }
}
