using SharedKernel.Filters;

namespace Application.Features.Users
{
    public class UserListFilter : ISearchFilter, IPaginationFilter
    {
        public int? RoleId { get; set; }

        public string? Search { get; set; }

        public int Page { get; set; }

        public int? PageSize { get; set; }
    }
}
