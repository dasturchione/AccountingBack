using Application.Features.ChartAccounts;

namespace Application.Features.Acc.ChartAccounts
{
    public class ChartAccountGroupedListDto : ChartAccountListDto
    {
        public List<ChartAccountListDto> Lines { get; set; } = new();
    }
}
