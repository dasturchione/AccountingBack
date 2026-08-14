namespace Application.Features.FaAssets
{
    public partial class FaAssetDto
    {
        public int? AssetAccountId { get; set; }
        public int? AccumulatedDepreciationAccountId { get; set; }
        public int? DepreciationExpenseAccountId { get; set; }
    }

    public partial class FaAssetListDto
    {
        public int? AssetAccountId { get; set; }
        public int? AccumulatedDepreciationAccountId { get; set; }
        public int? DepreciationExpenseAccountId { get; set; }
    }
}

namespace Application.Features.FaRevaluations
{
    public partial class FaRevaluationBaseDto
    {
        public int? RevaluationReserveAccountId { get; set; }
        public int? RevaluationLossAccountId { get; set; }
    }
public partial class FaRevaluationDto
    {
        public int? RevaluationReserveAccountId { get; set; }
        public int? RevaluationLossAccountId { get; set; }
    }

    public partial class FaRevaluationLineDto
    {
        public int? AssetAccountId { get; set; }
        public int? AccumulatedDepreciationAccountId { get; set; }
    }
}

namespace Application.Features.FaDisposals
{
    public partial class FaDisposalBaseDto
    {
        public int? DisposalAccountId { get; set; }
        public int? CustomerAccountId { get; set; }
        public int? VatAccountId { get; set; }
        public int? GainAccountId { get; set; }
        public int? LossAccountId { get; set; }
    }


    public partial class FaDisposalDto
    {
        public int? DisposalAccountId { get; set; }
        public int? CustomerAccountId { get; set; }
        public int? VatAccountId { get; set; }
        public int? GainAccountId { get; set; }
        public int? LossAccountId { get; set; }
    }

    public partial class FaDisposalLineDto
    {
        public int? AssetAccountId { get; set; }
        public int? AccumulatedDepreciationAccountId { get; set; }
    }
}

namespace Application.Features.FaDepreciations
{
    public partial class FaDepreciationRunLineDto
    {
        public int? ExpenseAccountId { get; set; }
        public int? AccumulatedDepreciationAccountId { get; set; }
    }
}
