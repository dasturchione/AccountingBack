using SharedKernel.Filters;

namespace Application.Features.Acc.DocumentAccountSettings
{
    public class DocumentAccountSettingListFilter : IPaginationFilter
    {
        public int Page { get; set; } = 1;

        public int? PageSize { get; set; }
    }
}
